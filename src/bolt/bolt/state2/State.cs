using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  public interface IState {

  }

  public interface IStateModifier {

  }

  public interface IStatePredictor {

  }

  public abstract class State : IEntitySerializer {
    public class Frame : IBoltListNode {
      public int Number;
      public object[] Objects;
      public readonly byte[] Data;
      public readonly List<int> ReadProperties;

      public Frame(int number, int size) {
        Number = number;
        Data = new byte[size];
        ReadProperties = new List<int>();
      }

      public Frame Duplicate(int frameNumber) {
        Frame f;

        f = new Frame(frameNumber, Data.Length);
        f.Objects = Objects;

        Array.Copy(Data, 0, f.Data, 0, Data.Length);

        return f;
      }

      object IBoltListNode.prev {
        get;
        set;
      }

      object IBoltListNode.next {
        get;
        set;
      }

      object IBoltListNode.list {
        get;
        set;
      }
    }

    internal EntityObject Entity;

    protected readonly int FrameSize;
    protected readonly int PropertyCount;
    protected readonly int PropertyIdBits;
    protected readonly object[] PropertyObjects;

    protected readonly int PacketMaxBits;
    protected readonly int PacketMaxProperties;
    protected readonly int PacketMaxPropertiesBits;

    protected readonly BoltDoubleList<Frame> Frames = new BoltDoubleList<Frame>();

    protected abstract BitArray GetDiffMask();
    protected abstract BitArray GetControllerMask();
    protected abstract BitArray GetFilterMask(Filter filter);

    protected abstract Priority[] GetTemporaryPriorityArray();

    protected abstract PropertySerializer[] GetPropertySerializersArray();

    protected State(int frameSize, int objectCount, int propertyCount, int packetMaxBits, int packetMaxProperties) {
      FrameSize = frameSize;
      PropertyCount = propertyCount;
      PropertyIdBits = BoltMath.BitsRequired(propertyCount - 1);
      PropertyObjects = new object[objectCount];
      PacketMaxBits = packetMaxBits;
      PacketMaxProperties = Math.Max(Math.Min(packetMaxProperties, 255), 1);
      PacketMaxPropertiesBits = BoltMath.BitsRequired(PacketMaxProperties);
    }

    internal void Pack(int frame, UdpConnection connection, UdpStream stream, Priority[] priority, List<Priority> written) {
      int ptr = stream.Ptr;
      int bits = 0;

      // reserve the space for our count
      stream.WriteByte(0, PacketMaxPropertiesBits);

      for (int i = 0; (i < priority.Length); ++i) {
        if ((bits + PropertyIdBits + 1) >= PacketMaxBits) {
          break;
        }

        if (written.Count == PacketMaxProperties) {
          break;
        }

        Priority p = priority[i];
        PropertySerializer s = GetPropertySerializersArray()[p.Property];

        if (p.Value == 0) {
          break;
        }

        int b = s.CalculateBits(Frames.first.Data);

        if ((bits + b) <= PacketMaxBits) {
          // write property id
          stream.WriteInt(p.Property, PropertyIdBits);

          // write data into stream
          s.Pack(Frames.first, connection, stream);

          // increment bits
          bits += b;

          // add to written list
          written.Add(p);

          // zero out priority (since we just sent it)
          priority[i].Value = 0;
        }
      }

      // gotta be less then 256
      Assert.True(written.Count <= PacketMaxProperties);

      // write the amount of properties
      UdpStream.WriteByteAt(stream.Data, ptr, PacketMaxPropertiesBits, (byte)written.Count);
    }

    internal void Read(int frameNumber, UdpConnection connection, UdpStream stream) {
      int count = stream.ReadByte(PacketMaxPropertiesBits);
      var frame = default(Frame);

      if (Frames.count == 0) {
        frame = AllocFrame(frameNumber);
      }
      else {
        frame = frame.Duplicate(frameNumber);
      }

      while (count > 0) {
        int property = stream.ReadInt(PropertyIdBits);
        var serializer = GetPropertySerializersArray()[property];

        // read data into frame
        serializer.Read(frame, connection, stream);

        // put property index into updated list
        frame.ReadProperties.Add(property);

        --count;
      }

      Frames.AddLast(frame);
    }

    internal BitArray CalculateDiff(byte[] a, byte[] b) {
      Assert.True(a != null);
      Assert.True(a.Length == FrameSize);

      Assert.True(b != null);
      Assert.True(b.Length == FrameSize);

      var array = GetDiffMask();
      var properties = GetPropertySerializersArray();

      array.Clear();

      for (int i = 0; i < properties.Length; ++i) {
        if (Blit.Diff(a, b, properties[i].ByteOffset, properties[i].ByteLength)) {
          array.Set(i);
        }
      }

      return array;
    }

    protected Frame AllocFrame(int number) {
      Frame f;

      f = new Frame(number, FrameSize);
      f.Objects = PropertyObjects;

      return f;
    }

    protected void FreeFrame(Frame frame) {

    }

    protected BitArray CalculateFilterPermutation(Filter filter, BitArray[] filters, Dictionary<Filter, BitArray> permutations) {
      if ((filter.Bits & 1) == 1) {
        return GetFullMask();
      }

      BitArray permutation;

      if (permutations.TryGetValue(filter, out permutation) == false) {
        permutation = BitArray.CreateClear(PropertyCount);

        for (int i = 0; i < 32; ++i) {
          long b = 1 << i;

          if ((filter.Bits & b) == b) {
            permutation.OrAssign(filters[i]);
          }
        }

        permutations.Add(filter, permutation);
      }

      return permutation;
    }

    #region Serializer

    public abstract TypeId TypeId { get; }
    public abstract BitArray GetFullMask();

    public virtual Filter GetDefaultFilter() {
      return new Filter(1);
    }

    public virtual float CalculatePriority(BoltConnection connection, BitArray mask, int skipped) {
      return skipped;
    }

    public virtual void OnRender() {
    }

    public virtual void OnCreate(EntityObject entity) {
      Entity = entity;
    }

    public virtual void OnSimulateBefore() {
    }

    public virtual void OnSimulateAfter() {
    }

    public virtual bool Pack(BoltConnection connection, UdpStream stream, EntityProxyEnvelope env) {
      BitArray filter = ResolveFilter(connection, env);
      PropertySerializer[] serializers = GetPropertySerializersArray();

      int tempCount = 0;

      Priority[] tempPriority = GetTemporaryPriorityArray();
      Priority[] proxyPriority = env.Proxy.PropertyPriority;

      for (int i = 0; i < proxyPriority.Length; ++i) {
        // if this property is set both in our filter and the proxy mask we can consider it for sending
        if (BitArray.SetInBoth(filter, env.Proxy.Mask, i)) {

          // increment priority for this property
          proxyPriority[i].Value += serializers[proxyPriority[i].Property].Priority;

          // copy to our temp array
          tempPriority[tempCount] = proxyPriority[i];

          // increment temp count
          tempCount += 1;
        }
      }

      // copy to temp array and sort it based on priority
      Array.Sort<Priority>(tempPriority, 0, tempCount, Priority.Comparer.Instance);
    }

    public virtual void Read(BoltConnection connection, UdpStream stream, int frame) {

    }

    public virtual BitArray ResolveFilter(BoltConnection connection, EntityProxyEnvelope env) {
      if (Entity.IsController(connection)) {
        return GetControllerMask();
      }

      return GetFilterMask(env.Proxy.Filter);
    }

    #endregion
  }
}

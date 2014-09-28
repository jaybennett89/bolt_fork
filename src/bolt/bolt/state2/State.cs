using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  public interface IState {

  }

  public interface IStateModifier : IDisposable {

  }

  public interface IStatePredictor : IDisposable {

  }

  internal abstract class State : IEntitySerializer {
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

    protected readonly Frame FrameDiffBuffer;
    protected readonly BoltDoubleList<Frame> Frames = new BoltDoubleList<Frame>();

    protected abstract Frame GetNullFrame();

    protected abstract BitArray GetFullMask();
    protected abstract BitArray GetDiffMask();
    protected abstract BitArray GetControllerMask();
    protected abstract BitArray GetFilterMask(Filter filter);

    protected abstract TypeId GetTypeId();
    protected abstract Priority[] GetTemporaryPriorityArray();
    protected abstract PropertySerializer[] GetPropertySerializersArray();

    protected State(int frameSize, int objectCount, int propertyCount, int packetMaxBits, int packetMaxProperties) {
      FrameSize = frameSize;
      FrameDiffBuffer = AllocFrame(-1);

      PropertyCount = propertyCount;
      PropertyIdBits = BoltMath.BitsRequired(propertyCount - 1);
      PropertyObjects = new object[objectCount];

      PacketMaxBits = packetMaxBits;
      PacketMaxProperties = Math.Max(Math.Min(packetMaxProperties, 255), 1);
      PacketMaxPropertiesBits = BoltMath.BitsRequired(PacketMaxProperties);
    }

    BitArray Diff(Frame a, Frame b) {
      BitArray array = GetDiffMask();
      PropertySerializer[] serializers = GetPropertySerializersArray();

      array.Clear();

      // calculate diff mask
      for (int i = 0; i < serializers.Length; ++i) {
        PropertySerializer s = serializers[i];

        if (Blit.Diff(a.Data, b.Data, s.ByteOffset, s.ByteLength)) {
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

    public TypeId TypeId {
      get { return GetTypeId(); }
    }

    public virtual float CalculatePriority(BoltConnection connection, BitArray mask, int skipped) {
      return skipped;
    }

    public BitArray GetDefaultMask() {
      // this calculates the mask for a new proxy
      return Diff(Frames.first, GetNullFrame()).Clone();
    }

    public void InitProxy(EntityProxy p) {
      p.PropertyPriority = new Priority[PropertyCount];
    }

    public void OnPrepareSend(BoltDoubleList<EntityProxy> proxy) {
      Assert.True(Entity.IsOwner);
      Assert.True(Frames.count > 0);

      // calculate diff mask
      BitArray diff = Diff(Frames.first, FrameDiffBuffer);

      // combine with existing masks for proxies
      var it = proxy.GetIterator();

      while (it.Next()) {
        it.val.Mask.OrAssign(diff);
      }

      // copy data from latest frame to diff buffer
      Array.Copy(Frames.first.Data, 0, FrameDiffBuffer.Data, 0, Frames.first.Data.Length);
    }

    public void OnRender() {
    }

    public void OnInitialized() {
      if (Entity.IsOwner) {
        Frames.AddLast(AllocFrame(BoltCore.frame));
      }
    }

    public void OnCreated(EntityObject entity) {
      Entity = entity;
    }

    public void OnSimulateBefore() {

    }

    public void OnSimulateAfter() {

    }

    public int Pack(BoltConnection connection, UdpStream stream, EntityProxyEnvelope env) {
      BitArray filter = GetFilter(connection, env.Proxy);
      PropertySerializer[] serializers = GetPropertySerializersArray();

      int tempCount = 0;

      Priority[] tempPriority = GetTemporaryPriorityArray();
      Priority[] proxyPriority = env.Proxy.PropertyPriority;

      for (int i = 0; i < proxyPriority.Length; ++i) {
        // if this property is set both in our filter and the proxy mask we can consider it for sending
        if (BitArray.SetInBoth(filter, env.Proxy.Mask, i)) {
          // increment priority for this property
          proxyPriority[i].PriorityValue += serializers[proxyPriority[i].PropertyIndex].Priority;

          // copy to our temp array
          tempPriority[tempCount] = proxyPriority[i];

          // increment temp count
          tempCount += 1;
        }
      }

      // sort temp array based on priority
      if (tempCount > 0) {
        Array.Sort<Priority>(tempPriority, 0, tempCount, Priority.Comparer.Instance);
      }

      // write into stream
      PackWrite(connection, stream, env, tempPriority, tempCount);

      for (int i = 0; i < env.Written.Count; ++i) {
        Priority p = env.Written[i];

        // clear priority for written property
        env.Proxy.PropertyPriority[p.PropertyIndex].PriorityValue = 0;

        // clear mask for it
        env.Proxy.Mask.Clear(p.PropertyIndex);
      }

      return env.Written.Count;
    }

    void PackWrite(BoltConnection connection, UdpStream stream, EntityProxyEnvelope env, Priority[] priority, int priorityCount) {
      int propertyCountPtr = stream.Ptr;
      stream.WriteByte(0, PacketMaxPropertiesBits);

      // how many bits can we write at the most
      int bits = Math.Min(PacketMaxBits, stream.Size - stream.Position);

      for (int i = 0; i < priorityCount; ++i) {

        // this means we can even fit another property id
        if (bits <= PropertyIdBits) {
          break;
        }

        // we have written enough properties
        if (env.Written.Count == PacketMaxProperties) {
          break;
        }

        Priority p = priority[i];
        PropertySerializer s = GetPropertySerializersArray()[p.PropertyIndex];

        if (p.PriorityValue == 0) {
          break;
        }

        int b = PropertyIdBits + s.CalculateBits(Frames.first.Data);
        int ptr = stream.Ptr;

        if (bits >= b) {
          // write property id
          stream.WriteInt(p.PropertyIndex, PropertyIdBits);

          // write data into stream
          s.Pack(Frames.first, connection, stream);

          // use up bits
          bits -= b;

          // add to written list
          env.Written.Add(p);
        }
      }

      // gotta be less then 256
      Assert.True(env.Written.Count <= PacketMaxProperties);

      // write the amount of properties
      UdpStream.WriteByteAt(stream.Data, propertyCountPtr, PacketMaxPropertiesBits, (byte)env.Written.Count);
    }

    public void Read(BoltConnection connection, UdpStream stream, int frameNumber) {
      int count = stream.ReadByte(PacketMaxPropertiesBits);
      var frame = default(Frame);

      if (Frames.count == 0) {
        frame = AllocFrame(frameNumber);
      }
      else {
        frame = Frames.last.Duplicate(frameNumber);
      }

      while (--count >= 0) {
        int property = stream.ReadInt(PropertyIdBits);
        var serializer = GetPropertySerializersArray()[property];

        // read data into frame
        serializer.Read(frame, connection, stream);

        // put property index into updated list
        frame.ReadProperties.Add(property);
      }

      Frames.AddLast(frame);
    }

    public BitArray GetFilter(BoltConnection connection, EntityProxy proxy) {
      if (Entity.IsController(connection)) {
        return GetControllerMask();
      }

      return GetFilterMask(proxy.Filter);
    }

    #endregion

  }
}

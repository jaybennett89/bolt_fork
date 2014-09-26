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

  internal abstract class State {
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
        Frame f = new Frame(frameNumber, Data.Length);
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

    internal int LocalId;
    internal int PrefabId;
    internal BoltUniqueId UniqueId;

    internal UE.Vector3 SpawnPosition;
    internal UE.Quaternion SpawnRotation;

    internal StateFlags Flags;
    internal BoltEntity Entity;
    internal BoltConnection SourceConnection;

    protected readonly int FrameSize;
    protected readonly int PropertyCount;
    protected readonly int PropertyIdBits;
    protected readonly object[] PropertyObjects;

    protected readonly int PacketMaxBits;
    protected readonly int PacketMaxProperties;
    protected readonly int PacketMaxPropertiesBits;

    protected readonly BoltDoubleList<Frame> Frames = new BoltDoubleList<Frame>();

    protected abstract BitArray GetDiffArray();
    protected abstract BitArray GetFullArray();
    protected abstract BitArray GetFilterArray(Filter filter);
    protected abstract BitArray GetControllerFilterArray();
    protected abstract PropertySerializer[] GetPropertyArray();

    protected State(int frameSize, int objectCount, int propertyCount, int packetMaxBits, int packetMaxProperties) {
      FrameSize = frameSize;
      PropertyCount = propertyCount;
      PropertyIdBits = BoltMath.BitsRequired(propertyCount - 1);
      PropertyObjects = new object[objectCount];
      PacketMaxBits = packetMaxBits;
      PacketMaxProperties = Math.Max(Math.Min(packetMaxProperties, 255), 1);
      PacketMaxPropertiesBits = BoltMath.BitsRequired(PacketMaxProperties);
    }

    internal void Pack(int frame, UdpConnection connection, UdpStream stream, PropertyPriority[] priority, List<PropertyPriority> written) {
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

        PropertyPriority p = priority[i];
        PropertySerializer s = GetPropertyArray()[p.Property];

        if (p.Priority == 0) {
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
          priority[i].Priority = 0;
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
        var serializer = GetPropertyArray()[property];

        // read data into frame
        serializer.Read(frame, connection, stream);

        // put property index into updated list
        frame.ReadProperties.Add(property);

        --count;
      }

      Frames.AddLast(frame);
    }

    internal BitArray GetFilter(Filter filter) {
      return GetFilterArray(filter);
    }

    internal BitArray CalculateDiff(byte[] a, byte[] b) {
      Assert.True(a != null);
      Assert.True(a.Length == FrameSize);

      Assert.True(b != null);
      Assert.True(b.Length == FrameSize);

      var array = GetDiffArray();
      var properties = GetPropertyArray();

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

  }
}

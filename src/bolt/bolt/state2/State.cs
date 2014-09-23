using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  public interface IState {

  }

  internal abstract class State {
    protected class Frame : IBoltListNode {
      public int Number;
      public readonly byte[] Data;

      public Frame(int number, int size) {
        Number = number;
        Data = new byte[size];
      }

      public Frame Duplicate() {
        Frame f = new Frame(Number, Data.Length);
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
    protected readonly int MaxBitsPerPacket;
    protected readonly BoltDoubleList<Frame> Frames = new BoltDoubleList<Frame>();

    protected abstract BitArray GetDiffArray();
    protected abstract BitArray GetFullArray();
    protected abstract BitArray GetFilterArray(Filter filter);
    protected abstract PropertySerializer[] GetPropertyArray();

    protected State(int frameSize, int propertyCount, int maxBitsPerPacket) {
      FrameSize = frameSize;
      PropertyCount = propertyCount;
      PropertyIdBits = BoltMath.BitsRequired(propertyCount - 1);
      MaxBitsPerPacket = maxBitsPerPacket;
    }

    internal void Pack(int frame, UdpConnection connection, UdpStream stream, PropertyPriority[] priority, List<PropertyPriority> written) {
      int bits = 0;
      PropertySerializer[] serializers = GetPropertyArray();

      for (int i = 0; (i < priority.Length); ++i) {
        if ((bits + PropertyIdBits + 1) >= MaxBitsPerPacket) {
          return;
        }

        PropertyPriority p = priority[i];
        PropertySerializer s = serializers[p.Property];

        if (p.Priority == 0) {
          return;
        }

        int b = s.CalculateBits(Frames.first.Data);

        if ((bits + b) <= MaxBitsPerPacket) {
          // write property id
          stream.WriteInt(p.Property, PropertyIdBits);

          // write data into stream
          s.Pack(frame, connection, stream, Frames.first.Data);

          // increment bits
          bits += b;

          // add to written list
          written.Add(p);

          // zero out priority (since we just sent it)
          priority[i].Priority = 0;
        }
      }
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
        if (Blit.Diff(a, b, properties[i].Offset, properties[i].Length)) {
          array.Set(i);
        }
      }

      return array;
    }

    protected Frame AllocFrame(int number) {
      return new Frame(number, FrameSize);
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

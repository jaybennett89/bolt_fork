using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UE = UnityEngine;

namespace Bolt {
  public interface IState {

  }

  public abstract class State {
    internal struct ByteMask {
      public long Mask;
      public int Index;

      public ByteMask(int index, int mask) {
        Index = index;
        Mask = mask; 
      }
    }

    protected class Frame : IBoltListNode {
      public int Number;
      public readonly byte[] Data;

      public Frame(int number, int size) {
        Number = number;
        Data = new byte[size];
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
    internal int NetworkId;
    internal BoltUniqueId UniqueId;

    internal UE.Vector3 SpawnPosition;
    internal UE.Quaternion SpawnRotation;

    internal StateFlags Flags;
    internal BoltEntity Entity;
    internal BoltConnection SourceConnection;

    protected readonly int FrameSize;
    protected readonly int StructCount;
    protected readonly BoltDoubleList<Frame> Frames;

    protected abstract void Diff(byte[] a, byte[] b, long[] mask);
    protected abstract long[] GetFilter(Filter filter);
    protected abstract long[] GetDiffMask();
    protected abstract long[] GetFullMask();
    protected abstract ByteMask[] GetByteMasks();

    protected State(int frameSize, int structCount) {
      Frames = new BoltDoubleList<Frame>();
      FrameSize = frameSize;
      StructCount = structCount;
      Flags = StateFlags.ZERO;
    }

    internal long[] CalculateDiff(byte[] a, byte[] b) {
      Assert.True(a != null);
      Assert.True(a.Length == FrameSize);

      Assert.True(b != null);
      Assert.True(b.Length == FrameSize);

      // setup vars
      int length = a.Length;
      long[] mask = GetDiffMask();
      ByteMask[] byteMap = GetByteMasks();

      // always zero out mask
      Array.Clear(mask, 0, mask.Length);

      // do unsafe fast compare
      unsafe {
        fixed (byte* ap = a)
        fixed (byte* bp = b) {
          for (int i = 0; i < length; ++i) {
            if (ap[i] != bp[i]) {
              mask[byteMap[i].Index] |= byteMap[i].Mask;
            }
          }
        }
      }

      return mask;
    }

    protected Frame AllocFrame(int number) {
      return new Frame(number, FrameSize);
    }

    protected void FreeFrame(Frame frame) {

    }

    protected long[] CalculateFilterPermutation(Filter filter, long[][] filters, Dictionary<Filter, long[]> permutations) {
      long[] permutation;

      if (permutations.TryGetValue(filter, out permutation) == false) {
        permutation = new long[StructCount];

        for (int i = 0; i < 32; ++i) {
          long b = 1 << i;

          if ((filter.Bits & b) == b) {
            for (int s = 0; s < StructCount; ++s) {
              permutation[s] |= filters[i][s];
            }
          }
        }

        permutations.Add(filter, permutation);
      }

      return permutation;
    }

  }
}

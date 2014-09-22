using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UE = UnityEngine;

namespace Bolt {
  public interface IState {

  }

  public abstract class State {
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

    internal long[] CompleteMask;
    internal long[][] FilterMask;
    internal Dictionary<Filter, long[]> FilterPermutations;

    protected State(int frameSize, int structCount, long[] complete, long[][] filters) {
      Frames = new BoltDoubleList<Frame>();
      FrameSize = frameSize;
      StructCount = structCount;
      Flags = StateFlags.ZERO;

      // all filters
      FilterMask = filters;
      FilterPermutations = new Dictionary<Filter, long[]>();

      // full mask
      CompleteMask = complete;

      // setup default filter permutations
      for (int i = 0; i < filters.Length; ++i) {
        if (filters[i] != null) {
          FilterPermutations.Add(new Filter(1 << i), filters[i]);
        }
      }
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

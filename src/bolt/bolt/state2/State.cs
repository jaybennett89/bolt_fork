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

    protected State(int frameSize, int structCount) {
      Frames = new BoltDoubleList<Frame>();
      FrameSize = frameSize;
      StructCount = structCount;
      Flags = StateFlags.ZERO;
    }

    protected Frame AllocFrame(int number) {
      return new Frame(number, FrameSize);
    }

    protected void FreeFrame(Frame frame) {

    }
  }
}

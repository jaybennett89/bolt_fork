using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  internal struct CommandMetaData {
    internal int InputByteSize;
    internal int ResultByteSize;
    internal int SmoothFrames;
    internal TypeId TypeId;
    internal PropertySerializer[] InputSerializers;
    internal PropertySerializer[] ResultSerializers;
  }

  public interface ICommandInput {

  }

  /// <summary>
  /// Base class that all commands inherit from
  /// </summary>
  [Documentation]
  public abstract class Command : IBoltListNode {
    internal const int SEQ_BITS = 8;
    internal const int SEQ_SHIFT = 16 - SEQ_BITS;
    internal const int SEQ_MASK = (1 << SEQ_BITS) - 1;

    internal byte[] InputData;
    internal byte[] ResultData;

    internal int Frame;
    internal ushort Sequence;

    internal int SmoothStart;
    internal int SmoothEnd;

    internal byte[] SmoothFrom;
    internal byte[] SmoothTo;

    internal CommandFlags Flags;
    internal CommandMetaData Meta;

    /// <summary>
    /// The value of the BoltNetwork.serverFrame property of the computer this command was created on
    /// </summary>
    public int ServerFrame {
      get { return Frame; }
    }

    /// <summary>
    /// Returns true if it's the first time this command executed
    /// </summary>
    public bool IsFirstExecution {
      get { return !(Flags & CommandFlags.HAS_EXECUTED); }
    }

    /// <summary>
    /// User assignable token that lets you pair arbitrary data with the command, this is not replicated over the network to any remote computers.
    /// </summary>
    public object UserToken {
      get;
      set;
    }

    object IBoltListNode.prev { get; set; }
    object IBoltListNode.next { get; set; }
    object IBoltListNode.list { get; set; }

    internal Command(CommandMetaData meta) {
      Meta = meta;
      InputData = new byte[meta.InputByteSize];
      ResultData = new byte[meta.ResultByteSize];
    }

    internal void VerifyCanSetInput() {
      if (Flags & CommandFlags.HAS_EXECUTED) {
        throw new BoltException("You can not change the input of a command after it has executed");
      }
    }

    internal void VerifyCanSetResult() {
      if (Flags & CommandFlags.CORRECTION_RECEIVED) {
        throw new BoltException("You can not change the result of a command after it has been corrected");
      }
    }

    internal Command Clone() {
      Command clone = (Command) MemberwiseClone();

      ((IBoltListNode)clone).list = null;
      ((IBoltListNode)clone).prev = null;
      ((IBoltListNode)clone).next = null;

      clone.InputData = clone.InputData.CloneArray();
      clone.ResultData = clone.ResultData.CloneArray();

      return clone;
    }

    internal void PackInput(BoltConnection connection, UdpPacket stream) {
      for (int i = 0; i < Meta.InputSerializers.Length; ++i) {
        Meta.InputSerializers[i].CommandPack(this, InputData, connection, stream);
      }
    }

    internal void ReadInput(BoltConnection connection, UdpPacket stream) {
      for (int i = 0; i < Meta.InputSerializers.Length; ++i) {
        Meta.InputSerializers[i].CommandRead(this, InputData, connection, stream);
      }
    }

    internal void PackResult(BoltConnection connection, UdpPacket stream) {
      for (int i = 0; i < Meta.ResultSerializers.Length; ++i) {
        Meta.ResultSerializers[i].CommandPack(this, ResultData, connection, stream);
      }
    }

    internal void ReadResult(BoltConnection connection, byte[] array, UdpPacket stream) {
      for (int i = 0; i < Meta.ResultSerializers.Length; ++i) {
        Meta.ResultSerializers[i].CommandRead(this, array, connection, stream);
      }
    }

    internal void SmoothCorrection() {
      if (SmoothFrom != null && SmoothTo != null) {
        float max = SmoothEnd - SmoothStart;
        float current = BoltCore.frame - SmoothStart;
        float t = UE.Mathf.Clamp01(current / max);

        for (int i = 0; i < Meta.ResultSerializers.Length; ++i) {
          Meta.ResultSerializers[i].CommandSmooth(SmoothFrom, SmoothTo, this.ResultData, t);
        }
      }
    }

    internal void Free() {

    }

    public static implicit operator bool(Command cmd) {
      return cmd != null;
    }
  }
}

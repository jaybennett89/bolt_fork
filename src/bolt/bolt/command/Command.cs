using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  public abstract class NetworkCommand_Data : NetworkObj {
    public IProtocolToken Token {
      get;
      set;
    }

    internal Command RootCommand
    {
      get { return (Command) Root; }
    }

    internal NetworkCommand_Data(NetworkObj_Meta meta)
      : base(meta) {
    }
  }

  internal abstract class Command_Meta : NetworkObj_Meta
  {
    internal int SmoothFrames;
  }

  /// <summary>
  /// Base class that all commands inherit from
  /// </summary>
  [Documentation]
  public abstract class Command : NetworkObj, IBoltListNode {
    internal const int SEQ_BITS = 8;
    internal const int SEQ_SHIFT = 16 - SEQ_BITS;
    internal const int SEQ_MASK = (1 << SEQ_BITS) - 1;

    internal new Command_Meta Meta;

    internal NetworkCommand_Data Input;
    internal NetworkCommand_Data Result;

    internal int SmoothFrameFrom;
    internal int SmoothFrameTo;

    internal NetworkStorage SmoothStorageFrom;
    internal NetworkStorage SmoothStorageTo;

    internal ushort Sequence;
    internal CommandFlags Flags;

    /// <summary>
    /// The value of the BoltNetwork.serverFrame property of the computer this command was created on
    /// </summary>
    public int ServerFrame
    {
      get; 
      internal set; 
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

    internal Command(Command_Meta meta) : base(meta)
    {
      Meta = meta;
    }

    internal void VerifyCanSetInput() {
      if (Flags & CommandFlags.HAS_EXECUTED) {
        throw new BoltException("You can not change the Data of a command after it has executed");
      }
    }

    internal void VerifyCanSetResult() {
      if (Flags & CommandFlags.CORRECTION_RECEIVED) {
        throw new BoltException("You can not change the Data of a command after it has been corrected");
      }
    }

    internal void PackInput(BoltConnection connection, UdpPacket packet) {
      for (int i = 0; i < Input.Meta.Properties.Length; ++i) {
        Input.Meta.Properties[i].Property.Write(connection, Input, Storage, packet);
      }
    }

    internal void ReadInput(BoltConnection connection, UdpPacket packet) {
      for (int i = 0; i < Input.Meta.Properties.Length; ++i) {
        Input.Meta.Properties[i].Property.Read(connection, Input, Storage, packet);
      }
    }

    internal void PackResult(BoltConnection connection, UdpPacket packet) {
      for (int i = 0; i < Result.Meta.Properties.Length; ++i) {
        Result.Meta.Properties[i].Property.Write(connection, Result, Storage, packet);
      }
    }

    internal void ReadResult(BoltConnection connection, UdpPacket packet)
    {
      for (int i = 0; i < Result.Meta.Properties.Length; ++i) {
        Result.Meta.Properties[i].Property.Write(connection, Result, SmoothStorageTo ?? Storage, packet);
      }
    }

    internal void BeginSmoothing() {
      SmoothStorageFrom = DuplicateStorage(Storage);
      SmoothStorageTo = DuplicateStorage(Storage);

      SmoothFrameFrom = BoltCore.frame;
      SmoothFrameTo = SmoothFrameFrom + Meta.SmoothFrames;
    }

    internal void SmoothCorrection()
    {
      if (SmoothStorageFrom != null && SmoothStorageTo != null) 
      {
        float max = SmoothFrameTo - SmoothFrameFrom;
        float current = BoltCore.frame - SmoothFrameFrom;
        float t = UE.Mathf.Clamp01(current / max);

        for (int i = 0; i < Result.Meta.Properties.Length; ++i)
        {
          Result.Meta.Properties[i].Property.SmoothCommandCorrection(Result, SmoothStorageFrom, SmoothStorageTo, Storage, t);
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

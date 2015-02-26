using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  public interface INetworkCommandData {
    IProtocolToken Token {
      get;
      set;
    }
  }

  internal abstract class NetworkCommand_Data : NetworkObj, INetworkCommandData {
    public IProtocolToken Token {
      get;
      set;
    }

    internal Command RootCommand {
      get { return (Command)Root; }
    }

    IProtocolToken INetworkCommandData.Token {
      get { return this.Token; }
      set { this.Token = value; }
    }

    internal NetworkCommand_Data(NetworkObj_Meta meta)
      : base(meta) {
    }

  }

  internal abstract class Command_Meta : NetworkObj_Meta {
    internal int SmoothFrames;
    internal bool CompressZeroValues;
  }

  /// <summary>
  /// Base class that all commands inherit from
  /// </summary>
  [Documentation]
  public abstract class Command : NetworkObj_Root, IBoltListNode {
    internal const int SEQ_BITS = 8;
    internal const int SEQ_SHIFT = 16 - SEQ_BITS;
    internal const int SEQ_MASK = (1 << SEQ_BITS) - 1;

    NetworkStorage storage;

    internal new Command_Meta Meta;

    internal override NetworkStorage Storage {
      get { return storage; }
    }

    internal NetworkCommand_Data InputObject {
      get { return (NetworkCommand_Data)Objects[1]; }
    }

    internal NetworkCommand_Data ResultObject {
      get { return (NetworkCommand_Data)Objects[2]; }
    }

    internal int SmoothFrameFrom;
    internal int SmoothFrameTo;

    internal NetworkStorage SmoothStorageFrom;
    internal NetworkStorage SmoothStorageTo;

    internal ushort Sequence;
    internal CommandFlags Flags;

    /// <summary>
    /// The value of the BoltNetwork.serverFrame property of the computer this command was created on
    /// </summary>
    public int ServerFrame {
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

    internal Command(Command_Meta meta)
      : base(meta) {
      Meta = meta;
      storage = AllocateStorage();
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
      for (int i = 0; i < InputObject.Meta.Properties.Length; ++i) {
        InputObject.Meta.Properties[i].Property.Write(connection, InputObject, Storage, packet);
      }
    }

    internal void ReadInput(BoltConnection connection, UdpPacket packet) {
      for (int i = 0; i < InputObject.Meta.Properties.Length; ++i) {
        InputObject.Meta.Properties[i].Property.Read(connection, InputObject, Storage, packet);
      }
    }

    internal void PackResult(BoltConnection connection, UdpPacket packet) {
      for (int i = 0; i < ResultObject.Meta.Properties.Length; ++i) {
        ResultObject.Meta.Properties[i].Property.Write(connection, ResultObject, Storage, packet);
      }
    }

    internal void ReadResult(BoltConnection connection, UdpPacket packet) {
      for (int i = 0; i < ResultObject.Meta.Properties.Length; ++i) {
        ResultObject.Meta.Properties[i].Property.Read(connection, ResultObject, SmoothStorageTo ?? Storage, packet);
      }
    }

    internal void BeginSmoothing() {
      SmoothStorageFrom = DuplicateStorage(Storage);
      SmoothStorageTo = DuplicateStorage(Storage);

      SmoothFrameFrom = BoltCore.frame;
      SmoothFrameTo = SmoothFrameFrom + Meta.SmoothFrames;
    }

    internal void SmoothCorrection() {
      if (SmoothStorageFrom != null && SmoothStorageTo != null) {
        float max = SmoothFrameTo - SmoothFrameFrom;
        float current = BoltCore.frame - SmoothFrameFrom;
        float t = UE.Mathf.Clamp01(current / max);

        for (int i = 0; i < ResultObject.Meta.Properties.Length; ++i) {
          ResultObject.Meta.Properties[i].Property.SmoothCommandCorrection(ResultObject, SmoothStorageFrom, SmoothStorageTo, Storage, t);
        }
      }
    }

    internal void Free() {
      FreeStorage(storage);
    }

    public static implicit operator bool(Command cmd) {
      return cmd != null;
    }
  }
}
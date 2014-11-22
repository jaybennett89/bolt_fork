using System;
using UdpKit;

namespace Bolt {
  /// <summary>
  /// Base class that all events inherit from
  /// </summary>
  [Documentation]
  public abstract class NetworkEvent : NetworkObj, IDisposable {
    internal const byte ENTITY_EVERYONE = 1;
    internal const byte ENTITY_EVERYONE_EXCEPT_OWNER = 3;
    internal const byte ENTITY_EVERYONE_EXCEPT_CONTROLLER = 5;
    internal const byte ENTITY_ONLY_CONTROLLER = 7;
    internal const byte ENTITY_ONLY_OWNER = 9;
    internal const byte ENTITY_ONLY_SELF = 11;

    internal const byte GLOBAL_EVERYONE = 2;
    internal const byte GLOBAL_OTHERS = 4;
    internal const byte GLOBAL_ONLY_SERVER = 6;
    internal const byte GLOBAL_ALL_CLIENTS = 8;
    internal const byte GLOBAL_SPECIFIC_CONNECTION = 10;
    internal const byte GLOBAL_ONLY_SELF = 12;

    internal const int RELIABLE_WINDOW_BITS = 10;
    internal const int RELIABLE_SEQUENCE_BITS = 12;

    NetworkStorage storage;

    internal uint Sequence;
    internal ReliabilityModes Reliability;

    internal int Targets;
    internal bool Reliable;
    internal Entity TargetEntity;
    internal BoltConnection TargetConnection;
    internal BoltConnection SourceConnection;

    internal new NetworkEvent_Meta Meta;

    public bool IsFromLocalComputer {
      get { return ReferenceEquals(SourceConnection, null); }
    }

    public BoltConnection RaisedBy {
      get { return SourceConnection; }
    }

    public bool IsGlobalEvent {
      get { return !IsEntityEvent; }
    }

    internal override NetworkStorage Storage
    {
      get { return storage; } 
    }

    public byte[] BinaryData
    {
      get; 
      set; 
    }

    internal bool IsEntityEvent {
      get {
        return
          Targets == ENTITY_EVERYONE ||
          Targets == ENTITY_EVERYONE_EXCEPT_OWNER ||
          Targets == ENTITY_EVERYONE_EXCEPT_CONTROLLER ||
          Targets == ENTITY_ONLY_CONTROLLER ||
          Targets == ENTITY_ONLY_SELF ||
          Targets == ENTITY_ONLY_OWNER;
      }
    }

    internal NetworkEvent(NetworkEvent_Meta meta) : base(meta)
    {
      Meta = meta;
      storage = AllocateStorage();
    }

    internal void IncrementRefs() {

    }

    internal bool Pack(BoltConnection connection, UdpPacket packet) {
      for (int i = 0; i < Meta.Properties.Length; ++i) {
        if (Meta.Properties[i].Property.Write(connection, this, storage, packet)) {
          return false;
        }
      }

      return true;
    }

    internal void Read(BoltConnection connection, UdpPacket packet) {
      for (int i = 0; i < Meta.Properties.Length; ++i) {
        Meta.Properties[i].Property.Read(connection, this, storage, packet);
      }
    }

    public void Send() {
      EventDispatcher.Enqueue(this);
    }

    [Obsolete("The using(var ev = ...) syntax is deprecated. Use the .Send method directly instead")]
    void IDisposable.Dispose() {
      Send();
    }

    internal void DecrementRefs() {
    
    }
  }
}

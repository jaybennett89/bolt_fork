using System;
using UdpKit;

namespace Bolt {
  internal struct EventMetaData {
    internal TypeId TypeId;
    internal int ByteSize;
    internal PropertySerializer[] PropertySerializers;
  }

  /// <summary>
  /// Base class that all events inherit from
  /// </summary>
  [Documentation]
  public abstract class Event : IDisposable {

    internal const byte ENTITY_EVERYONE = 1;
    internal const byte ENTITY_EVERYONE_EXCEPT_CONTROLLER = 3;
    internal const byte ENTITY_ONLY_CONTROLLER = 5;
    internal const byte ENTITY_ONLY_OWNER = 7;

    internal const byte GLOBAL_EVERYONE = 2;
    internal const byte GLOBAL_OTHERS = 4;
    internal const byte GLOBAL_SERVER = 6;
    internal const byte GLOBAL_ALL_CLIENTS = 8;
    internal const byte GLOBAL_SPECIFIC_CONNECTION = 10;

    internal const int RELIABLE_WINDOW_BITS = 10;
    internal const int RELIABLE_SEQUENCE_BITS = 12;

    int refs;

    internal EventMetaData Meta;

    internal uint Sequence;
    internal byte[] Data;

    internal int Targets;
    internal bool Reliable;
    internal Entity TargetEntity;
    internal BoltConnection TargetConnection;
    internal BoltConnection SourceConnection;

    public bool IsFromLocalComputer {
      get { return ReferenceEquals(SourceConnection, null); }
    }

    public BoltConnection RaisedBy {
      get { return SourceConnection; }
    }

    public bool IsGlobalEvent {
      get { return !IsEntityEvent; }
    }

    internal bool IsEntityEvent {
      get {
        VerifyIsActive();
        return Targets == ENTITY_EVERYONE || Targets == ENTITY_EVERYONE_EXCEPT_CONTROLLER;
      }
    }

    internal Event Clone() {
      Event clone;

      clone = (Event)MemberwiseClone();
      clone.Data = new byte[Data.Length];

      Array.Copy(Data, 0, clone.Data, 0, Data.Length);

      return clone;
    }

    internal Event(EventMetaData meta) {
      Meta = meta;
      Data = new byte[Meta.ByteSize];
    }

    internal void IncrementRefs() {
      refs += 1;
    }

    internal bool Pack(BoltConnection connection, UdpStream stream) {
      for (int i = 0; i < Meta.PropertySerializers.Length; ++i) {
        if (Meta.PropertySerializers[i].EventPack(this, connection, stream) == false) {
          return false;
        }
      }

      return true;
    }

    internal void Read(BoltConnection connection, UdpStream stream) {
      for (int i = 0; i < Meta.PropertySerializers.Length; ++i) {
        Meta.PropertySerializers[i].EventRead(this, connection, stream);
      }
    }

    [Obsolete("Use Event.Send instead")]
    public void Commit() {
      EventDispatcher.Enqueue(this);
    }

    /// <summary>
    /// Send this event
    /// </summary>
    public void Send() {
      EventDispatcher.Enqueue(this);
    }

    void IDisposable.Dispose() {
      Commit();
    }

    void VerifyIsActive() {
      Assert.True(refs > 0);
    }

    internal void DecrementRefs() {
      VerifyIsActive();

      if (--refs == 0) {

      }
    }

  }
}

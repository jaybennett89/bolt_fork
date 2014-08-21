using System;
using System.Collections;
using System.Collections.Generic;
using UdpKit;

internal abstract class BoltEventS2C : BoltEventBase {
  protected internal BoltEventS2C (ushort id)
    : base(id, false, BoltEventDeliveryMode.Reliable) {
  }

  public override bool FilterSend (BoltConnection cn) {
    if (ReferenceEquals(cn, _connection)) return false;
    return cn.udpConnection.IsServer;
  }

  public override bool FilterInvoke () {
    return BoltCore.isClient;
  }
}

internal abstract class BoltEventC2S : BoltEventBase {
  protected internal BoltEventC2S (ushort id)
    : base(id, false, BoltEventDeliveryMode.Reliable) {
  }

  public override bool FilterSend (BoltConnection cn) {
    if (ReferenceEquals(cn, _connection)) return false;
    return cn.udpConnection.IsClient;
  }

  public override bool FilterInvoke () {
    return BoltCore.isServer;
  }
}

internal abstract class BoltEventANY : BoltEventBase {
  protected internal BoltEventANY (ushort id)
    : base(id, false, BoltEventDeliveryMode.Reliable) {
  }

  public override bool FilterSend (BoltConnection cn) {
    if (ReferenceEquals(cn, _connection)) return false;
    return true;
  }

  public override bool FilterInvoke () {
    return true;
  }
}

/// <summary>
/// Base class for all events
/// </summary>
public abstract class BoltEventBase : BoltObject, IDisposable, IBoltEvent {
  internal const int USER_START_ID = 16;
  internal const int RELIABLE_WINDOW_BITS = 6;
  internal const int RELIABLE_SEQUENCE_BITS = 8;

  internal readonly ushort _id;
  internal readonly bool _isEntityEvent;

  internal int _frame;
  internal int _refCount;
  internal uint _entityNetworkId;
  internal bool _entityIsOutgoing;

  internal BoltEntity _entity;
  internal BoltConnection _connection;
  internal BoltEventDeliveryMode _deliveryMode;

  protected internal BoltEventBase (ushort id, bool entity, BoltEventDeliveryMode mode) {
    _id = id;
    _isEntityEvent = entity;
    _deliveryMode = mode;
  }

  /// <summary>
  /// Called for packing this event
  /// </summary>
  /// <param name="stream">The stream we are writing to</param>
  /// <param name="connection">The connection the stream will be sent to</param>
  public abstract void Pack (UdpStream stream, BoltConnection connection);

  /// <summary>
  /// Called for reading this event
  /// </summary>
  /// <param name="stream">The stream we are reading from</param>
  /// <param name="connection">The connection the stream was received from</param>
  public abstract void Read (UdpStream stream, BoltConnection connection);

  /// <summary>
  /// Called when the event is ready to be recycled, allows
  /// freeing of resources.
  /// </summary>
  public abstract void Free ();

  public virtual BoltEventBase Clone () {
    return (BoltEventBase) MemberwiseClone();
  }

  /// <summary>
  /// Called to check if we should send the event to a connection
  /// </summary>
  /// <param name="cn">The connection we are testing</param>
  public virtual bool FilterSend (BoltConnection cn) {
    return true;
  }

  /// <summary>
  /// Called to check if we should receive an event from a connection
  /// </summary>
  /// <param name="cn">The connection we are testing</param>
  public virtual bool FilterRecv (BoltConnection cn) {
    return true;
  }

  /// <summary>
  /// Called to check if we should invoke this event on our local machine
  /// </summary>
  public virtual bool FilterInvoke () {
    return true;
  }

  public override string ToString () {
    return string.Format("[Event type={0} mode={1}]", GetType().Name, _deliveryMode);
  }

  public void Dispose () {
    Assert.True(_refCount > 0);
    _refCount -= 1;

    if (_refCount == 0) {
      Free();
    }
  }

  internal void GrabEntity (BoltConnection connection) {
    if (_isEntityEvent) {
      if (_entityIsOutgoing) {
        _entity = connection.GetOutgoingEntity(_entityNetworkId);
      } else {
        _entity = connection.GetIncommingEntity(_entityNetworkId);
      }
    }
  }

  internal void RefCountIncrement () {
    _refCount += 1;
  }

  internal static void Invoke (BoltEventBase evnt) {
    Invoke(evnt, BoltCore._connections.GetIterator());
  }

  internal static void Invoke (BoltEventBase evnt, BoltIterator<BoltConnection> connections) {
    Assert.True(evnt._refCount > 0);

    if (evnt.FilterInvoke()) {
      BoltLog.Info("invoking {0}", evnt);
      Call(evnt);
    }

    while (connections.Next()) {
      connections.val.Raise(evnt);
    }

    evnt.Dispose();
  }

  internal static void Invoke (BoltEventBase evnt, IEnumerable connections) {
    Assert.True(evnt._refCount > 0);

    if (evnt.FilterInvoke()) {
      BoltLog.Info("invoking {0}", evnt);
      Call(evnt);
    }

    foreach (BoltConnection cn in connections) {
      cn.Raise(evnt);
    }

    evnt.Dispose();
  }

  static void Call (BoltEventBase evnt) {
    Assert.True(evnt._refCount > 0);
    IBoltEventFactory handler = BoltFactory.GetEventFactory(evnt._id);

    if (evnt._isEntityEvent) {
      if (evnt._entity) {
        evnt._entity._eventDispatcher.Dispatch(evnt, handler);
      }
    } else {
      BoltCore._eventDispatcher.Dispatch(evnt, handler);
    }
  }
}
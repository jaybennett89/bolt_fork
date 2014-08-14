using System;
using UdpKit;

public interface ITeleport {
  BoltEntity target { get; set; }
}

public interface ITeleportReceiver {
  void OnEvent (ITeleport evnt, BoltConnection connection);
}

class Teleport : BoltEvent, ITeleport {
  public const ushort ID = 2;

  public BoltEntity target {
    get;
    set;
  }

  internal Teleport ()
    : base(ID, false, BoltEventDeliveryMode.UnreliableFrameSynced) {
  }

  public override bool FilterSend (BoltConnection cn) {
    if (ReferenceEquals(cn, _connection)) {
      return false;
    }

    return cn._entityChannel.MightExistOnRemote(target);
  }

  public override bool FilterInvoke () {
    return true;
  }

  public override void Pack (UdpStream stream, BoltConnection connection) {
    stream.WriteEntity(target, connection);
  }

  public override void Read (UdpStream stream, BoltConnection connection) {
    target = stream.ReadEntity(connection);
  }

  public override void Free () {
    target = null;
  }
}

class TeleportFactory : IBoltEventFactory {
  public Type eventType { get { return typeof(ITeleport); } }
  public ushort eventId { get { return Teleport.ID; } }

  public object Create () {
    return new Teleport();
  }

  public void Dispatch (object @event, object target) {
    Teleport evnt = (Teleport) @event;
    ITeleportReceiver receiver = target as ITeleportReceiver;

    if (receiver != null) {
      receiver.OnEvent(evnt, evnt._connection);
    }
  }
}



using System;
using UdpKit;

internal interface ILoadMapDone : IBoltEvent {
  Map map { get; set; }
}

internal interface ILoadMapDoneReceiver {
  void OnEvent (ILoadMapDone evnt, BoltConnection connection);
}

class LoadMapDone : BoltEventANY, ILoadMapDone {
  public const ushort ID = 1;

  Map _map;

  public Map map {
    get { return _map; }
    set { _map = value; }
  }

  internal LoadMapDone ()
    : base(ID) {
  }

  public override void Pack (UdpStream stream, BoltConnection cn) {
    stream.WriteString(_map.name);
    stream.WriteInt(_map.token);
  }

  public override void Read (UdpStream stream, BoltConnection cn) {
    _map = new Map(stream.ReadString(), stream.ReadInt());
  }

  public override void Free () {
    _map = default(Map);
  }
}

class LoadMapDoneFactory : IBoltEventFactory {
  public Type eventType { get { return typeof(ILoadMapDone); } }
  public ushort eventId { get { return LoadMapDone.ID; } }

  public object Create () {
    return new LoadMapDone();
  }

  public void Dispatch (object @event, object target) {
    LoadMapDone evnt = (LoadMapDone) @event;
    ILoadMapDoneReceiver receiver = target as ILoadMapDoneReceiver;

    if (receiver != null) {
      receiver.OnEvent(evnt, evnt._connection);
    }
  }
}



using System;
using UdpKit;

internal interface ILoadMap : IBoltEvent {
  Scene map { get; set; }
}

internal interface ILoadMapReceiver {
  void OnEvent (ILoadMap evnt, BoltConnection connection);
}

class LoadMap : BoltEventS2C, ILoadMap {
  public const ushort ID = 0;

  Scene _map;

  public Scene map {
    get { return _map; }
    set { _map = value; }
  }

  internal LoadMap ()
    : base(ID) {
  }

  public override void Pack (UdpStream stream, BoltConnection cn) {
    stream.WriteString(_map.name);
    stream.WriteInt(_map.token);
  }

  public override void Read (UdpStream stream, BoltConnection cn) {
    _map = new Scene(stream.ReadString(), stream.ReadInt());
  }

  public override void Free () {
    _map = new Scene();
  }
}

class LoadMapFactory : IBoltEventFactory {
  public Type eventType { get { return typeof(ILoadMap); } }
  public ushort eventId { get { return LoadMap.ID; } }

  public object Create () {
    return new LoadMap();
  }

  public void Dispatch (object @event, object target) {
    LoadMap evnt = (LoadMap) @event;
    ILoadMapReceiver receiver = target as ILoadMapReceiver;

    if (receiver != null) {
      receiver.OnEvent(evnt, evnt._connection);
    }
  }
}
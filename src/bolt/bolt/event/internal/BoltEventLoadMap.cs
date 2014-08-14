using System;
using UdpKit;

public interface ILoadMap : IBoltEvent {
  BoltMapLoadOp op { get; set; }
}

public interface ILoadMapReceiver {
  void OnEvent (ILoadMap evnt, BoltConnection connection);
}

class LoadMap : BoltEventS2C, ILoadMap {
  public const ushort ID = 0;

  BoltMapLoadOp _op;

  public BoltMapLoadOp op {
    get { return _op; }
    set { _op = value; }
  }

  internal LoadMap ()
    : base(ID) {
  }

  public override void Pack (UdpStream stream, BoltConnection cn) {
    stream.WriteString(_op.map);
    stream.WriteInt(_op.token);
  }

  public override void Read (UdpStream stream, BoltConnection cn) {
    _op.map = stream.ReadString();
    _op.token = stream.ReadInt();
  }

  public override void Free () {
    _op.map = default(string);
    _op.token = default(int);
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
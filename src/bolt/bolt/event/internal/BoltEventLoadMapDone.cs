using System;
using UdpKit;

public interface ILoadMapDone : IBoltEvent {
  BoltMapLoadOp op { get; set; }
}

public interface ILoadMapDoneReceiver {
  void OnEvent (ILoadMapDone evnt, BoltConnection connection);
}

class LoadMapDone : BoltEventANY, ILoadMapDone {
  public const ushort ID = 1;

  BoltMapLoadOp _op;

  public BoltMapLoadOp op {
    get { return _op; }
    set { _op = value; }
  }

  internal LoadMapDone ()
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



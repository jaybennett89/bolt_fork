using System;

abstract class BoltChannel {
  BoltConnection _cn;

  public BoltConnection connection {
    get { return _cn; }
    internal set {
      if (_cn == null) {
        _cn = value;
      } else {
        throw new InvalidOperationException();
      }
    }
  }

  public abstract void Pack (BoltPacket packet);
  public abstract void Read (BoltPacket packet);

  public virtual void Lost (BoltPacket packet) { }
  public virtual void Delivered (BoltPacket packet) { }

  public virtual void ReadDone () { }
  public virtual void StepRemoteFrame () { }
  public virtual void RemoteFrameReset (int oldFrame, int newFrame) { }
  public virtual void Disconnected () { }
}

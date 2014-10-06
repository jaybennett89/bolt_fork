using System;

class BoltEventReliable : BoltObject, IBoltSequenced, IDisposable {
  public uint Sequence { get; set; }
  public BoltEventBase evnt { get; set; }

  public void Dispose () {
    Sequence = 0;

    if (evnt) {
      evnt.Dispose();
      evnt = null;
    }
  }

  public override string ToString () {
    return evnt.ToString();
  }
}
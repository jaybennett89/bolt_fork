using System;

class BoltEventReliable : BoltObject, IBoltSequenced, IDisposable {
  public uint sequence { get; set; }
  public BoltEvent evnt { get; set; }

  public void Dispose () {
    sequence = 0;

    if (evnt) {
      evnt.Dispose();
      evnt = null;
    }
  }

  public override string ToString () {
    return evnt.ToString();
  }
}
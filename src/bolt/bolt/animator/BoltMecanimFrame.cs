using System;

internal abstract class BoltMecanimFrame : BoltObject, IDisposable {
  public int _frame;
  public bool _triggerOnly;
  public float[] _layerWeights;

  public abstract void Free ();
  public abstract BoltMecanimFrame Clone ();

  public void Dispose () {
    Free();
  }
}

using System;

public abstract class BoltStateFrame : BoltObject, IDisposable {
  internal int _frame;
  internal Bits _mask;

  public virtual void Dispose () {
  }

  public virtual BoltStateFrame Clone () {
    return (BoltStateFrame) MemberwiseClone();
  }
}
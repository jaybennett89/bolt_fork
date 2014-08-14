using System;

internal struct BoltMecanimInteger {
  public int value;

  public bool NotSame (int v) {
    return value != v;
  }

  public void Set (int v) {
    value = v;
  }
}
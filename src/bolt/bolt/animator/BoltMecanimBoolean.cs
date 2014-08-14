using System;

internal struct BoltMecanimBoolean {
  public bool value;

  public bool NotSame (bool v) {
    return value != v;
  }

  public void Set (bool v) {
    value = v;
  }
}

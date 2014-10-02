using UnityEngine;

internal struct BoltMecanimFloat {
  public float value;

  public bool NotSame(float v) {
    return Mathf.Approximately(value, v) == false;
  }

  public void Set(float v) {
    value = v;
  }
}
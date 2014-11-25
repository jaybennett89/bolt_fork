using Bolt;
using System;
using UnityEngine;

namespace Bolt {
  [Documentation]
  public class NetworkArray_Vector : NetworkArray_Values<Vector3> {
    internal NetworkArray_Vector(int length)
      : base(length) {
    }

    protected override Vector3 GetValue(int index) {
      return Storage.Values[index].Vector3;
    }

    protected override void SetValue(int index, Vector3 value) {
      Storage.Values[index].Vector3 = value;
    }
  }
}
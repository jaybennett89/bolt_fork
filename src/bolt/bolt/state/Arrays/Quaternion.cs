using Bolt;
using System;
using UnityEngine;

namespace Bolt {
  [Documentation]
  public class NetworkArray_Quaternion : NetworkArray_Values<Quaternion> {
    internal NetworkArray_Quaternion(int length)
      : base(length) {
    }

    protected override Quaternion GetValue(int index) {
      return Storage.Values[index].Quaternion;
    }

    protected override void SetValue(int index, Quaternion value) {
      Storage.Values[index].Quaternion = value;
    }
  }
}
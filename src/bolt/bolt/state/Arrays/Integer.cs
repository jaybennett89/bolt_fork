using Bolt;
using System;
using UnityEngine;

namespace Bolt {
  [Documentation]
  public class NetworkArray_Integer : NetworkArray_Values<Int32> {
    internal NetworkArray_Integer(int length)
      : base(length) {
    }

    protected override Int32 GetValue(int index) {
      return Storage.Values[index].Int0;
    }

    protected override void SetValue(int index, Int32 value) {
      Storage.Values[index].Int0 = value;
    }
  }
}
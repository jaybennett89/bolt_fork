using Bolt;
using System;
using UnityEngine;

namespace Bolt {
  [Documentation]
  public class NetworkArray_Float : NetworkArray_Values<Single> {
    internal NetworkArray_Float(int length)
      : base(length) {
    }

    protected override Single GetValue(int index) {
      return Storage.Values[index].Float0;
    }

    protected override void SetValue(int index, Single value) {
      Storage.Values[index].Float0 = value;
    }
  }
}
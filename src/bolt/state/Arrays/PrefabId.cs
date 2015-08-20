using Bolt;
using System;
using UnityEngine;

namespace Bolt {
  [Documentation]
  public class NetworkArray_PrefabId : NetworkArray_Values<PrefabId> {
    internal NetworkArray_PrefabId(int length, int stride)
      : base(length, stride) {
      Assert.True(stride == 1);
    }

    protected override PrefabId GetValue(int index) {
      return Storage.Values[index].PrefabId;
    }

    protected override bool SetValue(int index, PrefabId value) {
      if (Storage.Values[index].PrefabId != value) {
        Storage.Values[index].PrefabId = value;
        return true;
      }

      return false;
    }
  }
}
﻿using Bolt;
using System;
using UnityEngine;

namespace Bolt {
  [Documentation]
  public class NetworkArray_Quaternion : NetworkArray_Values<Quaternion> {
    internal NetworkArray_Quaternion(int length, int stride)
      : base(length, stride) {
      Assert.True((stride == 1) || (stride == 2));
    }

    protected override Quaternion GetValue(int index) {
      return Storage.Values[index].Quaternion;
    }

    protected override bool SetValue(int index, Quaternion value) {
      if (Storage.Values[index].Quaternion != value) {
        Storage.Values[index].Quaternion = value;
        return true;
      }

      return false;
    }
  }
}
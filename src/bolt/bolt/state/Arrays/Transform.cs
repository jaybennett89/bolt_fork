﻿using Bolt;
using System;
using UnityEngine;

namespace Bolt {
  [Documentation]
  public class NetworkArray_Transform : NetworkArray_Values<NetworkTransform> {
    public new NetworkTransform this[int index]
    {
      get { return base[index]; }
    }

    internal NetworkArray_Transform(int length)
      : base(length) {
    }

    protected override NetworkTransform GetValue(int index) {
      return Storage.Values[index].Transform;
    }

    protected override void SetValue(int index, NetworkTransform value) {
      throw new NotSupportedException();
    }
  }
}
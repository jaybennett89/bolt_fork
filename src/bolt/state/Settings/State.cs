using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UE = UnityEngine;

namespace Bolt {
  internal struct PropertyStateSettings {
    public int Priority;
    public int ByteLength;

    public ArrayIndices CallbackIndices;

    public PropertyStateSettings(int priority, int byteLength, int objectOffset, string propertyPath, string[] callbackPaths, ArrayIndices callbackIndices) {
      Priority = UE.Mathf.Max(1, priority);
      ByteLength = byteLength;
      ObjectOffset = objectOffset;
      CallbackIndices = callbackIndices;
    }
  }
}

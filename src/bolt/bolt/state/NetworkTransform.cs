using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UE = UnityEngine;

namespace Bolt {
  [Documentation]
  public class NetworkTransform {
    internal UE.Transform Render;
    internal UE.Transform Simulate;

    internal DoubleBuffer<UE.Vector3> RenderDoubleBufferPosition;
    internal DoubleBuffer<UE.Quaternion> RenderDoubleBufferRotation;

    public UE.Vector3 Position {
      get { return RenderDoubleBufferPosition.Current; }
    }

    public UE.Quaternion Rotation {
      get { return RenderDoubleBufferRotation.Current; }
    }

    public void SetTransforms(UE.Transform simulate) {
      SetTransforms(simulate, null);
    }

    public void SetTransforms(UE.Transform simulate, UE.Transform render) {
      if (render) {
        Render = render;
        RenderDoubleBufferPosition = DoubleBuffer<UE.Vector3>.InitBuffer(simulate.position);
        RenderDoubleBufferRotation = DoubleBuffer<UE.Quaternion>.InitBuffer(simulate.rotation);
      }

      Simulate = simulate;
    }
  }

}

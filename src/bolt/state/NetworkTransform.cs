using UE = UnityEngine;

namespace Bolt {
  [Documentation]
  public class NetworkTransform {
    internal System.Int32 PropertyIndex;
    internal UE.Transform Render;
    internal UE.Transform Simulate;
    internal System.Func<BoltEntity, UE.Vector3, UE.Vector3> Clamper = (entity, position) => position;

    internal DoubleBuffer<UE.Vector3> RenderDoubleBufferPosition;
    internal DoubleBuffer<UE.Quaternion> RenderDoubleBufferRotation;

    public UE.Transform Transform {
      get { return Simulate; }
    }

    public UE.Vector3 Position {
      get { return RenderDoubleBufferPosition.Current; }
    }

    public UE.Quaternion Rotation {
      get { return RenderDoubleBufferRotation.Current; }
    }

    public void SetExtrapolationClamper(System.Func<BoltEntity, UE.Vector3, UE.Vector3> clamper) {
      Assert.NotNull(clamper);
      Clamper = clamper;
    }

    [System.Obsolete("For setting the transform to replicate in Attached use the new IState.SetTransforms method instead, for changing the transform after it's been set use the new ChangeTransforms method")]
    public void SetTransforms(UE.Transform simulate) {
      ChangeTransforms(simulate, null);
    }

    [System.Obsolete("For setting the transform to replicate in Attached use the new IState.SetTransforms method instead, for changing the transform after it's been set use the new ChangeTransforms method")]
    public void SetTransforms(UE.Transform simulate, UE.Transform render) {
      ChangeTransforms(simulate, render);
    }

    public void ChangeTransforms(UE.Transform simulate) {
      ChangeTransforms(simulate, null);
    }

    public void ChangeTransforms(UE.Transform simulate, UE.Transform render) {
      if (render) {
        Render = render;
        RenderDoubleBufferPosition = DoubleBuffer<UE.Vector3>.InitBuffer(simulate.position);
        RenderDoubleBufferRotation = DoubleBuffer<UE.Quaternion>.InitBuffer(simulate.rotation);
      }
      else {
        Render = null;
        RenderDoubleBufferPosition = DoubleBuffer<UE.Vector3>.InitBuffer(UE.Vector3.zero);
        RenderDoubleBufferRotation = DoubleBuffer<UE.Quaternion>.InitBuffer(UE.Quaternion.identity);
      }

      Simulate = simulate;
    }

    internal void SetTransformsInternal(UE.Transform simulate, UE.Transform render) {
      if (render) {
        Render = render;
        RenderDoubleBufferPosition = DoubleBuffer<UE.Vector3>.InitBuffer(simulate.position);
        RenderDoubleBufferRotation = DoubleBuffer<UE.Quaternion>.InitBuffer(simulate.rotation);
      }
      else {
        Render = null;
        RenderDoubleBufferPosition = DoubleBuffer<UE.Vector3>.InitBuffer(UE.Vector3.zero);
        RenderDoubleBufferRotation = DoubleBuffer<UE.Quaternion>.InitBuffer(UE.Quaternion.identity);
      }

      Simulate = simulate;
    }
  }

}

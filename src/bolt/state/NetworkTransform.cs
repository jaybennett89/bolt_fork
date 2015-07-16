using UE = UnityEngine;

namespace Bolt {
  [Documentation]
  public class NetworkTransform {
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

    public void SetTransforms(UE.Transform simulate) {
      SetTransforms(simulate, null);
    }

    public void SetTransforms(UE.Transform simulate, UE.Transform render) {
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

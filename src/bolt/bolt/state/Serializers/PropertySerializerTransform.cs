using System;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  [Documentation]
  public class TransformData {
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


  class PropertySerializerTransform : PropertySerializer {
    PropertyTransformCompressionSettings TransformCompression;

    const int POSITION_OFFSET = 0;
    const int VELOCITY_OFFSET = 12;
    const int ROTATION_OFFSET = 24;

    public void AddSettings(PropertyTransformCompressionSettings transformCompression) {
      TransformCompression = transformCompression;
    }

    public override int StateBits(State state, State.Frame frame) {
      int bits = 1;

      bits += TransformCompression.Position.BitsRequired;
      bits += TransformCompression.Rotation.BitsRequired;

      if (SmoothingSettings.Algorithm == SmoothingAlgorithms.Extrapolation) {
        bits += TransformCompression.Position.BitsRequired;
      }

      return bits;
    }

    public override object GetDebugValue(State state) {
      var td = (TransformData)state.Frames.first.Objects[StateSettings.ObjectOffset];
      if (td.Simulate) {
        var p = state.Frames.first.Data.ReadVector3(Settings.ByteOffset + POSITION_OFFSET);
        var r = state.Frames.first.Data.ReadQuaternion(Settings.ByteOffset + ROTATION_OFFSET).eulerAngles;
        var pos = string.Format("X:{0} Y:{1} Z:{2}", p.x.ToString("F3"), p.y.ToString("F3"), p.z.ToString("F3"));
        var rot = string.Format("X:{0} Y:{1} Z:{2}", r.x.ToString("F3"), r.y.ToString("F3"), r.z.ToString("F3"));
        return string.Format("{0} / {1}", pos, rot);
      }
      else {
        return "NOT ASSIGNED";
      }
    }

    public override void OnInit(State state) {
      state.PropertyObjects[StateSettings.ObjectOffset] = new TransformData();
    }

    public override void OnRender(State state, State.Frame frame) {
      var td = (TransformData)state.Frames.first.Objects[StateSettings.ObjectOffset];
      if (td.Render) {
        var p = td.RenderDoubleBufferPosition.Previous;
        var c = td.RenderDoubleBufferPosition.Current;
        td.Render.position = UE.Vector3.Lerp(p, c, BoltCore.frameAlpha);
        td.Render.rotation = td.RenderDoubleBufferRotation.Current;
      }
    }

    public override void OnParentChanged(State state, Entity newParent, Entity oldParent) {
      var td = (TransformData)state.Frames.first.Objects[StateSettings.ObjectOffset];
      if (newParent == null) {
        td.Simulate.transform.parent = null;
        UpdateTransformValues(state, oldParent.UnityObject.transform.localToWorldMatrix, UE.Matrix4x4.identity);
      }
      else if (oldParent == null) {
        td.Simulate.transform.parent = newParent.UnityObject.transform;
        UpdateTransformValues(state, UE.Matrix4x4.identity, newParent.UnityObject.transform.worldToLocalMatrix);
      }
      else {
        td.Simulate.transform.parent = newParent.UnityObject.transform;
        UpdateTransformValues(state, oldParent.UnityObject.transform.localToWorldMatrix, newParent.UnityObject.transform.worldToLocalMatrix);
      }
    }

    void UpdateTransformValues(State state, UE.Matrix4x4 l2w, UE.Matrix4x4 w2l) {
      var it = state.Frames.GetIterator();

      while (it.Next()) {
        UE.Vector3 p = it.val.Data.ReadVector3(Settings.ByteOffset + POSITION_OFFSET);
        UE.Quaternion r = it.val.Data.ReadQuaternion(Settings.ByteOffset + ROTATION_OFFSET);

        float angle;
        UE.Vector3 axis;
        r.ToAngleAxis(out angle, out axis);

        // transform position
        p = l2w.MultiplyPoint(p);
        p = w2l.MultiplyPoint(p);

        // transform rotation
        axis = l2w.MultiplyVector(axis);
        axis = w2l.MultiplyVector(axis);
        r = UE.Quaternion.AngleAxis(angle, axis);

        // put back into frame
        it.val.Data.PackVector3(Settings.ByteOffset + POSITION_OFFSET, p);
        it.val.Data.PackQuaternion(Settings.ByteOffset + ROTATION_OFFSET, r);
      }
    }

    public override void OnSimulateBefore(State state) {
      var td = (TransformData)state.Frames.first.Objects[StateSettings.ObjectOffset];
      if (td.Simulate && !state.Entity.IsOwner && !state.Entity.HasPredictedControl) {
        var p = Settings.ByteOffset + POSITION_OFFSET;
        var v = Settings.ByteOffset + VELOCITY_OFFSET;
        var r = Settings.ByteOffset + ROTATION_OFFSET;

        switch (SmoothingSettings.Algorithm) {
          case SmoothingAlgorithms.None:
            PerformNone(td, state);
            break;

          case SmoothingAlgorithms.Interpolation:
            td.Simulate.localPosition = Math.InterpolateVector(state.Frames, p, state.Entity.Frame);
            td.Simulate.localRotation = Math.InterpolateQuaternion(state.Frames, r, state.Entity.Frame);
            break;

          case SmoothingAlgorithms.Extrapolation:
            int frame = UE.Mathf.Min(state.Frames.first.Number + SmoothingSettings.ExtrapolationMaxFrames, state.Entity.Frame);
            td.Simulate.localPosition = Math.ExtrapolateVector(state.Frames, p, v, frame, SmoothingSettings, td.Simulate.localPosition);
            td.Simulate.localRotation = Math.ExtrapolateQuaternion(state.Frames, r, frame, SmoothingSettings, td.Simulate.localRotation);
            break;
        }
      }
    }

    public override void OnSimulateAfter(State state) {
      var td = (TransformData)state.Frames.first.Objects[StateSettings.ObjectOffset];
      if (td.Simulate) {
        var f = state.Frames.first;

        if (state.Entity.IsOwner) {
          UE.Vector3 position = f.Data.ReadVector3(Settings.ByteOffset + POSITION_OFFSET);
          UE.Vector3 velocity = GetVelocity(td, position);

          f.Data.PackVector3(Settings.ByteOffset + POSITION_OFFSET, td.Simulate.localPosition);
          f.Data.PackVector3(Settings.ByteOffset + VELOCITY_OFFSET, velocity);
          f.Data.PackQuaternion(Settings.ByteOffset + ROTATION_OFFSET, td.Simulate.localRotation);
        }

        td.RenderDoubleBufferPosition = td.RenderDoubleBufferPosition.Shift(td.Simulate.position);
        td.RenderDoubleBufferRotation = td.RenderDoubleBufferRotation.Shift(td.Simulate.rotation);
      }
    }

    UE.Vector3 GetVelocity(TransformData td, UE.Vector3 position) {
      switch (SmoothingSettings.ExtrapolationVelocityMode) {
        case ExtrapolationVelocityModes.CalculateFromPosition:
          return (td.Simulate.localPosition - position) * BoltCore._config.framesPerSecond;

        case ExtrapolationVelocityModes.CopyFromRigidbody:
          return td.Simulate.rigidbody.velocity;

        case ExtrapolationVelocityModes.CopyFromRigidbody2D:
          return td.Simulate.rigidbody2D.velocity;

        case ExtrapolationVelocityModes.CopyFromCharacterController:
          return td.Simulate.GetComponent<UE.CharacterController>().velocity;

        default:
          BoltLog.Warn("Unknown velocity extrapolation mode {0}", SmoothingSettings.ExtrapolationVelocityMode);
          return (td.Simulate.localPosition - position) * BoltCore._config.framesPerSecond;
      }
    }

    public override bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      if (state.Entity.HasParent) {
        if (connection._entityChannel.ExistsOnRemote(state.Entity.Parent)) {
          stream.WriteEntity(state.Entity.Parent, connection);
        }
        else {
          return false;
        }
      }
      else {
        stream.WriteEntity(null, connection);
      }

      UE.Vector3 p = frame.Data.ReadVector3(Settings.ByteOffset + POSITION_OFFSET);
      TransformCompression.Position.Pack(stream, p);

      if (SmoothingSettings.Algorithm == SmoothingAlgorithms.Extrapolation) {
        UE.Vector3 v = frame.Data.ReadVector3(Settings.ByteOffset + VELOCITY_OFFSET);
        TransformCompression.Position.Pack(stream, v);
      }

      UE.Quaternion r = frame.Data.ReadQuaternion(Settings.ByteOffset + ROTATION_OFFSET);
      TransformCompression.Rotation.Pack(stream, r);

      return true;
    }

    public override void StateRead(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      state.Entity.SetParentInternal(stream.ReadEntity(connection));

      UE.Vector3 p = default(UE.Vector3);
      UE.Vector3 v = default(UE.Vector3);
      UE.Quaternion r = default(UE.Quaternion);

      p = TransformCompression.Position.Read(stream);

      if (SmoothingSettings.Algorithm == SmoothingAlgorithms.Extrapolation) {
        v = TransformCompression.Position.Read(stream);
      }

      r = TransformCompression.Rotation.Read(stream);

      frame.Data.PackVector3(Settings.ByteOffset + POSITION_OFFSET, p);
      frame.Data.PackVector3(Settings.ByteOffset + VELOCITY_OFFSET, v);
      frame.Data.PackQuaternion(Settings.ByteOffset + ROTATION_OFFSET, r);
    }

    void PerformNone(TransformData td, State state) {
      var f0 = state.Frames.first;

      UE.Vector3 p0 = f0.Data.ReadVector3(Settings.ByteOffset + POSITION_OFFSET);
      UE.Quaternion r0 = f0.Data.ReadQuaternion(Settings.ByteOffset + ROTATION_OFFSET);

      td.Simulate.localPosition = p0;
      td.Simulate.localRotation = r0;
    }
  }
}

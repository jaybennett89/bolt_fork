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

  struct PropertyTransformCompressionSettings {
    public PropertyFloatCompressionSettings PositionX;
    public PropertyFloatCompressionSettings PositionY;
    public PropertyFloatCompressionSettings PositionZ;

    public PropertyFloatCompressionSettings RotationX;
    public PropertyFloatCompressionSettings RotationY;
    public PropertyFloatCompressionSettings RotationZ;

    public bool QuaternionMode;
    public PropertyFloatCompressionSettings Quaternion;

    public static PropertyTransformCompressionSettings Create(
      PropertyFloatCompressionSettings posX,
      PropertyFloatCompressionSettings posY,
      PropertyFloatCompressionSettings posZ,
      PropertyFloatCompressionSettings quaternion) {

      return new PropertyTransformCompressionSettings {
        PositionX = posX,
        PositionY = posY,
        PositionZ = posZ,
        Quaternion = quaternion,
        QuaternionMode = true
      };
    }

    public static PropertyTransformCompressionSettings Create(
      PropertyFloatCompressionSettings posX,
      PropertyFloatCompressionSettings posY,
      PropertyFloatCompressionSettings posZ,
      PropertyFloatCompressionSettings rotX,
      PropertyFloatCompressionSettings rotY,
      PropertyFloatCompressionSettings rotZ) {

      return new PropertyTransformCompressionSettings {
        PositionX = posX,
        PositionY = posY,
        PositionZ = posZ,
        RotationX = rotX,
        RotationY = rotY,
        RotationZ = rotZ,
        QuaternionMode = false
      };
    }
  }

  class PropertySerializerTransform : PropertySerializer {
    PropertySmoothingSettings SmoothingSettings;
    PropertyTransformCompressionSettings TransformCompression;

    const int POSITION_OFFSET = 0;
    const int ROTATION_OFFSET = 24;
    const int VELOCITY_OFFSET = 12;

    public void AddSettings(PropertySmoothingSettings smoothingSettings) {
      SmoothingSettings = smoothingSettings;
    }

    public void AddSettings(PropertyTransformCompressionSettings transformCompression) {
      TransformCompression = transformCompression;
    }

    public override int StateBits(State state, State.Frame frame) {
      int bits = 1;

      bits += TransformCompression.PositionX.BitsRequired;
      bits += TransformCompression.PositionY.BitsRequired;
      bits += TransformCompression.PositionZ.BitsRequired;

      if (TransformCompression.QuaternionMode) {
        bits += (TransformCompression.Quaternion.BitsRequired * 4);
      }
      else {
        bits += TransformCompression.RotationX.BitsRequired;
        bits += TransformCompression.RotationY.BitsRequired;
        bits += TransformCompression.RotationZ.BitsRequired;
      }

      if (SmoothingSettings.Algorithm == SmoothingAlgorithms.Extrapolation) {
        bits += TransformCompression.PositionX.BitsRequired;
        bits += TransformCompression.PositionY.BitsRequired;
        bits += TransformCompression.PositionZ.BitsRequired;
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
        UE.Vector3 p = it.val.Data.ReadVector3(StateSettings.ObjectOffset + POSITION_OFFSET);
        UE.Quaternion r = it.val.Data.ReadQuaternion(StateSettings.ObjectOffset + ROTATION_OFFSET);

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
        it.val.Data.PackVector3(StateSettings.ObjectOffset + POSITION_OFFSET, p);
        it.val.Data.PackQuaternion(StateSettings.ObjectOffset + ROTATION_OFFSET, r);
      }
    }

    public override void OnSimulateBefore(State state) {
      var td = (TransformData)state.Frames.first.Objects[StateSettings.ObjectOffset];
      if (td.Simulate && !state.Entity.IsOwner && !state.Entity.HasPredictedControl) {
        var p = StateSettings.ObjectOffset + POSITION_OFFSET;
        var v = StateSettings.ObjectOffset + VELOCITY_OFFSET;
        var r = StateSettings.ObjectOffset + ROTATION_OFFSET;

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
            td.Simulate.localPosition = Math.ExtrapolateVector(state.Frames, p, v, frame, SmoothingSettings.ExtrapolationCorrectionFrames, td.Simulate.localPosition);
            td.Simulate.localRotation = Math.ExtrapolateQuaternion(state.Frames, r, frame, SmoothingSettings.ExtrapolationCorrectionFrames, td.Simulate.localRotation);
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
      TransformCompression.PositionX.Pack(stream, p.x);
      TransformCompression.PositionY.Pack(stream, p.y);
      TransformCompression.PositionZ.Pack(stream, p.z);

      if (SmoothingSettings.Algorithm == SmoothingAlgorithms.Extrapolation) {
        UE.Vector3 v = frame.Data.ReadVector3(Settings.ByteOffset + VELOCITY_OFFSET);
        TransformCompression.PositionX.Pack(stream, v.x);
        TransformCompression.PositionY.Pack(stream, v.y);
        TransformCompression.PositionZ.Pack(stream, v.z);
      }

      UE.Quaternion r = frame.Data.ReadQuaternion(Settings.ByteOffset + ROTATION_OFFSET);
      if (TransformCompression.QuaternionMode) {
        TransformCompression.Quaternion.Pack(stream, r.x);
        TransformCompression.Quaternion.Pack(stream, r.y);
        TransformCompression.Quaternion.Pack(stream, r.z);
        TransformCompression.Quaternion.Pack(stream, r.w);
      }
      else {
        UE.Vector3 a = r.eulerAngles;
        TransformCompression.RotationX.Pack(stream, a.x);
        TransformCompression.RotationY.Pack(stream, a.y);
        TransformCompression.RotationZ.Pack(stream, a.z);
      }

      return true;
    }

    public override void StateRead(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      state.Entity.SetParentInternal(stream.ReadEntity(connection));

      UE.Vector3 p = new UE.Vector3();
      UE.Vector3 v = new UE.Vector3();
      UE.Quaternion r = new UE.Quaternion();

      p.x = TransformCompression.PositionX.Read(stream);
      p.y = TransformCompression.PositionY.Read(stream);
      p.z = TransformCompression.PositionZ.Read(stream);

      if (SmoothingSettings.Algorithm == SmoothingAlgorithms.Extrapolation) {
        v.x = TransformCompression.PositionX.Read(stream);
        v.y = TransformCompression.PositionY.Read(stream);
        v.z = TransformCompression.PositionZ.Read(stream);
      }

      if (TransformCompression.QuaternionMode) {
        r.x = TransformCompression.Quaternion.Read(stream);
        r.y = TransformCompression.Quaternion.Read(stream);
        r.z = TransformCompression.Quaternion.Read(stream);
        r.w = TransformCompression.Quaternion.Read(stream);
      }
      else {
        UE.Vector3 a = new UE.Vector3();
        a.x = TransformCompression.RotationX.Read(stream);
        a.y = TransformCompression.RotationY.Read(stream);
        a.z = TransformCompression.RotationZ.Read(stream);

        r = UE.Quaternion.Euler(a);
      }

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

    //void PerformInterpolation(TransformData td, State state, bool position) {
    //  var f0 = state.Frames.first;

    //  UE.Vector3 p0 = f0.Data.ReadVector3(StateData.ByteOffset + POSITION_OFFSET);
    //  UE.Quaternion r0 = f0.Data.ReadQuaternion(StateData.ByteOffset + ROTATION_OFFSET);

    //  if ((state.Frames.count == 1) || (f0.Number >= state.Entity.Frame)) {
    //    if (position) {
    //      td.Simulate.localPosition = p0;
    //    }

    //    td.Simulate.localRotation = r0;
    //  }
    //  else {
    //    var f1 = state.Frames.Next(f0);
    //    UE.Vector3 p1 = f1.Data.ReadVector3(StateData.ByteOffset + POSITION_OFFSET);
    //    UE.Quaternion r1 = f1.Data.ReadQuaternion(StateData.ByteOffset + ROTATION_OFFSET);

    //    Assert.True(f1.Number > f0.Number);
    //    Assert.True(f1.Number > state.Entity.Frame);

    //    float t = f1.Number - f0.Number;
    //    float d = state.Entity.Frame - f0.Number;

    //    if (position) {
    //      td.Simulate.localPosition = UE.Vector3.Lerp(p0, p1, d / t);
    //    }

    //    td.Simulate.localRotation = UE.Quaternion.Lerp(r0, r1, d / t);
    //  }
    //}

    //void PerformExtrapolation(TransformData td, State state) {
    //  var f = state.Frames.first;

    //  UE.Vector3 p = f.Data.ReadVector3(StateData.ByteOffset + POSITION_OFFSET);
    //  UE.Vector3 v = f.Data.ReadVector3(StateData.ByteOffset + VELOCITY_OFFSET) * BoltNetwork.frameDeltaTime;

    //  float d = (state.Entity.Frame + 1) - f.Number;
    //  float t = d / state.Entity.SendRate;

    //  // blend from current to new position
    //  td.Simulate.localPosition = UE.Vector3.Lerp(td.Simulate.localPosition + v, p + (v * d), t);

    //  // interpolate rotation
    //  PerformInterpolation(td, state, false);
    //}
  }
}

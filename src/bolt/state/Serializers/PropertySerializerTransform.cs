using System;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  class PropertySerializerTransform : PropertySerializer {
    PropertyTransformCompressionSettings TransformCompression;

    const int POSITION_OFFSET = 0;
    const int VELOCITY_OFFSET = 3;
    const int ROTATION_OFFSET = 4;

    public void AddSettings(PropertyTransformCompressionSettings transformCompression) {
      TransformCompression = transformCompression;
    }

    public override int StateBits(State state, NetworkFrame frame) {
      int bits = 1;

      bits += TransformCompression.Position.BitsRequired;
      bits += TransformCompression.Rotation.BitsRequired;

      if (SmoothingSettings.Algorithm == SmoothingAlgorithms.Extrapolation) {
        bits += TransformCompression.Position.BitsRequired;
      }

      return bits;
    }

    //public override object GetDebugValue(State state) {
    //  var td = (NetworkTransform)state.Objects[Settings.OffsetObjects];
    //  if (td.Simulate) {
    //    var p = state.CurrentFrame.Storage[Settings.OffsetStorage + POSITION_OFFSET].Vector3;
    //    var r = state.CurrentFrame.Storage[Settings.OffsetStorage + ROTATION_OFFSET].Quaternion;

    //    var pos = string.Format("X:{0} Y:{1} Z:{2}", p.x.ToString("F3"), p.y.ToString("F3"), p.z.ToString("F3"));
    //    var rot = string.Format("X:{0} Y:{1} Z:{2}", r.x.ToString("F3"), r.y.ToString("F3"), r.z.ToString("F3"));

    //    return string.Format("{0} / {1}", pos, rot);
    //  }
    //  else {
    //    return "NOT ASSIGNED";
    //  }
    //}

    public override void OnInit(State state) {
      //state.Objects[Settings.OffsetObjects] = new NetworkTransform();
    }

    public override void OnRender(State state, NetworkFrame frame) {
      //var td = (NetworkTransform)state.Objects[Settings.OffsetObjects];

      //if (ReferenceEquals(td.Render, null)) {
      //  return;
      //}

      //var p = td.RenderDoubleBufferPosition.Previous;
      //var c = td.RenderDoubleBufferPosition.Current;

      //td.Render.position = UE.Vector3.Lerp(p, c, BoltCore.frameAlpha);
      //td.Render.rotation = td.RenderDoubleBufferRotation.Current;
    }

    public override void OnParentChanged(State state, Entity newParent, Entity oldParent) {
      //var td = (NetworkTransform)state.Objects[Settings.OffsetObjects];
      //if (newParent == null) {
      //  td.Simulate.transform.parent = null;
      //  UpdateTransformValues(state, oldParent.UnityObject.transform.localToWorldMatrix, UE.Matrix4x4.identity);
      //}
      //else if (oldParent == null) {
      //  td.Simulate.transform.parent = newParent.UnityObject.transform;
      //  UpdateTransformValues(state, UE.Matrix4x4.identity, newParent.UnityObject.transform.worldToLocalMatrix);
      //}
      //else {
      //  td.Simulate.transform.parent = newParent.UnityObject.transform;
      //  UpdateTransformValues(state, oldParent.UnityObject.transform.localToWorldMatrix, newParent.UnityObject.transform.worldToLocalMatrix);
      //}
    }

    void UpdateTransformValues(State state, UE.Matrix4x4 l2w, UE.Matrix4x4 w2l) {
      var it = state.Frames.GetIterator();

      while (it.Next()) {
        var p = it.val.Storage[Settings.OffsetStorage + POSITION_OFFSET].Vector3;
        var r = it.val.Storage[Settings.OffsetStorage + ROTATION_OFFSET].Quaternion;

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
        it.val.Storage[Settings.OffsetStorage + POSITION_OFFSET].Vector3 = p;
        it.val.Storage[Settings.OffsetStorage + ROTATION_OFFSET].Quaternion = r;
      }
    }

    public override void OnSimulateBefore(State state) {
      //if (state.Entity.IsDummy) {
      //  var td = (NetworkTransform)state.Objects[Settings.OffsetObjects];

      //  if (ReferenceEquals(td.Simulate, null)) {
      //    return;
      //  }

      //  var p = Settings.OffsetStorage + POSITION_OFFSET;
      //  var v = Settings.OffsetStorage + VELOCITY_OFFSET;
      //  var r = Settings.OffsetStorage + ROTATION_OFFSET;
      //  var snap = false;

      //  switch (SmoothingSettings.Algorithm) {
      //    case SmoothingAlgorithms.None:
      //      PerformNone(td, state);
      //      break;

      //    case SmoothingAlgorithms.Interpolation:
      //      //td.Simulate.localPosition = Math.InterpolateVector(state.Frames, p, state.Entity.ServerFrame, SmoothingSettings.SnapMagnitude, ref snap);
      //      //td.Simulate.localRotation = Math.InterpolateQuaternion(state.Frames, r, state.Entity.ServerFrame);
      //      break;

      //    case SmoothingAlgorithms.Extrapolation:
      //      //td.Simulate.localPosition = Math.ExtrapolateVector(state.Frames, p, state.Entity.ServerFrame, SmoothingSettings, ref snap);            //BoltPoll.ASSIGN.Stop();
      //      //td.Simulate.localRotation = Math.ExtrapolateQuaternion(state.Frames, r, state.Entity.ServerFrame, SmoothingSettings);
      //      break;
      //  }

      //  if (snap) {
      //    td.RenderDoubleBufferPosition = td.RenderDoubleBufferPosition.Shift(td.Simulate.position).Shift(td.Simulate.position);
      //  }
      //}
      //else {
      //  //BoltLog.Warn("The transform of {0}.{1} has not been assigned", state.Entity.UnityObject.gameObject.name, Settings.PropertyName);
      //}
    }

    public override void OnSimulateAfter(State state) {
      //var td = (NetworkTransform)state.Objects[Settings.OffsetObjects];

      //if (ReferenceEquals(td.Simulate, null)) {
      //  return;
      //}

      //var f = state.Frames.first;

      //if (state.Entity.IsOwner) {
      //  UE.Vector3 p = f.Storage[Settings.OffsetStorage + POSITION_OFFSET].Vector3;
      //  UE.Vector3 v = GetVelocity(td, p);

      //  f.Storage[Settings.OffsetStorage + POSITION_OFFSET].Vector3 = td.Simulate.localPosition;
      //  f.Storage[Settings.OffsetStorage + VELOCITY_OFFSET].Vector3 = v;
      //  f.Storage[Settings.OffsetStorage + ROTATION_OFFSET].Quaternion = td.Simulate.localRotation;
      //}

      //td.RenderDoubleBufferPosition = td.RenderDoubleBufferPosition.Shift(td.Simulate.position);
      //td.RenderDoubleBufferRotation = td.RenderDoubleBufferRotation.Shift(td.Simulate.rotation);
    }

    UE.Vector3 GetVelocity(NetworkTransform td, UE.Vector3 position) {
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

    public override bool StatePack(State state, NetworkFrame frame, BoltConnection connection, UdpKit.UdpPacket stream) {
      if (state.Entity.HasParent) {
        if (connection._entityChannel.ExistsOnRemote(state.Entity.Parent)) {
          stream.WriteEntity(state.Entity.Parent);
        }
        else {
          return false;
        }
      }
      else {
        stream.WriteEntity(null);
      }

      // position
      TransformCompression.Position.Pack(stream, frame.Storage[Settings.OffsetStorage + POSITION_OFFSET].Vector3);

      if (SmoothingSettings.Algorithm == SmoothingAlgorithms.Extrapolation) {
        // velocity
        TransformCompression.Position.Pack(stream, frame.Storage[Settings.OffsetStorage + VELOCITY_OFFSET].Vector3);
      }

      // rotation
      TransformCompression.Rotation.Pack(stream, frame.Storage[Settings.OffsetStorage + ROTATION_OFFSET].Quaternion);

      return true;
    }

    public override void StateRead(State state, NetworkFrame frame, BoltConnection connection, UdpKit.UdpPacket stream) {
      state.Entity.SetParentInternal(stream.ReadEntity());
      
      // position
      frame.Storage[Settings.OffsetStorage + POSITION_OFFSET].Vector3 = TransformCompression.Position.Read(stream);

      if (SmoothingSettings.Algorithm == SmoothingAlgorithms.Extrapolation) {
        // velocity
        frame.Storage[Settings.OffsetStorage + VELOCITY_OFFSET].Vector3 = TransformCompression.Position.Read(stream);
      }

      // rotation
      frame.Storage[Settings.OffsetStorage + ROTATION_OFFSET].Quaternion = TransformCompression.Rotation.Read(stream);
    }

    void PerformNone(NetworkTransform td, State state) {
      td.Simulate.localPosition = state.CurrentFrame.Storage[Settings.OffsetStorage + POSITION_OFFSET].Vector3;
      td.Simulate.localRotation = state.CurrentFrame.Storage[Settings.OffsetStorage + ROTATION_OFFSET].Quaternion;
    }
  }
}
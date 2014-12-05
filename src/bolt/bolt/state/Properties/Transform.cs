using System;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  internal class NetworkProperty_Transform : NetworkProperty {
    const int POSITION = 0;
    const int ROTATION = 1;
    const int VELOCITY = 2;

    PropertyExtrapolationSettings Extrapolation;
    PropertyQuaternionCompression RotationCompression;
    PropertyVectorCompressionSettings PositionCompression;

    public void Settings_Vector(PropertyFloatCompressionSettings x, PropertyFloatCompressionSettings y, PropertyFloatCompressionSettings z) {
      PositionCompression = PropertyVectorCompressionSettings.Create(x, y, z);
    }

    public void Settings_Quaternion(PropertyFloatCompressionSettings compression) {
      RotationCompression = PropertyQuaternionCompression.Create(compression);
    }

    public void Settings_QuaternionEuler(PropertyFloatCompressionSettings x, PropertyFloatCompressionSettings y, PropertyFloatCompressionSettings z) {
      RotationCompression = PropertyQuaternionCompression.Create(PropertyVectorCompressionSettings.Create(x, y, z));
    }

    public void Settings_Extrapolation(PropertyExtrapolationSettings extrapolation) {
      Extrapolation = extrapolation;
    }

    public override bool AllowCallbacks {
      get { return false; }
    }

    public override bool WantsOnRender {
      get { return true; }
    }

    public override bool WantsOnSimulateAfter {
      get { return true; }
    }

    public override bool WantsOnSimulateBefore {
      get { return true; }
    }

    public override int BitCount(NetworkObj obj) {
      if (Extrapolation.Enabled) {
        return (PositionCompression.BitsRequired * 2) + RotationCompression.BitsRequired;
      }

      return PositionCompression.BitsRequired + RotationCompression.BitsRequired;
    }

    public override void OnInit(NetworkObj obj) {
      obj.Storage.Values[obj[this] + POSITION].Transform = new NetworkTransform();
      obj.Storage.Values[obj[this] + ROTATION].Quaternion = UE.Quaternion.identity;
    }

    public override object DebugValue(NetworkObj obj, NetworkStorage storage) {
      var nt = obj.Storage.Values[obj[this]].Transform;

      if (nt != null && nt.Simulate) {
        var p = obj.Storage.Values[obj[this] + POSITION].Vector3;
        var r = obj.Storage.Values[obj[this] + ROTATION].Quaternion;

        var pos = string.Format("X:{0} Y:{1} Z:{2}", p.x.ToString("F3"), p.y.ToString("F3"), p.z.ToString("F3"));
        var rot = string.Format("X:{0} Y:{1} Z:{2}", r.x.ToString("F3"), r.y.ToString("F3"), r.z.ToString("F3"));

        return string.Format("{0} / {1}", pos, rot);
      }

      return "NOT ASSIGNED";
    }

    public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      if (obj.RootState.Entity.HasParent) {
        if (connection._entityChannel.ExistsOnRemote(obj.RootState.Entity.Parent)) {
          packet.WriteEntity(obj.RootState.Entity.Parent);
        }
        else {
          return false;
        }
      }
      else {
        packet.WriteEntity(null);
      }

      PositionCompression.Pack(packet, storage.Values[obj[this] + POSITION].Vector3);
      RotationCompression.Pack(packet, storage.Values[obj[this] + ROTATION].Quaternion);

      if (Extrapolation.Enabled) {
        PositionCompression.Pack(packet, storage.Values[obj[this] + VELOCITY].Vector3);
      }

      return true;
    }

    public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      obj.RootState.Entity.SetParentInternal(packet.ReadEntity());

      storage.Values[obj[this] + POSITION].Vector3 = PositionCompression.Read(packet);
      storage.Values[obj[this] + ROTATION].Quaternion = RotationCompression.Read(packet);

      if (Extrapolation.Enabled) {
        storage.Values[obj[this] + VELOCITY].Vector3 = PositionCompression.Read(packet);
      }
    }

    public override void OnRender(NetworkObj obj) {
      var nt = obj.Storage.Values[obj[this] + POSITION].Transform;
      if (nt != null && nt.Render) {
        var p = nt.RenderDoubleBufferPosition.Previous;
        var c = nt.RenderDoubleBufferPosition.Current;

        nt.Render.position = UE.Vector3.Lerp(p, c, BoltCore.frameAlpha);
        nt.Render.rotation = nt.RenderDoubleBufferRotation.Current;
      }
    }

    public override void OnSimulateAfter(NetworkObj obj) {
      var nt = obj.Storage.Values[obj[this] + POSITION].Transform;

      if (nt != null && nt.Simulate) {
        if (obj.RootState.Entity.IsOwner) {
          var oldPosition = obj.Storage.Values[obj[this] + POSITION].Vector3;
          var oldVelocity = obj.Storage.Values[obj[this] + VELOCITY].Vector3;
          var oldRotation = obj.Storage.Values[obj[this] + ROTATION].Quaternion;

          obj.Storage.Values[obj[this] + POSITION].Vector3 = nt.Simulate.localPosition;
          obj.Storage.Values[obj[this] + VELOCITY].Vector3 = CalculateVelocity(nt, oldPosition);
          obj.Storage.Values[obj[this] + ROTATION].Quaternion = nt.Simulate.localRotation;

          var positionChanged = oldPosition != obj.Storage.Values[obj[this] + POSITION].Vector3;
          var velocityChanged = oldVelocity != obj.Storage.Values[obj[this] + VELOCITY].Vector3;
          var rotationChanged = oldRotation != obj.Storage.Values[obj[this] + ROTATION].Quaternion;

          if (positionChanged || velocityChanged || rotationChanged) {
            obj.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);
          }
        }

        nt.RenderDoubleBufferPosition = nt.RenderDoubleBufferPosition.Shift(nt.Simulate.position);
        nt.RenderDoubleBufferRotation = nt.RenderDoubleBufferRotation.Shift(nt.Simulate.rotation);
      }
    }

    public override void OnSimulateBefore(NetworkObj obj) {
      if (obj.RootState.Entity.IsDummy) {
        var nt = obj.Storage.Values[obj[this]].Transform;
        if (nt != null && nt.Simulate) {
          var snapped = false;
          if (Extrapolation.Enabled) {
            UE.Vector3 pos;
            UE.Quaternion rot;

            pos = Math.ExtrapolateVector(
              /* currentPosition */   nt.Simulate.localPosition,
              /* receivedPosition */  obj.Storage.Values[obj[this] + POSITION].Vector3,
              /* receivedVelocity */  obj.Storage.Values[obj[this] + VELOCITY].Vector3,
              /* receivedFrame */     obj.RootState.Frames.first.Frame,
              /* entityFrame */       obj.RootState.Entity.Frame,
              /* extrapolation */     Extrapolation,
              /* snapping */          ref snapped
            );

            rot = Math.ExtrapolateQuaternion(
              /* currentRotation */   nt.Simulate.localRotation,
              /* receivedRotation */  obj.Storage.Values[obj[this] + ROTATION].Quaternion,
              /* receivedFrame */     obj.RootState.Frames.first.Frame,
              /* entityFrame */       obj.RootState.Entity.Frame,
              /* extrapolation */     Extrapolation
            );

            nt.Simulate.localPosition = nt.Clamper(obj.RootState.Entity.UnityObject, pos);
            nt.Simulate.localRotation = rot;
          }
          else if (Interpolation.Enabled) {
            // position
            nt.Simulate.localPosition = Math.InterpolateVector(
              obj.RootState.Frames,
              obj[this] + POSITION,
              obj.RootState.Entity.Frame,
              Interpolation.SnapMagnitude,
              ref snapped
            );

            // rotation
            nt.Simulate.localRotation = Math.InterpolateQuaternion(
              obj.RootState.Frames,
              obj[this] + ROTATION,
              obj.RootState.Entity.Frame
            );
          }
          else {
            // always snapped on this
            snapped = true;

            // position
            nt.Simulate.localPosition = obj.Storage.Values[obj[this] + POSITION].Vector3;

            // rotation
            nt.Simulate.localRotation = obj.Storage.Values[obj[this] + ROTATION].Quaternion;
          }

          if (snapped) {
            nt.RenderDoubleBufferPosition =
              nt.RenderDoubleBufferPosition
                .Shift(nt.Simulate.position)
                .Shift(nt.Simulate.position);
          }
        }
      }
    }

    public override void OnParentChanged(NetworkObj obj, Entity newParent, Entity oldParent) {
      var nt = obj.Storage.Values[obj[this] + POSITION].Transform;
      if (nt != null && nt.Simulate) {
        if (newParent == null) {
          nt.Simulate.transform.parent = null;
          UpdateTransformValues(obj, oldParent.UnityObject.transform.localToWorldMatrix, UE.Matrix4x4.identity);
        }
        else if (oldParent == null) {
          nt.Simulate.transform.parent = newParent.UnityObject.transform;
          UpdateTransformValues(obj, UE.Matrix4x4.identity, newParent.UnityObject.transform.worldToLocalMatrix);
        }
        else {
          nt.Simulate.transform.parent = newParent.UnityObject.transform;
          UpdateTransformValues(obj, oldParent.UnityObject.transform.localToWorldMatrix, newParent.UnityObject.transform.worldToLocalMatrix);
        }
      }
    }

    UE.Vector3 CalculateVelocity(NetworkTransform nt, UE.Vector3 position) {
      switch (Extrapolation.VelocityMode) {
        case ExtrapolationVelocityModes.CalculateFromPosition:
          return (nt.Simulate.localPosition - position) * BoltCore._config.framesPerSecond;

        case ExtrapolationVelocityModes.CopyFromRigidbody:
          return nt.Simulate.rigidbody.velocity;

        case ExtrapolationVelocityModes.CopyFromRigidbody2D:
          return nt.Simulate.rigidbody2D.velocity;

        case ExtrapolationVelocityModes.CopyFromCharacterController:
          return nt.Simulate.GetComponent<UE.CharacterController>().velocity;

        default:
          BoltLog.Error("Unknown velocity extrapolation mode {0}", Extrapolation.VelocityMode);
          return (nt.Simulate.localPosition - position) * BoltCore._config.framesPerSecond;
      }
    }

    void UpdateTransformValues(NetworkObj obj, UE.Matrix4x4 l2w, UE.Matrix4x4 w2l) {
      var it = obj.RootState.Frames.GetIterator();

      while (it.Next()) {
        var p = obj.Storage.Values[obj[this] + POSITION].Vector3;
        var r = obj.Storage.Values[obj[this] + ROTATION].Quaternion;

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
        obj.Storage.Values[obj[this] + POSITION].Vector3 = p;
        obj.Storage.Values[obj[this] + ROTATION].Quaternion = r;
      }
    }
  }
}

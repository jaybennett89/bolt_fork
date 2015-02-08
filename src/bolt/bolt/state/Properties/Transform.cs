using System;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  internal class NetworkProperty_Transform : NetworkProperty {
    const int POSITION = 0;
    const int ROTATION = 1;
    const int VELOCITY = 2;

    Int32 PositionMask;
    Int32 RotationMask;

    Boolean PositionEnabled = false;
    Boolean RotationEnabled = false;

    PropertyExtrapolationSettings Extrapolation;
    PropertyQuaternionCompression RotationCompression;
    PropertyVectorCompressionSettings PositionCompression;

    public void Settings_Vector(PropertyFloatCompressionSettings x, PropertyFloatCompressionSettings y, PropertyFloatCompressionSettings z, bool strict) {
      PositionEnabled = true;
      PositionCompression = PropertyVectorCompressionSettings.Create(x, y, z, strict);

      if (PositionCompression.X.BitsRequired > 0) { PositionMask |= 1; }
      if (PositionCompression.Y.BitsRequired > 0) { PositionMask |= 2; }
      if (PositionCompression.Z.BitsRequired > 0) { PositionMask |= 4; }
    }

    public void Settings_Quaternion(PropertyFloatCompressionSettings compression, bool strict) {
      RotationEnabled = true;
      RotationCompression = PropertyQuaternionCompression.Create(compression, strict);
    }

    public void Settings_QuaternionEuler(PropertyFloatCompressionSettings x, PropertyFloatCompressionSettings y, PropertyFloatCompressionSettings z, bool strict) {
      RotationEnabled = true;
      RotationCompression = PropertyQuaternionCompression.Create(PropertyVectorCompressionSettings.Create(x, y, z, strict));

      if (RotationCompression.Euler.X.BitsRequired > 0) { RotationMask |= 1; }
      if (RotationCompression.Euler.Y.BitsRequired > 0) { RotationMask |= 2; }
      if (RotationCompression.Euler.Z.BitsRequired > 0) { RotationMask |= 4; }
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

    UE.Vector3 GetLocalPosition(UE.Transform t) {
      if (PositionMask == 7) {
        return t.localPosition;
      }
      else {
        UE.Vector3 p = t.localPosition;

        switch (PositionMask) {
          case 6: p.x = 0; break;
          case 5: p.y = 0; break;
          case 4: p.x = 0; p.y = 0; break;
          case 3: p.z = 0; break;
          case 2: p.x = 0; p.z = 0; break;
          case 1: p.y = 0; p.z = 0; break;
        }

        return p;
      }
    }

    void SetLocalPosition(UE.Transform t, UE.Vector3 p) {
      if (PositionMask == 7) {
        t.localPosition = p;
      }
      else {
        UE.Vector3 c = t.localPosition;

        switch (PositionMask) {
          case 6: t.localPosition = new UE.Vector3(c.x, p.y, p.z); break;
          case 5: t.localPosition = new UE.Vector3(p.x, c.y, p.z); break;
          case 4: t.localPosition = new UE.Vector3(c.x, c.y, p.z); break;
          case 3: t.localPosition = new UE.Vector3(p.x, p.y, c.z); break;
          case 2: t.localPosition = new UE.Vector3(c.x, p.y, c.z); break;
          case 1: t.localPosition = new UE.Vector3(p.x, c.y, c.z); break;
        }
      }
    }

    void SetLocalRotation(UE.Transform t, UE.Quaternion q) {
      if (RotationMask == 0 || RotationMask == 7) {
        t.localRotation = q;
      }
      else {
        UE.Vector3 r = q.eulerAngles;
        UE.Vector3 c = t.localRotation.eulerAngles;

        switch (RotationMask) {
          case 6: c.y = r.y; c.z = r.z; break;
          case 5: c.x = r.x; c.z = r.z; break;
          case 4: c.z = r.z; break;
          case 3: c.x = r.x; c.y = r.y; break;
          case 2: c.y = r.y; break;
          case 1: c.x = r.x; break;
        }

        t.localRotation = UE.Quaternion.Euler(c);
      }
    }

    UE.Quaternion GetLocalRotation(UE.Transform t) {
      if (RotationMask == 0 || RotationMask == 7) {
        return t.localRotation;
      }
      else {
        UE.Vector3 r = t.localRotation.eulerAngles;

        switch (RotationMask) {
          case 6: r.x = 0; break;
          case 5: r.y = 0; break;
          case 4: r.x = 0; r.y = 0; break;
          case 3: r.z = 0; break;
          case 2: r.x = 0; r.z = 0; break;
          case 1: r.y = 0; r.z = 0; break;
        }

        return UE.Quaternion.Euler(r);
      }
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

      if (PositionEnabled) {
        PositionCompression.Pack(packet, storage.Values[obj[this] + POSITION].Vector3);

        if (Extrapolation.Enabled) {
          PositionCompression.Pack(packet, storage.Values[obj[this] + VELOCITY].Vector3);
        }
      }

      if (RotationEnabled) {
        RotationCompression.Pack(packet, storage.Values[obj[this] + ROTATION].Quaternion);
      }

      return true;
    }

    public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      obj.RootState.Entity.SetParentInternal(packet.ReadEntity());

      if (PositionEnabled) {
        storage.Values[obj[this] + POSITION].Vector3 = PositionCompression.Read(packet);

        if (Extrapolation.Enabled) {
          storage.Values[obj[this] + VELOCITY].Vector3 = PositionCompression.Read(packet);
        }
      }

      if (RotationEnabled) {
        storage.Values[obj[this] + ROTATION].Quaternion = RotationCompression.Read(packet);
      }
    }

    public override void OnRender(NetworkObj obj) {
      var nt = obj.Storage.Values[obj[this] + POSITION].Transform;
      if (nt != null && nt.Render) {
        if (PositionEnabled) {
          var p = nt.RenderDoubleBufferPosition.Previous;
          var c = nt.RenderDoubleBufferPosition.Current;
          nt.Render.position = UE.Vector3.Lerp(p, c, BoltCore.frameAlpha);
        }

        if (RotationEnabled) {
          nt.Render.rotation = nt.RenderDoubleBufferRotation.Current;
        }
      }
    }

    public override void OnSimulateAfter(NetworkObj obj) {
      var nt = obj.Storage.Values[obj[this] + POSITION].Transform;

      if (nt != null && nt.Simulate) {
        if (obj.RootState.Entity.IsOwner) {
          var oldPosition = obj.Storage.Values[obj[this] + POSITION].Vector3;
          var oldVelocity = obj.Storage.Values[obj[this] + VELOCITY].Vector3;
          var oldRotation = obj.Storage.Values[obj[this] + ROTATION].Quaternion;

          obj.Storage.Values[obj[this] + POSITION].Vector3 = GetLocalPosition(nt.Simulate);
          obj.Storage.Values[obj[this] + VELOCITY].Vector3 = CalculateVelocity(nt, oldPosition);
          obj.Storage.Values[obj[this] + ROTATION].Quaternion = GetLocalRotation(nt.Simulate);

          var positionChanged = false;
          var velocityChanged = false;
          var rotationChanged = false;

          if (PositionCompression.StrictComparison) {
            positionChanged = NetworkValue.Diff_Strict(oldPosition, obj.Storage.Values[obj[this] + POSITION].Vector3);
            velocityChanged = NetworkValue.Diff_Strict(oldVelocity, obj.Storage.Values[obj[this] + VELOCITY].Vector3);
          }
          else {
            positionChanged = oldPosition != obj.Storage.Values[obj[this] + POSITION].Vector3;
            velocityChanged = oldVelocity != obj.Storage.Values[obj[this] + VELOCITY].Vector3;
          }

          if (RotationCompression.StrictComparison) {
            rotationChanged = NetworkValue.Diff_Strict(oldRotation, obj.Storage.Values[obj[this] + ROTATION].Quaternion);
          }
          else {
            rotationChanged = oldRotation != obj.Storage.Values[obj[this] + ROTATION].Quaternion;
          }

          if (positionChanged || velocityChanged || rotationChanged) {
            obj.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);
          }
        }

        nt.RenderDoubleBufferPosition = nt.RenderDoubleBufferPosition.Shift(nt.Simulate.position);
        nt.RenderDoubleBufferRotation = nt.RenderDoubleBufferRotation.Shift(nt.Simulate.rotation);
      }
    }

    public override void OnSimulateBefore(NetworkObj obj) {
      var root = (NetworkState)obj.Root;

      if (root.Entity.IsOwner) {
        return;
      }

      if (root.Entity.HasControl && !ToController) {
        return;
      }

      var nt = obj.Storage.Values[obj[this]].Transform;
      if (nt != null && nt.Simulate) {
        var snapped = false;

        UE.Vector3 pos = UE.Vector3.zero;
        UE.Quaternion rot = UE.Quaternion.identity;

        if (Extrapolation.Enabled) {
          if (PositionEnabled) {
            pos = Math.ExtrapolateVector(
              /* currentPosition */   GetLocalPosition(nt.Simulate),
              /* receivedPosition */  obj.Storage.Values[obj[this] + POSITION].Vector3,
              /* receivedVelocity */  obj.Storage.Values[obj[this] + VELOCITY].Vector3,
              /* receivedFrame */     obj.RootState.Frames.first.Frame,
              /* entityFrame */       obj.RootState.Entity.Frame,
              /* extrapolation */     Extrapolation,
              /* snapping */          ref snapped
            );

            // clamp position
            pos = nt.Clamper(obj.RootState.Entity.UnityObject, pos);
          }

          if (RotationEnabled) {
            rot = Math.ExtrapolateQuaternion(
              /* currentRotation */   GetLocalRotation(nt.Simulate),
              /* receivedRotation */  obj.Storage.Values[obj[this] + ROTATION].Quaternion,
              /* receivedFrame */     obj.RootState.Frames.first.Frame,
              /* entityFrame */       obj.RootState.Entity.Frame,
              /* extrapolation */     Extrapolation
            );
          }
        }
        else if (Interpolation.Enabled) {
          // position
          if (PositionEnabled) {
            pos = Math.InterpolateVector(
              obj.RootState.Frames,
              obj[this] + POSITION,
              obj.RootState.Entity.Frame,
              Interpolation.SnapMagnitude,
              ref snapped
            );
          }

          // rotation
          if (RotationEnabled) {
            rot = Math.InterpolateQuaternion(
              obj.RootState.Frames,
              obj[this] + ROTATION,
              obj.RootState.Entity.Frame
            );
          }
        }
        else {
          // always snapped on this
          snapped = true;

          // position
          if (PositionEnabled) {
            pos = obj.Storage.Values[obj[this] + POSITION].Vector3;
          }

          // rotation
          if (RotationEnabled) {
            rot = obj.Storage.Values[obj[this] + ROTATION].Quaternion;
          }
        }

        if (PositionEnabled) {
          SetLocalPosition(nt.Simulate, pos);

          if (snapped) {
            nt.RenderDoubleBufferPosition = nt.RenderDoubleBufferPosition.Shift(nt.Simulate.position).Shift(nt.Simulate.position);
          }
        }

        if (RotationEnabled) {
          SetLocalRotation(nt.Simulate, rot);
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
          return (GetLocalPosition(nt.Simulate) - position) * BoltCore._config.framesPerSecond;

        case ExtrapolationVelocityModes.CopyFromRigidbody:
          return nt.Simulate.GetComponent<UE.Rigidbody>().velocity;

        case ExtrapolationVelocityModes.CopyFromRigidbody2D:
          return nt.Simulate.GetComponent<UE.Rigidbody2D>().velocity;

        case ExtrapolationVelocityModes.CopyFromCharacterController:
          return nt.Simulate.GetComponent<UE.CharacterController>().velocity;

        default:
          BoltLog.Error("Unknown velocity extrapolation mode {0}", Extrapolation.VelocityMode);
          return (GetLocalPosition(nt.Simulate) - position) * BoltCore._config.framesPerSecond;
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

using System;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  internal struct PropertySerializerTransformData {
    public Axis[] PositionAxes;
    public Axis[] RotationAxes;
    public FloatCompression QuaternionCompression;
  }

  public class TransformData {
    internal UE.Transform Simulate;
    internal UE.Transform Render;
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

  class PropertySerializerTransform : PropertySerializer {
    PropertySerializerTransformData PropertyData;

    public PropertySerializerTransform(StatePropertyMetaData info)
      : base(info) {
    }

    public void SetPropertyData(PropertySerializerTransformData propertyData) {
      PropertyData = propertyData;
    }

    public override int StateBits(State state, State.Frame frame) {
      return ((12 + 16) * 8) + 1;
    }

    public override object GetDebugValue(State state) {
      var td = (TransformData)state.Frames.first.Objects[StateData.ObjectOffset];
      if (td.Simulate) {
        var p = state.Frames.first.Data.ReadVector3(StateData.ByteOffset);
        var r = state.Frames.first.Data.ReadQuaternion(StateData.ByteOffset + 12).eulerAngles;
        var pos = string.Format("X:{0} Y:{1} Z:{2}", p.x.ToString("F3"), p.y.ToString("F3"), p.z.ToString("F3"));
        var rot = string.Format("X:{0} Y:{1} Z:{2}", r.x.ToString("F3"), r.y.ToString("F3"), r.z.ToString("F3"));
        return string.Format("{0} / {1}", pos, rot);
      }
      else {
        return "NOT ASSIGNED";
      }
    }

    public override void OnInit(State state) {
      state.PropertyObjects[StateData.ObjectOffset] = new TransformData();
    }

    public override void OnRender(State state, State.Frame frame) {
      var td = (TransformData)state.Frames.first.Objects[StateData.ObjectOffset];
      if (td.Render) {
        var p = td.RenderDoubleBufferPosition.Previous;
        var c = td.RenderDoubleBufferPosition.Current;
        td.Render.position = UE.Vector3.Lerp(p, c, BoltCore.frameAlpha);
        td.Render.rotation = td.RenderDoubleBufferRotation.Current;
      }
    }

    public override void OnParentChanged(State state, Entity newParent, Entity oldParent) {
      var td = (TransformData)state.Frames.first.Objects[StateData.ObjectOffset];
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
        UE.Vector3 p = it.val.Data.ReadVector3(StateData.ObjectOffset);
        UE.Quaternion r = it.val.Data.ReadQuaternion(StateData.ObjectOffset + 12);

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
        it.val.Data.PackVector3(StateData.ObjectOffset, p);
        it.val.Data.PackQuaternion(StateData.ObjectOffset + 12, r);
      }
    }

    public override void OnSimulateBefore(State state) {
      var td = (TransformData)state.Frames.first.Objects[StateData.ObjectOffset];
      if (td.Simulate && !state.Entity.IsOwner && !state.Entity.HasPredictedControl) {
        PerformInterpolation(td, state);

        //switch (Config.TransformMode) {
        //  case TransformModes.None:
        //    PerformNone(td, state);
        //    break;

        //  case TransformModes.Interpolate:
        //    PerformInterpolation(td, state);
        //    break;

        //  case TransformModes.Extrapolate:
        //    PerformExtrapolation(td, state);
        //    break;
        //}
      }
    }
    public override void OnSimulateAfter(State state) {
      var td = (TransformData)state.Frames.first.Objects[StateData.ObjectOffset];
      if (td.Simulate) {
        if (state.Entity.IsOwner) {
          state.Frames.first.Data.PackVector3(StateData.ByteOffset, td.Simulate.localPosition);
          state.Frames.first.Data.PackQuaternion(StateData.ByteOffset + 12, td.Simulate.localRotation);
        }

        td.RenderDoubleBufferPosition = td.RenderDoubleBufferPosition.Shift(td.Simulate.position);
        td.RenderDoubleBufferRotation = td.RenderDoubleBufferRotation.Shift(td.Simulate.rotation);
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

      UE.Vector3 p = frame.Data.ReadVector3(StateData.ByteOffset);
      UE.Quaternion r = frame.Data.ReadQuaternion(StateData.ByteOffset + 12);

      stream.WriteFloat(p.x);
      stream.WriteFloat(p.y);
      stream.WriteFloat(p.z);

      stream.WriteFloat(r.x);
      stream.WriteFloat(r.y);
      stream.WriteFloat(r.z);
      stream.WriteFloat(r.w);

      return true;

      //if (Config.PositionAxes[Axis.X].Enabled) Config.PositionAxes[Axis.X].Compression.Pack(stream, p.x);
      //if (Config.PositionAxes[Axis.Y].Enabled) Config.PositionAxes[Axis.Y].Compression.Pack(stream, p.y);
      //if (Config.PositionAxes[Axis.Z].Enabled) Config.PositionAxes[Axis.Z].Compression.Pack(stream, p.z);

      //switch (Config.RotationMode) {
      //  case TransformRotationMode.QuaternionComponents:
      //    Config.QuaternionCompression.Pack(stream, r.x);
      //    Config.QuaternionCompression.Pack(stream, r.y);
      //    Config.QuaternionCompression.Pack(stream, r.z);
      //    Config.QuaternionCompression.Pack(stream, r.w);
      //    break;

      //  case TransformRotationMode.EulerAngles:
      //    UE.Vector3 a = r.eulerAngles;

      //    if (Config.RotationAxes[Axis.X].Enabled) Config.RotationAxes[Axis.X].Compression.Pack(stream, a.x);
      //    if (Config.RotationAxes[Axis.Y].Enabled) Config.RotationAxes[Axis.Y].Compression.Pack(stream, a.y);
      //    if (Config.RotationAxes[Axis.Z].Enabled) Config.RotationAxes[Axis.Z].Compression.Pack(stream, a.z);

      //    break;
      //}
    }

    public override void StateRead(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      state.Entity.SetParentInternal(stream.ReadEntity(connection));

      UE.Vector3 p = new UE.Vector3();
      UE.Quaternion r = new UE.Quaternion();

      p.x = stream.ReadFloat();
      p.y = stream.ReadFloat();
      p.z = stream.ReadFloat();

      r.x = stream.ReadFloat();
      r.y = stream.ReadFloat();
      r.z = stream.ReadFloat();
      r.w = stream.ReadFloat();

      //if (Config.PositionAxes[Axis.X].Enabled) p.x = Config.PositionAxes[Axis.X].Compression.Read(stream);
      //if (Config.PositionAxes[Axis.Y].Enabled) p.y = Config.PositionAxes[Axis.X].Compression.Read(stream);
      //if (Config.PositionAxes[Axis.Z].Enabled) p.z = Config.PositionAxes[Axis.X].Compression.Read(stream);

      //switch (Config.RotationMode) {
      //  case TransformRotationMode.QuaternionComponents:
      //    r.x = Config.QuaternionCompression.Read(stream);
      //    r.y = Config.QuaternionCompression.Read(stream);
      //    r.z = Config.QuaternionCompression.Read(stream);
      //    r.w = Config.QuaternionCompression.Read(stream);
      //    break;

      //  case TransformRotationMode.EulerAngles:
      //    UE.Vector3 a = new UE.Vector3();

      //    if (Config.RotationAxes[Axis.X].Enabled) a.x = Config.RotationAxes[Axis.X].Compression.Read(stream);
      //    if (Config.RotationAxes[Axis.Y].Enabled) a.y = Config.RotationAxes[Axis.X].Compression.Read(stream);
      //    if (Config.RotationAxes[Axis.Z].Enabled) a.z = Config.RotationAxes[Axis.X].Compression.Read(stream);

      //    r = UE.Quaternion.Euler(a);
      //    break;
      //}

      frame.Data.PackVector3(StateData.ByteOffset, p);
      frame.Data.PackQuaternion(StateData.ByteOffset + 12, r);
    }

    void PerformNone(TransformData td, State state) {
      var f0 = state.Frames.first;
      UE.Vector3 p0 = f0.Data.ReadVector3(StateData.ByteOffset);
      UE.Quaternion r0 = f0.Data.ReadQuaternion(StateData.ByteOffset + 12);

      td.Simulate.localPosition = p0;
      td.Simulate.localRotation = r0;
    }

    void PerformInterpolation(TransformData td, State state) {
      var f0 = state.Frames.first;
      UE.Vector3 p0 = f0.Data.ReadVector3(StateData.ByteOffset);
      UE.Quaternion r0 = f0.Data.ReadQuaternion(StateData.ByteOffset + 12);

      if ((state.Frames.count == 1) || (f0.Number >= state.Entity.Frame)) {
        //BoltLog.Info("frame: {0}, f0.number={1}", state.Entity.Frame, f0.Number);
        td.Simulate.localPosition = p0;
        td.Simulate.localRotation = r0;
      }
      else {
        var f1 = state.Frames.Next(f0);
        UE.Vector3 p1 = f1.Data.ReadVector3(StateData.ByteOffset);
        UE.Quaternion r1 = f1.Data.ReadQuaternion(StateData.ByteOffset + 12);

        Assert.True(f1.Number > f0.Number);
        Assert.True(f1.Number > state.Entity.Frame);

        float t = f1.Number - f0.Number;
        float d = state.Entity.Frame - f0.Number;

        //BoltLog.Info("frame: {0}, f0.number={1}, f1.number={2}, d/t={3}", state.Entity.Frame, f0.Number, f1.Number, d / t);
        td.Simulate.localPosition = UE.Vector3.Lerp(p0, p1, d / t);
        td.Simulate.localRotation = UE.Quaternion.Lerp(r0, r1, d / t);
      }
    }

    void PerformExtrapolation(TransformData td, State state) {
      throw new NotImplementedException();
    }
  }
}

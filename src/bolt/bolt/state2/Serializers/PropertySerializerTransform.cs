using System;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  internal struct TransformConfiguration {
    public Axis[] PositionAxes;
    public Axis[] RotationAxes;
    public TransformSpace Space;
    public TransformRotationMode RotationMode;
    public FloatCompression QuaternionCompression;
  }

  internal enum TransformSpace {
    World = 0,
    Local = 1
  }

  internal enum TransformRotationMode {
    QuaternionComponents = 0,
    EulerAngles = 1
  }

  public class TransformData {
    public UE.Transform Render;
    public UE.Transform Simulate;
    public DoubleBuffer<UE.Vector3> RenderDoubleBuffer;
  }

  internal class PropertySerializerTransform : PropertySerializer {
    TransformConfiguration config;

    public PropertySerializerTransform(TransformConfiguration cfg, PropertyMetaData info)
      : base(info) {
      config = cfg;
    }

    public override int CalculateBits(byte[] data) {
      return (12 + 16) * 4;
    }

    public override void OnInit(State state) {
      state.PropertyObjects[MetaData.ObjectOffset] = new TransformData();
    }

    public override void OnRender(State state, State.Frame frame) {
      var td = (TransformData)state.Frames.first.Objects[MetaData.ObjectOffset];
      if (td.Render) {
        PositionSet(td.Render, UE.Vector3.Lerp(td.RenderDoubleBuffer.Value0, td.RenderDoubleBuffer.Value1, BoltCore.frameAlpha));
      }
    }

    public override void OnSimulateBefore(State state) {
      var td = (TransformData)state.Frames.first.Objects[MetaData.ObjectOffset];
      if (td.Simulate && !state.Entity.IsOwner && (!state.Entity.HasControl || !state.Entity.ClientPrediction)) {
        var f0 = state.Frames.first;
        UE.Vector3 p0 = f0.Data.ReadVector3(MetaData.ByteOffset);
        UE.Quaternion r0 = f0.Data.ReadQuaternion(MetaData.ByteOffset + 12);

        if ((state.Frames.count == 1) || (f0.Number >= state.Entity.Frame)) {
          //BoltLog.Info("frame: {0}, f0.number={1}", state.Entity.Frame, f0.Number);

          PositionSet(td.Simulate, p0);
          RotationGet(td.Simulate, r0);
        }
        else {
          var f1 = state.Frames.Next(f0);
          UE.Vector3 p1 = f1.Data.ReadVector3(MetaData.ByteOffset);
          UE.Quaternion r1 = f1.Data.ReadQuaternion(MetaData.ByteOffset + 12);

          Assert.True(f1.Number > f0.Number);
          Assert.True(f1.Number > state.Entity.Frame);

          float t = f1.Number - f0.Number;
          float d = state.Entity.Frame - f0.Number;

          //BoltLog.Info("frame: {0}, f0.number={1}, f1.number={2}, d/t={3}", state.Entity.Frame, f0.Number, f1.Number, d / t);

          PositionSet(td.Simulate, UE.Vector3.Lerp(p0, p1, d / t));
          RotationGet(td.Simulate, UE.Quaternion.Lerp(r0, r1, d / t));
        }
      }
    }
    public override void OnSimulateAfter(State state) {
      var td = (TransformData)state.Frames.first.Objects[MetaData.ObjectOffset];
      if (td.Simulate) {
        if (state.Entity.IsOwner) {
          state.Frames.first.Data.PackVector3(MetaData.ByteOffset, td.Simulate.position);
          state.Frames.first.Data.PackQuaternion(MetaData.ByteOffset + 12, td.Simulate.rotation);
        }

        if (td.Render) {
          td.RenderDoubleBuffer = td.RenderDoubleBuffer.Shift(td.Simulate.position);
        }
      }
    }

    public override void Pack(State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      UE.Vector3 p = frame.Data.ReadVector3(MetaData.ByteOffset);
      UE.Quaternion r = frame.Data.ReadQuaternion(MetaData.ByteOffset + 12);

      if (config.PositionAxes[Axis.X].Enabled) config.PositionAxes[Axis.X].Compression.Pack(stream, p.x);
      if (config.PositionAxes[Axis.Y].Enabled) config.PositionAxes[Axis.Y].Compression.Pack(stream, p.y);
      if (config.PositionAxes[Axis.Z].Enabled) config.PositionAxes[Axis.Z].Compression.Pack(stream, p.z);

      switch (config.RotationMode) {
        case TransformRotationMode.QuaternionComponents:
          config.QuaternionCompression.Pack(stream, r.x);
          config.QuaternionCompression.Pack(stream, r.y);
          config.QuaternionCompression.Pack(stream, r.z);
          config.QuaternionCompression.Pack(stream, r.w);
          break;

        case TransformRotationMode.EulerAngles:
          UE.Vector3 a = r.eulerAngles;

          if (config.RotationAxes[Axis.X].Enabled) config.RotationAxes[Axis.X].Compression.Pack(stream, a.x);
          if (config.RotationAxes[Axis.Y].Enabled) config.RotationAxes[Axis.Y].Compression.Pack(stream, a.y);
          if (config.RotationAxes[Axis.Z].Enabled) config.RotationAxes[Axis.Z].Compression.Pack(stream, a.z);
          break;
      }
    }

    public override void Read(State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      UE.Vector3 p = new UE.Vector3();
      UE.Quaternion r = new UE.Quaternion();

      if (config.PositionAxes[Axis.X].Enabled) p.x = config.PositionAxes[Axis.X].Compression.Read(stream);
      if (config.PositionAxes[Axis.Y].Enabled) p.y = config.PositionAxes[Axis.X].Compression.Read(stream);
      if (config.PositionAxes[Axis.Z].Enabled) p.z = config.PositionAxes[Axis.X].Compression.Read(stream);

      switch (config.RotationMode) {
        case TransformRotationMode.QuaternionComponents:
          r.x = config.QuaternionCompression.Read(stream);
          r.y = config.QuaternionCompression.Read(stream);
          r.z = config.QuaternionCompression.Read(stream);
          r.w = config.QuaternionCompression.Read(stream);
          break;

        case TransformRotationMode.EulerAngles:
          UE.Vector3 a = new UE.Vector3();

          if (config.RotationAxes[Axis.X].Enabled) a.x = config.RotationAxes[Axis.X].Compression.Read(stream);
          if (config.RotationAxes[Axis.Y].Enabled) a.y = config.RotationAxes[Axis.X].Compression.Read(stream);
          if (config.RotationAxes[Axis.Z].Enabled) a.z = config.RotationAxes[Axis.X].Compression.Read(stream);

          r = UE.Quaternion.Euler(a);
          break;
      }

      frame.Data.PackVector3(MetaData.ByteOffset, p);
      frame.Data.PackQuaternion(MetaData.ByteOffset + 12, r);
    }

    UE.Vector3 PositionGet(UE.Transform t) { return config.Space == TransformSpace.World ? t.position : t.localPosition; }
    UE.Quaternion RotationGet(UE.Transform t) { return config.Space == TransformSpace.World ? t.rotation : t.localRotation; }

    void PositionSet(UE.Transform t, UE.Vector3 pos) {
      if (config.Space == TransformSpace.World) { t.position = pos; } else { t.localPosition = pos; }
    }

    void RotationGet(UE.Transform t, UE.Quaternion rot) {
      if (config.Space == TransformSpace.World) { t.rotation = rot; } else { t.localRotation = rot; }
    }
  }
}

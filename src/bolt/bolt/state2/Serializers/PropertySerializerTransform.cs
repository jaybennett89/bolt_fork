using System;
using UE = UnityEngine;

namespace Bolt {
  internal class PropertySerializerTransform : PropertySerializer {
    public PropertySerializerTransform(PropertyMetaData info)
      : base(info) {
    }

    public override int CalculateBits(byte[] data) {
      return (12 + 16) * 4;
    }

    public override void OnSimulateBefore(State state) {
      if (!state.Entity.IsOwner && (!state.Entity.HasControl || !state.Entity.ClientPrediction)) {
        var transform = state.Frames.first.Objects[MetaData.ObjectOffset] as UE.Transform;
        if (transform) {
          var f0 = state.Frames.first;
          UE.Vector3 p0 = f0.Data.ReadVector3(MetaData.ByteOffset);
          UE.Quaternion r0 = f0.Data.ReadQuaternion(MetaData.ByteOffset + 12);

          if ((state.Frames.count == 1) || (f0.Number >= state.Entity.Frame)) {
            //BoltLog.Info("frame: {0}, f0.number={1}", state.Entity.Frame, f0.Number);

            transform.position = p0;
            transform.rotation = r0;
          }
          else {
            var f1 = state.Frames.Next(f0);
            UE.Vector3 p1 = f1.Data.ReadVector3(MetaData.ByteOffset);
            UE.Quaternion r1 = f1.Data.ReadQuaternion(MetaData.ByteOffset + 12);

            Assert.True(f1.Number > f0.Number);
            Assert.True(f1.Number > state.Entity.Frame);

            float t = f1.Number - f0.Number;
            float d = state.Entity.Frame - f0.Number;

            //BoltLog.Info("frame: {0}, f0.number={1}, f1.number={2}, a/d={3}", state.Entity.Frame, f0.Number, f1.Number, d / t);

            transform.position = UE.Vector3.Lerp(p0, p1, d / t);
            transform.rotation = UE.Quaternion.Lerp(r0, r1, d / t);
          }
        }
      }
    }

    public override void OnSimulateAfter(State state) {
      if (state.Entity.IsOwner) {
        var transform = state.Frames.first.Objects[MetaData.ObjectOffset] as UE.Transform;
        if (transform) {
          UE.Vector3 p = transform.position;
          UE.Quaternion r = transform.rotation;

          state.Frames.first.Data.PackF32(MetaData.ByteOffset + 0, p.x);
          state.Frames.first.Data.PackF32(MetaData.ByteOffset + 4, p.y);
          state.Frames.first.Data.PackF32(MetaData.ByteOffset + 8, p.z);

          state.Frames.first.Data.PackF32(MetaData.ByteOffset + 12, r.x);
          state.Frames.first.Data.PackF32(MetaData.ByteOffset + 16, r.y);
          state.Frames.first.Data.PackF32(MetaData.ByteOffset + 20, r.z);
          state.Frames.first.Data.PackF32(MetaData.ByteOffset + 24, r.w);
        }
      }
    }

    public override void Pack(State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteFloat(frame.Data.ReadF32(MetaData.ByteOffset + 0));
      stream.WriteFloat(frame.Data.ReadF32(MetaData.ByteOffset + 4));
      stream.WriteFloat(frame.Data.ReadF32(MetaData.ByteOffset + 8));
      stream.WriteFloat(frame.Data.ReadF32(MetaData.ByteOffset + 12));
      stream.WriteFloat(frame.Data.ReadF32(MetaData.ByteOffset + 16));
      stream.WriteFloat(frame.Data.ReadF32(MetaData.ByteOffset + 20));
      stream.WriteFloat(frame.Data.ReadF32(MetaData.ByteOffset + 24));
    }

    public override void Read(State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      frame.Data.PackF32(MetaData.ByteOffset + 0, stream.ReadFloat());
      frame.Data.PackF32(MetaData.ByteOffset + 4, stream.ReadFloat());
      frame.Data.PackF32(MetaData.ByteOffset + 8, stream.ReadFloat());
      frame.Data.PackF32(MetaData.ByteOffset + 12, stream.ReadFloat());
      frame.Data.PackF32(MetaData.ByteOffset + 16, stream.ReadFloat());
      frame.Data.PackF32(MetaData.ByteOffset + 20, stream.ReadFloat());
      frame.Data.PackF32(MetaData.ByteOffset + 24, stream.ReadFloat());
    }
  }
}

using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  class PropertySerializerQuaternion : PropertySerializerSimple {
    PropertyQuaternionCompression QuaternionCompression;

    public void AddSettings(PropertyQuaternionCompression quaternionCompression) {
      QuaternionCompression = quaternionCompression;
    }

    public override object GetDebugValue(State state) {
      var q = state.CurrentFrame.Storage[Settings.OffsetStorage].Quaternion;
      return string.Format("X:{0} Y:{1} Z:{2}", q.x.ToString("F3"), q.y.ToString("F3"), q.z.ToString("F3"));
    }

    public override void OnSimulateBefore(State state) {
      if (state.Entity.IsDummy) {
        var f = state.Frames.first;

        switch (SmoothingSettings.Algorithm) {
          case SmoothingAlgorithms.Interpolation:
            //f.Storage[Settings.OffsetStorage].Quaternion = Bolt.Math.InterpolateQuaternion(state.Frames, Settings.OffsetStorage, state.Entity.ServerFrame);
            break;

          case SmoothingAlgorithms.Extrapolation:
            //f.Storage[Settings.OffsetStorage].Quaternion = Bolt.Math.ExtrapolateQuaternion(state.Frames, Settings.OffsetStorage, state.Entity.ServerFrame, SmoothingSettings);
            break;
        }
      }
    }

    public override int StateBits(State state, NetworkFrame frame) {
      return QuaternionCompression.BitsRequired;
    }

    protected override bool Pack(NetworkValue[] data, BoltConnection connection, UdpPacket stream) {
      QuaternionCompression.Pack(stream, data[Settings.OffsetStorage].Quaternion);
      return true;
    }

    protected override void Read(NetworkValue[] data, BoltConnection connection, UdpPacket stream) {
      data[Settings.OffsetStorage].Quaternion = QuaternionCompression.Read(stream);
    }

    public override void CommandSmooth(NetworkValue[] from, NetworkValue[] to, NetworkValue[] into, float t) {
      var v0 = from[Settings.OffsetStorage].Quaternion;
      var v1 = to[Settings.OffsetStorage].Quaternion;
      into[Settings.OffsetStorage].Quaternion = UE.Quaternion.Lerp(v0, v1, t);
    }
  }
}

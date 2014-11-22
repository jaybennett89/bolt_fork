using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  class PropertySerializerVector : PropertySerializerSimple {
    PropertyVectorCompressionSettings VectorCompression;

    public void AddSettings(PropertyVectorCompressionSettings vectorCompression) {
      VectorCompression = vectorCompression;
    }

    public override void OnSimulateBefore(State state) {
      if (state.Entity.IsDummy) {
        var f = state.Frames.first;

        switch (SmoothingSettings.Algorithm) {
          case SmoothingAlgorithms.Interpolation:
          case SmoothingAlgorithms.Extrapolation:
            var snap = false;
            var snapMagnitude = SmoothingSettings.SnapMagnitude == 0f ? 1 << 16 : SmoothingSettings.SnapMagnitude;
            //f.Storage[Settings.OffsetStorage].Vector3 = Bolt.Math.InterpolateVector(state.Frames, Settings.OffsetStorage + 1, state.Entity.ServerFrame, snapMagnitude, ref snap);
            break;
        }
      }
    }

    public override object GetDebugValue(State state) {
      var v = state.CurrentFrame.Storage[Settings.OffsetStorage].Vector3;
      return string.Format("X:{0} Y:{1} Z:{2}", v.x.ToString("F3"), v.y.ToString("F3"), v.z.ToString("F3"));
    }

    public override int StateBits(State state, NetworkFrame frame) {
      return VectorCompression.BitsRequired;
    }

    protected override bool Pack(NetworkValue[] storage, BoltConnection connection, UdpPacket stream) {
      VectorCompression.Pack(stream, storage[Settings.OffsetStorage].Vector3);
      return true;
    }

    protected override void Read(NetworkValue[] storage, BoltConnection connection, UdpPacket stream) {
      storage[Settings.OffsetStorage].Vector3 = VectorCompression.Read(stream);
    }

    public override void CommandSmooth(NetworkValue[] from, NetworkValue[] to, NetworkValue[] into, float t) {
      var v0 = from[Settings.OffsetStorage].Vector3;
      var v1 = to[Settings.OffsetStorage].Vector3;
      var m = (v1 - v0).sqrMagnitude;

      if ((CommandSettings.SmoothCorrections == false) || (m > (SmoothingSettings.SnapMagnitude * SmoothingSettings.SnapMagnitude))) {
        into[Settings.OffsetStorage].Vector3 = v1;
      }
      else {
        into[Settings.OffsetStorage].Vector3 = UE.Vector3.Lerp(v0, v1, t);
      }
    }
  }
}

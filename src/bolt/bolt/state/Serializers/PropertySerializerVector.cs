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

    public new void AddSettings(PropertyStateSettings stateSettings) {
      Assert.True(stateSettings.ByteLength == 24);
      StateSettings = stateSettings;
      StateSettings.ByteLength = 12;
    }

    public override void OnSimulateBefore(State state) {
      if (state.Entity.IsDummy) {
        var f = state.Frames.first;

        switch (SmoothingSettings.Algorithm) {
          case SmoothingAlgorithms.Interpolation:
          case SmoothingAlgorithms.Extrapolation:
            var snap = false;
            var snapMagnitude = SmoothingSettings.SnapMagnitude == 0f ? 1 << 16 : SmoothingSettings.SnapMagnitude;

            f.Data.PackVector3(SettingsOld.ByteOffset, Bolt.Math.InterpolateVector(state.Frames, SettingsOld.ByteOffset + 12, state.Entity.Frame, snapMagnitude, ref snap));

            break;
        }
      }
    }

    public override object GetDebugValue(State state) {
      var v = Blit.ReadVector3(state.Frames.first.Data, SettingsOld.ByteOffset);
      return string.Format("X:{0} Y:{1} Z:{2}", v.x.ToString("F3"), v.y.ToString("F3"), v.z.ToString("F3"));
    }

    public override int StateBits(State state, State.NetworkFrame frame) {
      return VectorCompression.BitsRequired;
    }

    protected override bool Pack(byte[] data, BoltConnection connection, UdpPacket stream) {
      VectorCompression.Pack(stream, Blit.ReadVector3(data, SettingsOld.ByteOffset));
      return true;
    }

    protected override void Read(byte[] data, BoltConnection connection, UdpPacket stream) {
      Blit.PackVector3(data, SettingsOld.ByteOffset, VectorCompression.Read(stream));
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {
      var v0 = from.ReadVector3(SettingsOld.ByteOffset);
      var v1 = to.ReadVector3(SettingsOld.ByteOffset);
      var m = (v1 - v0).sqrMagnitude;

      if ((CommandSettings.SmoothCorrections == false) || (m > (CommandSettings.SnapMagnitude * CommandSettings.SnapMagnitude))) {
        into.PackVector3(SettingsOld.ByteOffset, v1);
      }
      else {
        into.PackVector3(SettingsOld.ByteOffset, UE.Vector3.Lerp(v0, v1, t));
      }
    }
  }
}

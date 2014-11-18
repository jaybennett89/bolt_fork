using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  class PropertySerializerQuaternion : PropertySerializerSimple {
    PropertyQuaternionCompression QuaternionCompression;

    public void AddSettings(PropertyQuaternionCompression quaternionCompression) {
      QuaternionCompression = quaternionCompression;
    }

    public new void AddSettings(PropertyStateSettings stateSettings) {
      Assert.True(stateSettings.ByteLength == 32);
      StateSettings = stateSettings;
      StateSettings.ByteLength = 16;
    }

    public override object GetDebugValue(State state) {
      var q = Blit.ReadQuaternion(state.Frames.first.Data, SettingsOld.ByteOffset).eulerAngles;
      return string.Format("X:{0} Y:{1} Z:{2}", q.x.ToString("F3"), q.y.ToString("F3"), q.z.ToString("F3"));
    }

    public override void OnSimulateBefore(State state) {
      if (state.Entity.IsDummy) {
        var f = state.Frames.first;

        switch (SmoothingSettings.Algorithm) {
          case SmoothingAlgorithms.Interpolation:
            f.Data.PackQuaternion(SettingsOld.ByteOffset, Bolt.Math.InterpolateQuaternion(state.Frames, SettingsOld.ByteOffset + 16, state.Entity.Frame));
            break;

          case SmoothingAlgorithms.Extrapolation:
            f.Data.PackQuaternion(SettingsOld.ByteOffset, Bolt.Math.ExtrapolateQuaternion(state.Frames, SettingsOld.ByteOffset + 16, state.Entity.Frame, SmoothingSettings, f.Data.ReadQuaternion(SettingsOld.ByteOffset)));
            break;
        }
      }
    }

    public override int StateBits(State state, State.NetworkFrame frame) {
      return QuaternionCompression.BitsRequired;
    }

    protected override bool Pack(byte[] data, BoltConnection connection, UdpPacket stream) {
      QuaternionCompression.Pack(stream, Blit.ReadQuaternion(data, SettingsOld.ByteOffset));
      return true;
    }

    protected override void Read(byte[] data, BoltConnection connection, UdpPacket stream) {
      Blit.PackQuaternion(data, SettingsOld.ByteOffset, QuaternionCompression.Read(stream));
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {
      var v0 = from.ReadQuaternion(SettingsOld.ByteOffset);
      var v1 = to.ReadQuaternion(SettingsOld.ByteOffset);
      into.PackQuaternion(SettingsOld.ByteOffset, UE.Quaternion.Lerp(v0, v1, t));
    }
  }
}

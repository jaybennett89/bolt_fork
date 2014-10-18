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
      var q = Blit.ReadQuaternion(state.Frames.first.Data, Settings.ByteOffset).eulerAngles;
      return string.Format("X:{0} Y:{1} Z:{2}", q.x.ToString("F3"), q.y.ToString("F3"), q.z.ToString("F3"));
    }

    public override void OnSimulateBefore(State state) {
      if (state.Entity.IsDummy) {
        var f = state.Frames.first;

        switch (SmoothingSettings.Algorithm) {
          case SmoothingAlgorithms.Interpolation:
            f.Data.PackQuaternion(Settings.ByteOffset, Bolt.Math.InterpolateQuaternion(state.Frames, Settings.ByteOffset + 16, state.Entity.Frame));
            break;

          case SmoothingAlgorithms.Extrapolation:
            f.Data.PackQuaternion(Settings.ByteOffset, Bolt.Math.ExtrapolateQuaternion(state.Frames, Settings.ByteOffset + 16, state.Entity.Frame, SmoothingSettings, f.Data.ReadQuaternion(Settings.ByteOffset)));
            break;
        }
      }
    }

    public override int StateBits(State state, State.Frame frame) {
      return 32 * 4;
    }

    protected override bool Pack(byte[] data, BoltConnection connection, UdpStream stream) {
      QuaternionCompression.Pack(stream, Blit.ReadQuaternion(data, Settings.ByteOffset));
      return true;
    }

    protected override void Read(byte[] data, BoltConnection connection, UdpStream stream) {
      Blit.PackQuaternion(data, Settings.ByteOffset, QuaternionCompression.Read(stream));
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {
      var v0 = from.ReadQuaternion(Settings.ByteOffset);
      var v1 = to.ReadQuaternion(Settings.ByteOffset);
      into.PackQuaternion(Settings.ByteOffset, UE.Quaternion.Lerp(v0, v1, t));
    }
  }
}

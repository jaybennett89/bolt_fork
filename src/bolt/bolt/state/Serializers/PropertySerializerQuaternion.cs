using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  class PropertySerializerQuaternion : PropertySerializerSimple {
    public override object GetDebugValue(State state) {
      var q = Blit.ReadQuaternion(state.Frames.first.Data, Settings.ByteOffset).eulerAngles;
      return string.Format("X:{0} Y:{1} Z:{2}", q.x.ToString("F3"), q.y.ToString("F3"), q.z.ToString("F3"));
    }

    public override int StateBits(State state, State.Frame frame) {
      return 32 * 4;
    }

    protected override bool Pack(byte[] data, BoltConnection connection, UdpStream stream) {
      stream.WriteQuaternion(Blit.ReadQuaternion(data, Settings.ByteOffset));
      return true;
    }

    protected override void Read(byte[] data, BoltConnection connection, UdpStream stream) {
      Blit.PackQuaternion(data, Settings.ByteOffset, stream.ReadQuaternion());
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {
      var v0 = from.ReadQuaternion(Settings.ByteOffset);
      var v1 = to.ReadQuaternion(Settings.ByteOffset);
      into.PackQuaternion(Settings.ByteOffset, UE.Quaternion.Lerp(v0, v1, t));
    }
  }
}

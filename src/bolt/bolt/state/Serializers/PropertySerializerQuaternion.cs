using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  class PropertySerializerQuaternion : PropertySerializerSimple {
    public PropertySerializerQuaternion(StatePropertyMetaData meta) : base(meta) { }
    public PropertySerializerQuaternion(EventPropertyMetaData meta) : base(meta) { }
    public PropertySerializerQuaternion(CommandPropertyMetaData meta) : base(meta) { }

    public override object GetDebugValue(State state) {
      var q =  Blit.ReadQuaternion(state.Frames.first.Data, StateData.ByteOffset).eulerAngles;
      return string.Format("X:{0} Y:{1} Z:{2}", q.x.ToString("F3"), q.y.ToString("F3"), q.z.ToString("F3"));
    }


    public override int StateBits(State state, State.Frame frame) {
      return 32 * 4;
    }

    protected override bool Pack(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      stream.WriteQuaternion(Blit.ReadQuaternion(data, offset));
      return true;
    }

    protected override void Read(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      Blit.PackQuaternion(data, offset, stream.ReadQuaternion());
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {
      var v0 = from.ReadQuaternion(CommandData.ByteOffset);
      var v1 = to.ReadQuaternion(CommandData.ByteOffset);
      into.PackQuaternion(CommandData.ByteOffset, UE.Quaternion.Lerp(v0, v1, t));
    }
  }
}

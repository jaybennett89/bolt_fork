using UdpKit;

namespace Bolt {
  class PropertySerializerNetworkId : PropertySerializerSimple {
    public override int StateBits(State state, State.Frame frame) {
      return 8 * 8;
    }

    public override object GetDebugValue(State state) {
      return Blit.ReadUniqueId(state.Frames.first.Data, Settings.ByteOffset);
    }

    protected override bool Pack(byte[] data, BoltConnection connection, UdpPacket stream) {
      stream.WriteNetworkId(Blit.ReadNetworkId(data, Settings.ByteOffset));
      return true;
    }

    protected override void Read(byte[] data, BoltConnection connection, UdpPacket stream) {
      Blit.PackNetworkId(data, Settings.ByteOffset, stream.ReadNetworkId());
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {

    }
  }
}

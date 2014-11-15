using UdpKit;

namespace Bolt {
  class PropertySerializerNetworkId : PropertySerializerSimple {
    public override int StateBits(State state, State.Frame frame) {
      return 16 * 8;
    }

    public override object GetDebugValue(State state) {
      return Blit.ReadUniqueId(state.Frames.first.Data, Settings.ByteOffset);
    }

    protected override bool Pack(byte[] data, BoltConnection connection, UdpPacket stream) {
      Bolt.UniqueId id = Blit.ReadUniqueId(data, Settings.ByteOffset);
      stream.WriteUInt(id.uint0);
      stream.WriteUInt(id.uint1);
      stream.WriteUInt(id.uint2);
      stream.WriteUInt(id.uint3);
      return true;
    }

    protected override void Read(byte[] data, BoltConnection connection, UdpPacket stream) {
      Bolt.UniqueId id = new UniqueId();
      id.uint0 = stream.ReadUInt();
      id.uint1 = stream.ReadUInt();
      id.uint2 = stream.ReadUInt();
      id.uint3 = stream.ReadUInt();
      Blit.PackUniqueId(data, Settings.ByteOffset, id);
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {

    }
  }
}

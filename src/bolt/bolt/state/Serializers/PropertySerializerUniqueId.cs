using UdpKit;

namespace Bolt {
  class PropertySerializerUniqueId : PropertySerializerSimple {
    public PropertySerializerUniqueId(StatePropertyMetaData info) : base(info) { }
    public PropertySerializerUniqueId(EventPropertyMetaData meta) : base(meta) { }
    public PropertySerializerUniqueId(CommandPropertyMetaData meta) : base(meta) { }

    public override int StateBits(State state, State.Frame frame) {
      return 16 * 8;
    }

    public override object GetDebugValue(State state) {
      return Blit.ReadUniqueId(state.Frames.first.Data, StateData.ByteOffset);
    }

    protected override bool Pack(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      Bolt.UniqueId id = Blit.ReadUniqueId(data, offset);
      stream.WriteUInt(id.uint0);
      stream.WriteUInt(id.uint1);
      stream.WriteUInt(id.uint2);
      stream.WriteUInt(id.uint3);

      return true;
    }

    protected override void Read(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      Bolt.UniqueId id = new UniqueId();
      id.uint0 = stream.ReadUInt();
      id.uint1 = stream.ReadUInt();
      id.uint2 = stream.ReadUInt();
      id.uint3 = stream.ReadUInt();

      Blit.PackUniqueId(data, offset, id);
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {

    }
  }
}

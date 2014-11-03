using UdpKit;

namespace Bolt {
  class PropertySerializerPrefabId : PropertySerializerSimple {
    public override int StateBits(State state, State.Frame frame) {
      return 32;
    }

    public override object GetDebugValue(State state) {
      return Blit.ReadPrefabId(state.Frames.first.Data, Settings.ByteOffset);
    }

    protected override bool Pack(byte[] data,  BoltConnection connection, UdpPacket stream) {
      stream.WriteInt(Blit.ReadPrefabId(data, Settings.ByteOffset).Value);
      return true;
    }

    protected override void Read(byte[] data, BoltConnection connection, UdpPacket stream) {
      Blit.PackPrefabId(data, Settings.ByteOffset, new Bolt.PrefabId(stream.ReadInt()));
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {

    }
  }
}

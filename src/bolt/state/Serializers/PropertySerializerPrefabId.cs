using UdpKit;

namespace Bolt {
  class PropertySerializerPrefabId : PropertySerializerSimple {
    public override int StateBits(State state, NetworkFrame frame) {
      return 32;
    }

    public override object GetDebugValue(State state) {
      return state.CurrentFrame.Storage[Settings.OffsetStorage].PrefabId;
    }

    protected override bool Pack(NetworkValue[] storage,  BoltConnection connection, UdpPacket stream) {
      stream.WritePrefabId(storage[Settings.OffsetStorage].PrefabId);
      return true;
    }

    protected override void Read(NetworkValue[] storage, BoltConnection connection, UdpPacket stream) {
      storage[Settings.OffsetStorage].PrefabId = stream.ReadPrefabId();
    }

    public override void CommandSmooth(NetworkValue[] from, NetworkValue[] to, NetworkValue[] into, float t) {

    }
  }
}

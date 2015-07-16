using UdpKit;

namespace Bolt {
  class PropertySerializerNetworkId : PropertySerializerSimple {
    public override int StateBits(State state, NetworkFrame frame) {
      return 8 * 8;
    }

    public override object GetDebugValue(State state) {
      return state.CurrentFrame.Storage[Settings.OffsetStorage].NetworkId;
    }

    protected override bool Pack(NetworkValue[] storage, BoltConnection connection, UdpPacket stream) {
      stream.WriteNetworkId(storage[Settings.OffsetStorage].NetworkId);
      return true;
    }

    protected override void Read(NetworkValue[] storage, BoltConnection connection, UdpPacket stream) {
      storage[Settings.OffsetStorage].NetworkId = stream.ReadNetworkId();
    }

    public override void CommandSmooth(NetworkValue[] from, NetworkValue[] to, NetworkValue[] into, float t) {

    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  class PropertySerializerEntity : PropertySerializerSimple {
    public override object GetDebugValue(State state) {
      Bolt.Entity entity = BoltCore.FindEntity(state.CurrentFrame.Storage[Settings.OffsetStorage].NetworkId);

      if (entity) {
        return entity.ToString();
      }

      return "NULL";
    }

    public override int StateBits(State state, NetworkFrame frame) {
      return 8 * 8;
    }

    protected override bool Pack(NetworkValue[] storage,  BoltConnection connection, UdpPacket stream) {
      stream.WriteNetworkId(storage[Settings.OffsetStorage].NetworkId);
      return true;
    }

    protected override void Read(NetworkValue[] storage, BoltConnection connection, UdpPacket stream) {
      storage[Settings.OffsetStorage].NetworkId = stream.ReadNetworkId();
    }
  }
}

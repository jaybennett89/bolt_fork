using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  class PropertySerializerEntity : PropertySerializerSimple {
    public override object GetDebugValue(State state) {
      Bolt.Entity entity = BoltCore.FindEntity(state.Frames.first.Data.ReadNetworkId(Settings.ByteOffset));

      if (entity) {
        return entity.ToString();
      }

      return "NULL";
    }

    public override int StateBits(State state, State.Frame frame) {
      return 8 * 8;
    }

    protected override bool Pack(byte[] data,  BoltConnection connection, UdpPacket stream) {
      stream.WriteNetworkId(data.ReadNetworkId(Settings.ByteOffset));
      return true;
    }

    protected override void Read(byte[] data, BoltConnection connection, UdpPacket stream) {
      data.PackNetworkId(Settings.ByteOffset, stream.ReadNetworkId());
    }
  }
}

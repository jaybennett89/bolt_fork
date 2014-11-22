using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  internal class NetworkProperty_Entity : NetworkProperty {
    public override int BitCount(NetworkObj obj) {
      return 64;
    }

    public override object DebugValue(NetworkObj obj, NetworkStorage storage) {
      Bolt.Entity entity = BoltCore.FindEntity(storage.Values[obj[this]].NetworkId);

      if (entity) {
        return entity.ToString();
      }

      return "NULL";
    }

    public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      packet.WriteNetworkId(storage.Values[obj[this]].NetworkId);
      return true;
    }

    public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      storage.Values[obj[this]].NetworkId = packet.ReadNetworkId();
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  internal class NetworkProperty_PrefabId : NetworkProperty {
    public override object DebugValue(NetworkObj obj, NetworkStorage storage) {
      return storage.Values[obj[this]].PrefabId;
    }

    public override int BitCount(NetworkObj obj) {
      return 32;
    }

    public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      packet.WritePrefabId(storage.Values[obj[this]].PrefabId);
      return true;
    }

    public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      storage.Values[obj[this]].PrefabId = packet.ReadPrefabId();
    }
  }
}

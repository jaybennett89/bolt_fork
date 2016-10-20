using System;
using UdpKit;

namespace Bolt {
  internal class NetworkProperty_Guid : NetworkProperty {
    public override object DebugValue(NetworkObj obj, NetworkStorage storage) {
      return storage.Values[obj[this]].Guid;
    }

    public override void SetDynamic(NetworkObj obj, object value) {
      var v = (Guid)value;

      if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Guid, v)) {
        obj.Storage.Values[obj[this]].Guid = v;
        obj.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);
      }
    }

    public override object GetDynamic(NetworkObj obj) {
      return obj.Storage.Values[obj[this]].Guid;
    }

    public override int BitCount(NetworkObj obj) {
      return 128;
    }

    public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      packet.WriteGuid(storage.Values[obj[this]].Guid);
      return true;
    }

    public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      storage.Values[obj[this]].Guid = packet.ReadGuid();
    }
  }
}

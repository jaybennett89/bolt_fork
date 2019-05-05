using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  class NetworkProperty_ProtocolToken : NetworkProperty {
    public override void SetDynamic(NetworkObj obj, object value) {
      var v = (IProtocolToken)value;

      if (NetworkValue.Diff(obj.Storage.Values[obj[this]].ProtocolToken, v)) {
        obj.Storage.Values[obj[this]].ProtocolToken = v;
        obj.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);
      }
    }

    public override object GetDynamic(NetworkObj obj) {
      return obj.Storage.Values[obj[this]].ProtocolToken;
    }

    public override object DebugValue(NetworkObj obj, NetworkStorage storage) {
      if (storage.Values[obj[this]].ProtocolToken == null) {
        return "NULL";
      }

      return storage.Values[obj[this]].ProtocolToken.ToString();
    }

    public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpKit.UdpPacket packet) {
      try {
        packet.WriteToken(storage.Values[obj[this]].ProtocolToken);
        return true;
      }
      catch (Exception exn) {
        BoltLog.Error("User code threw exception while serializing protocol token");
        BoltLog.Exception(exn);
        return false;
      }
    }

    public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpKit.UdpPacket packet) {
      storage.Values[obj[this]].ProtocolToken = packet.ReadToken();
    }
  }
}

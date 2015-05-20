using System;
using UdpKit;
using UnityEngine;

namespace Bolt {
  internal class NetworkProperty_Color32 : NetworkProperty {
    public override object DebugValue(NetworkObj obj, NetworkStorage storage) {
      return storage.Values[obj[this]].Color32;
    }

    public override void SetDynamic(NetworkObj obj, object value) {
      var v = (Color32)value;

      if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Color32, v)) {
        obj.Storage.Values[obj[this]].Color32 = v;
        obj.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);
      }
    }

    public override object GetDynamic(NetworkObj obj) {
      return obj.Storage.Values[obj[this]].Color32;
    }

    public override int BitCount(NetworkObj obj) {
      return 128;
    }

    public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      packet.WriteColor32RGBA(storage.Values[obj[this]].Color32);
      return true;
    }

    public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      storage.Values[obj[this]].Color32 = packet.ReadColor32RGBA();
    }
  }
}

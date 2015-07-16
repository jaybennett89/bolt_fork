using System;
using UdpKit;
using UnityEngine;

namespace Bolt {
  internal class NetworkProperty_Matrix4x4 : NetworkProperty {
    public override object DebugValue(NetworkObj obj, NetworkStorage storage) {
      return storage.Values[obj[this]].Matrix4x4;
    }

    public override void SetDynamic(NetworkObj obj, object value) {
      var v = (Matrix4x4)value;

      if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Matrix4x4, v)) {
        obj.Storage.Values[obj[this]].Matrix4x4 = v;
        obj.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);
      }
    }

    public override object GetDynamic(NetworkObj obj) {
      return obj.Storage.Values[obj[this]].Matrix4x4;
    }

    public override int BitCount(NetworkObj obj) {
      return 512;
    }

    public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      packet.WriteMatrix4x4(storage.Values[obj[this]].Matrix4x4);
      return true;
    }

    public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      storage.Values[obj[this]].Matrix4x4 = packet.ReadMatrix4x4();
    }
  }
}

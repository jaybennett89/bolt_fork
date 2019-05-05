using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  internal class NetworkProperty_String : NetworkProperty {
    PropertyStringSettings StringSettings;

    public void AddStringSettings(StringEncodings encoding) {
      StringSettings.Encoding = encoding;
    }

    public override void SetDynamic(NetworkObj obj, object value) {
      var v = (string)value;

      if (NetworkValue.Diff(obj.Storage.Values[obj[this]].String, v)) {
        obj.Storage.Values[obj[this]].String = v;
        obj.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);
      }
    }

    public override object GetDynamic(NetworkObj obj) {
      return obj.Storage.Values[obj[this]].String;
    }

    public override int BitCount(NetworkObj obj) {
      if (obj.Storage.Values[obj[this]].String == null) {
        return 16;
      }

      return 16 + StringSettings.EncodingClass.GetByteCount(obj.Storage.Values[obj[this]].String);
    }

    public override object DebugValue(NetworkObj obj, NetworkStorage storage) {
      return storage.Values[obj[this]].String;
    }

    public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      packet.WriteString(storage.Values[obj[this]].String, StringSettings.EncodingClass);
      return true;
    }

    public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      storage.Values[obj[this]].String = packet.ReadString(StringSettings.EncodingClass);
    }
  }
}

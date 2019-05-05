using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal struct PropertyStringSettings {
    public Bolt.StringEncodings Encoding;
    public Encoding EncodingClass {
      get {
        switch (Encoding) {
          case StringEncodings.ASCII: return System.Text.Encoding.ASCII;
          case StringEncodings.UTF8: return System.Text.Encoding.UTF8;
        }

        throw new NotImplementedException();
      }
    }
  }

  class PropertySerializerString : PropertySerializerSimple {
    PropertyStringSettings StringSettings;

    public void AddStringSettings(StringEncodings encoding) {
      StringSettings.Encoding = encoding;
    }

    public override object GetDebugValue(State state) {
      return state.CurrentFrame.Storage[Settings.OffsetStorage].String;
    }

    public override int StateBits(State state, NetworkFrame frame) {
      if (state.CurrentFrame.Storage[Settings.OffsetStorage].String == null) {
        return 16;
      }

      return 16 + StringSettings.EncodingClass.GetByteCount(state.CurrentFrame.Storage[Settings.OffsetStorage].String);
    }

    protected override bool Pack(NetworkValue[] data, BoltConnection connection, UdpKit.UdpPacket stream) {
      stream.WriteString(data[Settings.OffsetStorage].String, StringSettings.EncodingClass);
      return true;
    }

    protected override void Read(NetworkValue[] data, BoltConnection connection, UdpKit.UdpPacket stream) {
      data[Settings.OffsetStorage].String = stream.ReadString(StringSettings.EncodingClass);
    }
  }
}

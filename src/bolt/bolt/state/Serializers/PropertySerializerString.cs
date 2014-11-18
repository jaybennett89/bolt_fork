using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal struct PropertyStringSettings {
    public Bolt.StringEncodings Encoding;
  }

  class PropertySerializerString : PropertySerializerSimple {
    PropertyStringSettings StringSettings;

    public void AddSettings(PropertyStringSettings stringSettings) {
      StringSettings = stringSettings;
    }

    public override object GetDebugValue(State state) {
      var frame = state.Frames.first;
      int length = Blit.ReadI32(frame.Data, SettingsOld.ByteOffset);

      if (StringSettings.Encoding == StringEncodings.ASCII) {
        return Encoding.ASCII.GetString(frame.Data, SettingsOld.ByteOffset + 4, length);
      }
      else {
        return Encoding.UTF8.GetString(frame.Data, SettingsOld.ByteOffset + 4, length);
      }
    }

    public override int StateBits(State state, State.NetworkFrame frame) {
      return 32 + (Blit.ReadI32(frame.Data, SettingsOld.ByteOffset) * 8);
    }

    protected override bool Pack(byte[] data, BoltConnection connection, UdpKit.UdpPacket stream) {
      stream.WriteInt(Blit.ReadI32(data, SettingsOld.ByteOffset));
      stream.WriteByteArray(data, SettingsOld.ByteOffset + 4, Blit.ReadI32(data, SettingsOld.ByteOffset));
      return true;
    }

    protected override void Read(byte[] data,  BoltConnection connection, UdpKit.UdpPacket stream) {
      Blit.PackI32(data, SettingsOld.ByteOffset, stream.ReadInt());
      Blit.PackBytes(data, SettingsOld.ByteOffset + 4, stream.ReadByteArray(Blit.ReadI32(data, SettingsOld.ByteOffset)));
    }
  }
}

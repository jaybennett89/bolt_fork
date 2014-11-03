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
      int length = Blit.ReadI32(frame.Data, Settings.ByteOffset);

      if (StringSettings.Encoding == StringEncodings.ASCII) {
        return Encoding.ASCII.GetString(frame.Data, Settings.ByteOffset + 4, length);
      }
      else {
        return Encoding.UTF8.GetString(frame.Data, Settings.ByteOffset + 4, length);
      }
    }

    public override int StateBits(State state, State.Frame frame) {
      return 32 + (Blit.ReadI32(frame.Data, Settings.ByteOffset) * 8);
    }

    protected override bool Pack(byte[] data, BoltConnection connection, UdpKit.UdpPacket stream) {
      stream.WriteInt(Blit.ReadI32(data, Settings.ByteOffset));
      stream.WriteByteArray(data, Settings.ByteOffset + 4, Blit.ReadI32(data, Settings.ByteOffset));
      return true;
    }

    protected override void Read(byte[] data,  BoltConnection connection, UdpKit.UdpPacket stream) {
      Blit.PackI32(data, Settings.ByteOffset, stream.ReadInt());
      Blit.PackBytes(data, Settings.ByteOffset + 4, stream.ReadByteArray(Blit.ReadI32(data, Settings.ByteOffset)));
    }
  }
}

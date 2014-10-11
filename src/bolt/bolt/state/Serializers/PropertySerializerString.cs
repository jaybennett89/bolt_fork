using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  struct PropertySerializerStringData {
    public Bolt.StringEncodings Encoding;
  }

  class PropertySerializerString : PropertySerializerSimple {
    PropertySerializerStringData StringSettings;

    public PropertySerializerString(StatePropertyMetaData meta) : base(meta) { }
    public PropertySerializerString(EventPropertyMetaData meta) : base(meta) { }

    public void SetPropertyData(PropertySerializerStringData data) {
      StringSettings = data;
    }

    public override object GetDebugValue(State state) {
      var frame = state.Frames.first;
      int length = Blit.ReadI32(frame.Data, StateData.ByteOffset);

      if (StringSettings.Encoding == StringEncodings.ASCII) {
        return Encoding.ASCII.GetString(frame.Data, StateData.ByteOffset + 4, length);
      }
      else {
        return Encoding.UTF8.GetString(frame.Data, StateData.ByteOffset + 4, length);
      }
    }

    public override int StateBits(State state, State.Frame frame) {
      return 32 + (Blit.ReadI32(frame.Data, StateData.ByteOffset) * 8);
    }

    protected override bool Pack(byte[] data, int offset, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteInt(Blit.ReadI32(data, offset));
      stream.WriteByteArray(data, offset + 4, Blit.ReadI32(data, offset));
      return true;
    }

    protected override void Read(byte[] data, int offset, BoltConnection connection, UdpKit.UdpStream stream) {
      Blit.PackI32(data, offset, stream.ReadInt());
      Blit.PackBytes(data, offset + 4, stream.ReadByteArray(Blit.ReadI32(data, offset)));
    }
  }
}

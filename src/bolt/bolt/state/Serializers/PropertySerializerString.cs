using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  class PropertySerializerString : PropertySerializerSimple {
    public PropertySerializerString(StatePropertyMetaData info) : base(info) { }
    public PropertySerializerString(EventPropertyMetaData meta) : base(meta) { }

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

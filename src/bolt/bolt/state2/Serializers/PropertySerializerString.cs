using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  class PropertySerializerString : PropertySerializer {
    public override int CalculateBits(byte[] data) {
      return 32 + (Blit.ReadI32(data, Data.ByteOffset) * 8);
    }

    public override void Pack(State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteInt(Blit.ReadI32(frame.Data, Data.ByteOffset));
      stream.WriteByteArray(frame.Data, Data.ByteOffset + 4, Blit.ReadI32(frame.Data, Data.ByteOffset));
    }

    public override void Read(State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      Blit.PackI32(frame.Data, Data.ByteOffset, stream.ReadInt());
      Blit.PackBytes(frame.Data, Data.ByteOffset + 4, stream.ReadByteArray(Blit.ReadI32(frame.Data, Data.ByteOffset)));
    }

    public PropertySerializerString(PropertyMetaData info)
      : base(info) {
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  class PropertySerializerString : PropertySerializer {
    public override int CalculateBits(byte[] data) {
      return 32 + (Blit.ReadI32(data, MetaData.ByteOffset) * 8);
    }

    public override void Pack(State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteInt(Blit.ReadI32(frame.Data, MetaData.ByteOffset));
      stream.WriteByteArray(frame.Data, MetaData.ByteOffset + 4, Blit.ReadI32(frame.Data, MetaData.ByteOffset));
    }

    public override void Read(State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      Blit.PackI32(frame.Data, MetaData.ByteOffset, stream.ReadInt());
      Blit.PackBytes(frame.Data, MetaData.ByteOffset + 4, stream.ReadByteArray(Blit.ReadI32(frame.Data, MetaData.ByteOffset)));
    }

    public PropertySerializerString(PropertyMetaData info)
      : base(info) {
    }
  }
}

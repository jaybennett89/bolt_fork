using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  public class PropertySerializerString : PropertySerializer {
    public override int CalculateBits(byte[] data) {
      return 32 + (Blit.ReadI32(data, Offset) * 8);
    }

    public override void Pack(int frame, UdpKit.UdpConnection connection, UdpKit.UdpStream stream, byte[] data) {
      stream.WriteInt(Blit.ReadI32(data, Offset));
      stream.WriteByteArray(data, Offset + 4, Blit.ReadI32(data, Offset));
    }

    public override void Read(int frame, UdpKit.UdpConnection connection, UdpKit.UdpStream stream, byte[] data) {
      Blit.PackI32(data, Offset, stream.ReadInt());
      Blit.PackBytes(data, Offset + 4, stream.ReadByteArray(Blit.ReadI32(data, Offset)));
    }

    public PropertySerializerString(int offset, int length, int priority)
      : base(offset, length, priority) {
    }
  }
}

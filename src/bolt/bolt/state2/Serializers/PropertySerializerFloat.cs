using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  public class PropertySerializerFloat : PropertySerializer {
    public override int CalculateBits(byte[] data) {
      return 32;
    }

    public override void Pack(int frame, UdpKit.UdpConnection connection, UdpKit.UdpStream stream, byte[] data) {
      stream.WriteFloat(Blit.ReadF32(data, Offset));
    }

    public override void Read(int frame, UdpKit.UdpConnection connection, UdpKit.UdpStream stream, byte[] data) {
      Blit.PackF32(data, Offset, stream.ReadFloat());
    }

    public PropertySerializerFloat(int offset, int length, int priority)
      : base(offset, length, priority) {
    }
  }
}

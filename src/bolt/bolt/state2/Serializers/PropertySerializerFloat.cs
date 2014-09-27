using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  class PropertySerializerFloat : PropertySerializer {
    public override int CalculateBits(byte[] data) {
      return 32;
    }

    public override void Pack(State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteFloat(Blit.ReadF32(frame.Data, ByteOffset));
    }

    public override void Read(State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      Blit.PackF32(frame.Data, ByteOffset, stream.ReadFloat());
    }

    public PropertySerializerFloat(int byteOffset, int byteLength, int objectOffset, int priority)
      : base(byteOffset, byteLength, objectOffset, priority) {
    }
  }
}

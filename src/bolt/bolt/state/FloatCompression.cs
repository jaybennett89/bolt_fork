using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  internal struct PropertyFloatCompressionSettings {
    public int Bits;
    public float Shift;
    public float PackMultiplier;
    public float ReadMultiplier;

    public void Pack(UdpStream stream, float value) {
      if (Bits == 32) {
        stream.WriteFloat(value);
      }
      else {
        stream.WriteInt((int)((value + Shift) * PackMultiplier), Bits);
      }
    }

    public float Read(UdpStream stream) {
      if (Bits == 32) {
        return stream.ReadFloat();
      }
      else {
        return (stream.ReadInt(Bits) * ReadMultiplier) + -Shift;
      }
    }
  }
}

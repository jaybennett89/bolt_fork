using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  struct FloatCompression {
    public int Bits;
    public int Adjust;
    public int Fractions;

    public void Pack(UdpStream stream, float value) {
      stream.WriteInt(((int)(value * Fractions)) + Adjust, Bits);
    }

    public float Read(UdpStream stream) {
      return (stream.ReadInt(Bits) + -Adjust) / (float)Fractions;
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  internal struct PropertyIntCompressionSettings {
    int _bits;
    int _shift;

    public int BitsRequired {
      get { return _bits; }
    }

    public static PropertyIntCompressionSettings Create() {
      PropertyIntCompressionSettings f;

      f = new PropertyIntCompressionSettings();
      f._bits = 32;

      return f;
    }

    public static PropertyIntCompressionSettings Create(int bits, int shift) {
      PropertyIntCompressionSettings f;

      f = new PropertyIntCompressionSettings();
      f._bits = 32;
      f._shift = shift;

      return f;
    }

    public void Pack(UdpPacket stream, int value) {
      stream.WriteInt(value + _shift, _bits);
    }

    public int Read(UdpPacket stream) {
      return stream.ReadInt(_bits) + _shift;
    }
  }

}

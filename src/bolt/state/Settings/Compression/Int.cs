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
      stream.WriteInt_Shifted(value, _bits, _shift);
    }

    public int Read(UdpPacket stream) {
      return stream.ReadInt_Shifted(_bits, _shift); //+-_shift;
    }
  }

}

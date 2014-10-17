using UdpKit;

namespace Bolt {
  internal struct PropertyFloatCompressionSettings {
    int _bits;

    float _shift;
    float _pack;
    float _read;

    public int BitsRequired {
      get { return _bits; }
    }

    public static PropertyFloatCompressionSettings CreateUncompressed() {
      PropertyFloatCompressionSettings f;
      
      f = new PropertyFloatCompressionSettings();
      f._bits = 32;

      return f;
    }

    public PropertyFloatCompressionSettings(int bits, float shift, float pack, float read) {
      _bits = bits;
      _shift = shift;
      _pack = pack;
      _read = read;
    }

    public void Pack(UdpStream stream, float value) {
      if (_bits == 32) {
        stream.WriteFloat(value);
      }
      else {
        stream.WriteInt((int)((value + _shift) * _pack), _bits);
      }
    }

    public float Read(UdpStream stream) {
      if (_bits == 32) {
        return stream.ReadFloat();
      }
      else {
        return (stream.ReadInt(_bits) * _read) + -_shift;
      }
    }
  }
}

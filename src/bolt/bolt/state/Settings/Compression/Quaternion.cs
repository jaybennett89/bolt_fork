using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  struct PropertyQuaternionCompression {
    bool QuaternionMode;

    public PropertyVectorCompressionSettings Euler;
    public PropertyFloatCompressionSettings Quaternion;

    public int BitsRequired {
      get {
        if (QuaternionMode) {
          return Quaternion.BitsRequired * 4;
        }

        return Euler.BitsRequired;
      }
    }

    public static PropertyQuaternionCompression Create(PropertyVectorCompressionSettings euler) {
      return new PropertyQuaternionCompression {
        Euler = euler,
        QuaternionMode = false
      };
    }

    public static PropertyQuaternionCompression Create(PropertyFloatCompressionSettings quaternion) {
      return new PropertyQuaternionCompression {
        Quaternion = quaternion,
        QuaternionMode = true
      };
    }

    public void Pack(UdpPacket stream, UE.Quaternion value) {
      if (QuaternionMode) {
        Quaternion.Pack(stream, value.x);
        Quaternion.Pack(stream, value.y);
        Quaternion.Pack(stream, value.z);
        Quaternion.Pack(stream, value.w);
      }
      else {
        Euler.Pack(stream, value.eulerAngles);
      }
    }

    public UE.Quaternion Read(UdpPacket stream) {
      UE.Quaternion q;

      if (QuaternionMode) {
        q.x = Quaternion.Read(stream);
        q.y = Quaternion.Read(stream);
        q.z = Quaternion.Read(stream);
        q.w = Quaternion.Read(stream);
      }
      else {
        q = UE.Quaternion.Euler(Euler.Read(stream));
      }

      return q;
    }
  }
}

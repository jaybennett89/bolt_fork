using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class FloatCompression {
    [ProtoMember(1)]
    public int MinValue;

    [ProtoMember(2)]
    public int MaxValue;

    [ProtoMember(5)]
    public float Accuracy;

    [ProtoMember(6)]
    public bool Enabled;

    public int BitsRequired {
      get {
        var pack = 1f / Accuracy;
        var shift = -MinValue;
        return BitForNumber((int)Math.Round((MaxValue + shift) * pack));
      }
    }

    public static FloatCompression Default() {
      return new FloatCompression {
        MinValue = -2048,
        MaxValue = +2048,
        Accuracy = 0.01f,
      };
    }

    public static FloatCompression DefaultAngle() {
      return new FloatCompression {
        MinValue = 0,
        MaxValue = 360,
        Accuracy = 0.1f,
      };
    }

    static int BitForNumber(int number) {
      if (number < 0) { return 32; }
      if (number == 0) { return 1; }

      for (int i = 31; i >= 0; --i) {
        int b = 1 << i;

        if ((number & b) == b) {
          return i + 1;
        }
      }

      throw new Exception();
    }
  }
}

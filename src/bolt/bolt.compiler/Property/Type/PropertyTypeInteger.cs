 using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract()]
  public class PropertyTypeInteger : PropertyType {
    [ProtoMember(2)]
    public int MinValue;

    [ProtoMember(3)]
    public int MaxValue;

    [ProtoMember(4)]
    public bool CompressionEnabled;

    static int CalculateBits(int number) {
      if (number < 0) {
        return 32;
      }

      if (number == 0) {
        return 1;
      }

      for (int i = 31; i >= 0; --i) {
        int b = 1 << i;

        if ((number & b) == b) {
          return i + 1;
        }
      }

      throw new Exception();
    }

    public int BitsRequired {
      get {
        if (CompressionEnabled) {
          return CalculateBits(MaxValue - MinValue);
        }

        return 32;
      }
    }

    public override bool HasSettings {
      get { return true; }
    }

    public override bool MecanimApplicable {
      get { return true; }
    }

    public override void OnCreated() {
      MinValue = 0;
      MaxValue = 255;
      CompressionEnabled = false;
    }

    public override PropertyDecorator CreateDecorator() {
      return new PropertyDecoratorInteger();
    }
  }
}

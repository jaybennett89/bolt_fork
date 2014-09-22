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

    [ProtoMember(3)]
    public int Bits;

    [ProtoMember(4)]
    public int Fractions;

    public static FloatCompression Default() {
      return new FloatCompression {
        Bits = 32,
        MinValue = -2048,
        MaxValue = +2048,
        Fractions = 1000
      };
    }
  }
}

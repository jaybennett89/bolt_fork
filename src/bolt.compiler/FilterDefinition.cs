using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class FilterDefinition {
    [ProtoMember(1)]
    public int Index;

    [ProtoMember(5)]
    public bool Enabled;

    [ProtoMember(3)]
    public string Name;

    [ProtoMember(4)]
    public Color4 Color;

    public int Bit {
      get { return 1 << Index; }
    }

    public bool IsOn(int bits) {
      return (bits & Bit) == Bit;
    }
  }
}

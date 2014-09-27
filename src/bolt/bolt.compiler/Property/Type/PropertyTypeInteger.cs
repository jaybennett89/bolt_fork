using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public enum IntegerMode {
    Signed,
    Unsigned
  }

  [ProtoContract]
  public class PropertyTypeInteger : PropertyType {
    [ProtoMember(1)]
    public IntegerMode Mode = IntegerMode.Signed;

    [ProtoMember(2)]
    public int MinValue;

    [ProtoMember(3)]
    public int MaxValue;
  }
}

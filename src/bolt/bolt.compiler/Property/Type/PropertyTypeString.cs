using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyTypeString : PropertyType {
    [ProtoMember(50)]
    public int MaxLength = 1;

    [ProtoMember(51)]
    public StringEncodings Encoding;

    public Encoding EncodingClass {
      get {
        switch (Encoding) {
          case StringEncodings.ASCII: return System.Text.Encoding.ASCII;
          case StringEncodings.UTF8: return System.Text.Encoding.UTF8;
        }

        throw new NotSupportedException(Encoding.ToString());
      }
    }

    public override PropertyDecorator CreateDecorator() {
      return new PropertyDecoratorString();
    }
  }
}

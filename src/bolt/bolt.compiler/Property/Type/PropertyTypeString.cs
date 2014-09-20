using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public enum StringEncodings {
    ASCII = 0,
    UTF8 = 1
  }

  [ProtoContract]
  public class PropertyTypeString : PropertyType {
    [ProtoMember(1)]
    public int MaxLength;

    [ProtoMember(1)]
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

    public override PropertyCodeEmitter CreateCodeEmitter() {
      return new PropertyCodeEmitterString();
    }

    public override int ByteSize {
      get { return 2 + EncodingClass.GetMaxByteCount(MaxLength); }
    }

    public override string UserType {
      get { return typeof(string).FullName; }
    }
  }
}

using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bolt.compiler {
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

    public override int ByteSize {
      get { return 2 + EncodingClass.GetMaxByteCount(MaxLength); }
    }
  }
}

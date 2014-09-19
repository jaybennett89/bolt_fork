using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace bolt.compiler {
  [ProtoContract]
  public class PropertyTypeFloat : PropertyType {
    [ProtoMember(1)]
    public FloatCompression Compression;

    public override int ByteSize {
      get { return 4; }
    }

    public override bool MecanimUsable {
      get { return true; }
    }
  }
}

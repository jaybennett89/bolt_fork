using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyTypeFloat : PropertyType {
    [ProtoMember(1)]
    public FloatCompression Compression;

    public override bool MecanimUsable {
      get { return true; }
    }

    public override PropertyDecorator CreateDecorator() {
      return new PropertyDecoratorFloat();
    }
  }
}

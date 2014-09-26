using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyTypeTransform : PropertyType {
    [ProtoMember(1)]
    public TransformSpaces Space;

    public override bool InterpolateAllowed {
      get { return true; }
    }

    public override bool CallbackAllowed {
      get { return false; }
    }
  }
}

using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyTypeBool : PropertyType {
    public override bool HasSettings {
      get { return false; }
    }

    public override PropertyDecorator CreateDecorator() {
      return new PropertyDecoratorBool();
    }
  }
}

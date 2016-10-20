using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyTypeEntity : PropertyType {
    [ProtoMember(1)]
    public bool IsParent;

    public override bool HasSettings {
      get { return true; }
    }

    public override PropertyDecorator CreateDecorator() {
      return new PropertyDecoratorEntity();
    }
  }
}

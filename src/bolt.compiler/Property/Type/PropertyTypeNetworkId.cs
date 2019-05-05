using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract()]
  public class PropertyTypeNetworkId : PropertyType {
    public override bool HasSettings {
      get { return false; }
    }

    public override bool MecanimApplicable {
      get { return false; }
    }

    public override PropertyDecorator CreateDecorator() {
      return new PropertyDecoratorNetworkId();
    }
  }
}

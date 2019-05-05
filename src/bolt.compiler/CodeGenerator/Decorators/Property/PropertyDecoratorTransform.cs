using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyDecoratorTransform : PropertyDecorator<PropertyTypeTransform> {
    public override int RequiredStorage {
      get { return 3; }
    }

    public override int RequiredObjects {
      get { return 0; }
    }

    public override string ClrType {
      get { return "Bolt.NetworkTransform"; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterTransform();
    }
  }
}

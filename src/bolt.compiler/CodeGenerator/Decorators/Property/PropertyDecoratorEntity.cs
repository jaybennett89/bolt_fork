using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyDecoratorEntity : PropertyDecorator<PropertyTypeEntity> {
    public override string ClrType {
      get { return "BoltEntity";  }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterEntity();
    }
  }
}

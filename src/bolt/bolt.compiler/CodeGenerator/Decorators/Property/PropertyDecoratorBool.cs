using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyDecoratorBool : PropertyDecorator<PropertyTypeBool> {
    public override string ClrType {
      get { return typeof(bool).FullName; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterBool();
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyDecoratorInteger : PropertyDecorator<PropertyTypeInteger> {
    public override string ClrType {
      get { return typeof(int).FullName; }
    }

    public override int ByteSize {
      get { return 4; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterInteger();
    }
  }
}

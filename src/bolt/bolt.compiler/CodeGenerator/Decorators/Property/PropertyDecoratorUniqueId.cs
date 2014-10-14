using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyDecoratorUniqueId : PropertyDecorator<PropertyTypeUniqueId> {
    public override string ClrType {
      get { return "Bolt.UniqueId"; }
    }

    public override int ByteSize {
      get { return 16; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterUniqueId();
    }
  }
}

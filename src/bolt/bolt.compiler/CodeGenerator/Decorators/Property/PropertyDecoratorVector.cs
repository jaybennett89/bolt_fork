using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyDecoratorVector : PropertyDecorator<PropertyTypeVector> {
    public override string ClrType {
      get {
        return "UE.Vector3";
      }
    }

    public override int ByteSize {
      get { return 16; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterVector();
    }
  }
}

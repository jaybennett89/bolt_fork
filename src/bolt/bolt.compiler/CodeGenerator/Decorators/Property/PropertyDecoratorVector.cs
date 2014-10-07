using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyDecoratorVector : PropertyDecorator<PropertyTypeVector> {
    public override string ClrType {
      get {
        if (PropertyType[VectorComponents.W].Enabled) {
          return "UE.Vector4";
        }

        if (PropertyType[VectorComponents.Z].Enabled) {
          return "UE.Vector3";
        }

        return "UE.Vector2";
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

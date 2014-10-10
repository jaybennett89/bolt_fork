using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyDecoratorQuaternion : PropertyDecorator<PropertyTypeQuaternion> {
    public override string ClrType {
      get { return "UE.Quaternion"; }
    }

    public override int ByteSize {
      get { return 16; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterQuaternion();
    }
  }
}

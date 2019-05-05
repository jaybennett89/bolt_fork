using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyDecoratorMatrix4x4: PropertyDecorator<PropertyTypeGuid> {
    public override string ClrType {
      get { return "UE.Matrix4x4"; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterMatrix4x4();
    }
  }
}

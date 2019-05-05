using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyDecoratorGuid : PropertyDecorator<PropertyTypeGuid> {
    public override string ClrType {
      get { return typeof(Guid).FullName; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterGuid();
    }
  }
}

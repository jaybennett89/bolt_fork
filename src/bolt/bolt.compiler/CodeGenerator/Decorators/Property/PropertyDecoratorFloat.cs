using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyDecoratorFloat : PropertyDecorator<PropertyTypeFloat> {
    public override int ByteSize {
      get { return 4; }
    }

    public override string ClrType {
      get { return typeof(float).FullName; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterFloat();
    }
  }
}

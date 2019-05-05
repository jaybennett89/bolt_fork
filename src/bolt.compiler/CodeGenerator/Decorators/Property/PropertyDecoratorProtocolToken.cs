using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyDecoratorProtocolToken : PropertyDecorator<PropertyTypeProcotolToken> {
    public override string ClrType {
      get { return "Bolt.IProtocolToken"; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterProtocolToken();
    }
  }
}

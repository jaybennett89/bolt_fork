using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyDecoratorNetworkId : PropertyDecorator<PropertyTypeNetworkId> {
    public override string ClrType {
      get { return "Bolt.NetworkId"; }
    }

    public override int ByteSize {
      get { return 8; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterNetworkId();
    }
  }
}

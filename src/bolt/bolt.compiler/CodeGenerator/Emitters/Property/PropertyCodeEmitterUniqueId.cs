using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyCodeEmitterNetworkId : PropertyCodeEmitterSimple<PropertyDecoratorNetworkId> {
    public override string ReadMethod {
      get { return "ReadNetworkId"; }
    }

    public override string PackMethod {
      get { return "PackNetworkId"; }
    }
  }
}

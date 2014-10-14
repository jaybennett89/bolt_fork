using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyCodeEmitterUniqueId : PropertyCodeEmitterSimple<PropertyDecoratorUniqueId> {
    public override string ReadMethod {
      get { return "ReadUniqueId"; }
    }

    public override string PackMethod {
      get { return "PackUniqueId"; }
    }
  }
}

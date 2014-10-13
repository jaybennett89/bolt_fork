using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyCodeEmitterPrefabId : PropertyCodeEmitterSimple<PropertyDecoratorPrefabId> {
    public override string ReadMethod {
      get { return "ReadPrefabId"; }
    }

    public override string PackMethod {
      get { return "PackPrefabId"; }
    }
  }
}

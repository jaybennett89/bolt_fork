using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyCodeEmitterBool : PropertyCodeEmitterSimple<PropertyDecoratorBool> {
    public override string ReadMethod {
      get { return "ReadBool"; }
    }

    public override string PackMethod {
      get { return "PackBool"; }
    }
  }
}

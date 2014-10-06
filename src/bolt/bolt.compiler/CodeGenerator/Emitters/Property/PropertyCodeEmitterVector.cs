using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyCodeEmitterVector : PropertyCodeEmitterSimple<PropertyDecoratorVector> {
    public override string ReadMethod {
      get { return "ReadVector3"; }
    }

    public override string PackMethod {
      get { return "PackVector3"; }
    }
  }
}

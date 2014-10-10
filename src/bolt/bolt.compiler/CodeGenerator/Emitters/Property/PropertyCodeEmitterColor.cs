using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyCodeEmitterColor : PropertyCodeEmitterSimple<PropertyDecoratorColor> {
    public override string ReadMethod {
      get { return "ReadColor"; }
    }

    public override string PackMethod {
      get { return "PackColor"; }
    }
  }
}

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler{
  class PropertyCodeEmitterEntity : PropertyCodeEmitterSimple<PropertyDecoratorEntity> {
    public override string ReadMethod {
      get { return "ReadEntity"; }
    }

    public override string PackMethod {
      get { return "PackEntity"; }
    }
  }
}

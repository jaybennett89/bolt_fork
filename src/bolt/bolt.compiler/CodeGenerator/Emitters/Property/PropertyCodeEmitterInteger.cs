using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyCodeEmitterInteger : PropertyCodeEmitterSimple<PropertyDecoratorInteger> {
    public override string ReadMethod {
      get { return "ReadI32"; }
    }

    public override string PackMethod {
      get { return "PackI32"; }
    }
    public override string[] EmitSetPropertyDataArgument() {
      List<string> propertyData = new List<string>();

      if (Decorator.DefiningAsset is StateDecorator) {
        propertyData.Add(Decorator.Definition.StateAssetSettings.GetMecanimDataExpression());
      }

      return propertyData.ToArray();
    }
  }
}

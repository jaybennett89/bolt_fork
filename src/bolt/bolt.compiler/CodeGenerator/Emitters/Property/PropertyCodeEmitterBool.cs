using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyCodeEmitterBool : PropertyCodeEmitter<PropertyDecoratorBool> {
    public override bool AllowSetter {
      get {
        var s = Decorator.Definition.StateAssetSettings;
        if (s != null) {
          return s.MecanimMode == MecanimMode.Disabled || s.MecanimDirection == MecanimDirection.UsingBoltProperties;
        }

        return true;
      }
    }
  }
}

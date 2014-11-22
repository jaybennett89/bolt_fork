using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyDecoratorInteger : PropertyDecorator<PropertyTypeInteger> {
    public override string ClrType {
      get { return typeof(int).FullName; }
    }

    public override bool OnSimulateAfterCallback {
      get {
        if (Definition.StateAssetSettings != null) {
          return Definition.StateAssetSettings.MecanimMode != MecanimMode.Disabled;
        }

        return false;
      }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterInteger();
    }
  }
}

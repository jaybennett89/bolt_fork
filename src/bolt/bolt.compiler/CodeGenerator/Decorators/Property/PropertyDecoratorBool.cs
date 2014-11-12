using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyDecoratorBool : PropertyDecorator<PropertyTypeBool> {
    public override string ClrType {
      get { return typeof(bool).FullName; }
    }

    public override int ByteSize {
      get { return 4; }
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
      return new PropertyCodeEmitterBool();
    }
  }
}

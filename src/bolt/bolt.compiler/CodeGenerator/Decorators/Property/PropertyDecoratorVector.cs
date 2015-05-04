using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyDecoratorVector : PropertyDecorator<PropertyTypeVector> {
    public override string ClrType {
      get { return "UE.Vector3"; }
    }

    public override int RequiredStorage {
      get {
        if (Definition.StateAssetSettings != null && (Definition.StateAssetSettings.SmoothingAlgorithm != SmoothingAlgorithms.None)) {
          return 2;
        }

        if (Definition.CommandAssetSettings != null && Definition.CommandAssetSettings.SmoothCorrection) {
          return 2;
        }

        return base.RequiredStorage;
      }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterVector();
    }
  }
}

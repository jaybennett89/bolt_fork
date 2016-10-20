using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyDecoratorPrefabId : PropertyDecorator<PropertyTypePrefabId> {
    public override string ClrType {
      get { return "Bolt.PrefabId"; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterPrefabId();
    }
  }
}

﻿using System;
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
    public override string EmitSetPropertyDataArgument() {
      if (Decorator.DefiningAsset is StateDecorator || Decorator.DefiningAsset is StructDecorator) {
        var s = Decorator.Definition.StateAssetSettings;
        return string.Format(
          "new Bolt.PropertyMecanimData {{ Mode = Bolt.MecanimMode.{0}, OwnerDirection = Bolt.MecanimDirection.{1}, ControllerDirection = Bolt.MecanimDirection.{2}, OthersDirection = Bolt.MecanimDirection.{3}, Layer = {4}, Damping = {5}f }}",
          s.MecanimMode,
          s.MecanimOwnerDirection,
          s.MecanimControllerDirection,
          s.MecanimOthersDirection,
          s.MecanimLayer,
          s.MecanimDamping
        );
      }
      else {
        return null;
      }
    }
  }
}

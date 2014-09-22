﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyDecoratorTransform : PropertyDecorator<PropertyTypeTransform> {
    public override int ByteSize {
      get {
        if (Definition.StateAssetSettings.Options.Contains(StatePropertyOptions.Extrapolate)) {
          // position + rotation + velocity + acceleration
          return 12 + 16 + 12 + 4;
        }
        else {
          // position + rotation
          return 12 + 16;
        }
      }
    }

    public override string ClrType {
      get { return "UE.Transform"; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterTransform();
    }
  }
}

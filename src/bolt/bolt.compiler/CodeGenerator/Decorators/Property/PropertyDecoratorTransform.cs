using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyDecoratorTransform : PropertyDecorator<PropertyTypeTransform> {
    public override int ByteSize {
      get {
        return 12 + 12 + 16;
      }
    }

    public override bool OnRenderCallback {
      get { return true; }
    }

    public override bool OnSimulateAfterCallback {
      get { return true; }
    }

    public override bool OnSimulateBeforeCallback {
      get { return true; }
    }

    public override int ObjectSize {
      get { return 1; }
    }

    public override string ClrType {
      get { return "UE.Transform"; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterTransform();
    }
  }
}

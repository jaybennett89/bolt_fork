using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyDecoratorTrigger : PropertyDecorator {
    public override string ClrType {
      get { return "System.Action"; }
    }

    public string TriggerListener {
      get { return "On" + Definition.Name; }
    }

    public string TriggerMethod {
      get { return Definition.Name; }
    }

    public override bool OnSimulateAfterCallback {
      get { return true; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterTrigger();
    }
  }
}

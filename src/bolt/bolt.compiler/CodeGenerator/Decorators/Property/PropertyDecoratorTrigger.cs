using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyDecoratorTrigger : PropertyDecorator {
    public override string ClrType {
      get { throw new NotImplementedException(); }
    }

    public override int ByteSize {
      get { return 16; }
    }

    public override int ObjectSize {
      get { return 1; }
    }

    public string SetMethodName {
      get { return Definition.Name; }
    }

    public override bool OnSimulateAfterCallback {
      get { return true; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterTrigger();
    }

    public override void FindAllProperties(List<StateProperty> all, StateProperty p) {
      all.Add(
        p
          .Combine(Definition.Filters, Definition.Controller)
          .Combine(all.Count, this)
      );
    }
  }
}

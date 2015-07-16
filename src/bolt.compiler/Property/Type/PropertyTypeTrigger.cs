using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyTypeTrigger : PropertyType {
    public override bool InterpolateAllowed {
      get { return false; }
    }

    public override bool CallbackAllowed {
      get { return false; }
    }

    public override bool IsValue {
      get { return false; }
    }

    public override bool HasSettings {
      get { return false; }
    }

    public override bool MecanimApplicable {
      get { return true; }
    }

    public override PropertyDecorator CreateDecorator() {
      return new PropertyDecoratorTrigger();
    }
  }
}

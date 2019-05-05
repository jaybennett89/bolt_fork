﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyTypeColor : PropertyType {
    [ProtoMember(10)]
    public bool StrictEquality;

    public override bool InterpolateAllowed {
      get { return true; }
    }

    public override bool HasSettings {
      get { return false; }
    }

    public override bool CanSmoothCorrections {
      get { return false; }
    }

    public override PropertyDecorator CreateDecorator() {
      return new PropertyDecoratorColor();
    }
  }
}

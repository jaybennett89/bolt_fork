using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract()]
  public class PropertyTypeInteger : PropertyType {
    [ProtoMember(2)]
    public int MinValue;

    [ProtoMember(3)]
    public int MaxValue;

    [ProtoMember(4)]
    public bool CompressionEnabled;

    public override bool HasSettings {
      get { return true; }
    }

    public override bool MecanimApplicable {
      get { return true; }
    }

    public override PropertyDecorator CreateDecorator() {
      return new PropertyDecoratorInteger();
    }
  }
}

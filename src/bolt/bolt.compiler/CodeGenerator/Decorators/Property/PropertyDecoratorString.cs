using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyDecoratorString : PropertyDecorator<PropertyTypeString> {
    public override int ByteSize {
      get { return 4 + PropertyType.EncodingClass.GetMaxByteCount(PropertyType.MaxLength); }
    }

    public override string ClrType {
      get { return typeof(string).FullName; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterString();
    }
  }
}

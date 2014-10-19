using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyCodeEmitterInteger : PropertyCodeEmitterSimple<PropertyDecoratorInteger> {
    public override string ReadMethod {
      get { return "ReadI32"; }
    }

    public override string PackMethod {
      get { return "PackI32"; }
    }

    public override void AddSettingsArgument(List<string> settings) {
      if (Decorator.PropertyType.CompressionEnabled) {
        var pt = Decorator.PropertyType;
        settings.Add(string.Format("Bolt.PropertyIntCompressionSettings.Create({0}, {1})", BitsRequired(pt.MaxValue - pt.MinValue), -pt.MinValue));
      }
      else {
        settings.Add("Bolt.PropertyIntCompressionSettings.Create()");
      }
    }

    static int BitsRequired(int number) {
      if (number < 0) {
        return 32;
      }

      if (number == 0) {
        return 1;
      }

      for (int i = 31; i >= 0; --i) {
        int b = 1 << i;

        if ((number & b) == b) {
          return i + 1;
        }
      }

      throw new Exception();
    }
  }
}

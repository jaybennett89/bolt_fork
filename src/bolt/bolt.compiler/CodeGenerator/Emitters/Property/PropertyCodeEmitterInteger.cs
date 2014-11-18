using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyCodeEmitterInteger : PropertyCodeEmitter<PropertyDecoratorInteger> {
    public override void AddSettingsArgument(List<string> settings) {
      if (Decorator.PropertyType.CompressionEnabled) {
        var pt = Decorator.PropertyType;
        settings.Add(string.Format("Bolt.PropertyIntCompressionSettings.Create({0}, {1})", pt.BitsRequired, -pt.MinValue));
      }
      else {
        settings.Add("Bolt.PropertyIntCompressionSettings.Create()");
      }
    }

  }
}

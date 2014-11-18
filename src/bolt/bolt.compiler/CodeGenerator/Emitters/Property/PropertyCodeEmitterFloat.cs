using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterFloat : PropertyCodeEmitter<PropertyDecoratorFloat> {
    public override void AddSettingsArgument(List<string> settings) {
      settings.Add(Generator.CreateFloatCompressionExpression(Decorator.PropertyType.Compression));
      settings.Add(Generator.CreateSmoothingSettings(Decorator.Definition));
    }
  }
}

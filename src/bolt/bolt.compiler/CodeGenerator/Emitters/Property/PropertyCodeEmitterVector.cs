using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyCodeEmitterVector : PropertyCodeEmitter<PropertyDecoratorVector> {
    public override void AddSettingsArgument(List<string> settings) {
      settings.Add(Generator.CreateVectorCompressionExpression(Decorator.PropertyType.Compression, Decorator.PropertyType.Selection));
      settings.Add(Generator.CreateSmoothingSettings(Decorator.Definition));
    }
  }
}

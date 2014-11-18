using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyCodeEmitterQuaternion : PropertyCodeEmitter<PropertyDecoratorQuaternion> {
    public override void AddSettingsArgument(List<string> settings) {
      settings.Add(Generator.CreateRotationCompressionExpression(Decorator.PropertyType.EulerCompression, Decorator.PropertyType.QuaternionCompression, Decorator.PropertyType.Selection));
      settings.Add(Generator.CreateSmoothingSettings(Decorator.Definition));
    }
  }
}

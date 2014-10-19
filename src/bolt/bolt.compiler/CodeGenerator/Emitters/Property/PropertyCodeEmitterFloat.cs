using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterFloat : PropertyCodeEmitterSimple<PropertyDecoratorFloat> {
    public override string ReadMethod {
      get { return "ReadF32"; }
    }

    public override string PackMethod {
      get { return "PackF32"; }
    }

    public override void AddSettingsArgument(List<string> settings) {
      settings.Add(Generator.CreateFloatCompressionExpression(Decorator.PropertyType.Compression));
      settings.Add(Generator.CreateSmoothingSettings(Decorator.Definition));
    }
  }
}

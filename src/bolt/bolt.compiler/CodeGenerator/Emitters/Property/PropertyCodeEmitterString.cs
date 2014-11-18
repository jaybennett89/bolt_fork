using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterString : PropertyCodeEmitter<PropertyDecoratorString> {
    public override void AddSettingsArgument(List<string> settings) {
      settings.Add(string.Format("new Bolt.PropertyStringSettings {{ Encoding = Bolt.StringEncodings.{0} }}", Decorator.PropertyType.Encoding));
    }
  }
}

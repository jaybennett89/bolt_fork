using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterString : PropertyCodeEmitter<PropertyDecoratorString> {
    public override void AddSettings(CodeExpression expr, CodeStatementCollection statements) {
      statements.Call(expr, "AddStringSettings", Decorator.PropertyType.Encoding.Literal());
    }
  }
}

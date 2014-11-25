using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterFloat : PropertyCodeEmitter<PropertyDecoratorFloat> {
    public override string StorageField {
      get { return "Float0"; }
    }

    public override void AddSettings(CodeExpression expr, CodeStatementCollection statements) {
      EmitFloatSettings(expr, statements, Decorator.PropertyType.Compression);
      EmitInterpolationSettings(expr, statements);
    }
  }
}

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyCodeEmitterQuaternion : PropertyCodeEmitter<PropertyDecoratorQuaternion> {
    public override void AddSettings(CodeExpression expr, CodeStatementCollection statements) {
      EmitQuaternionSettings(expr, statements, Decorator.PropertyType.EulerCompression, Decorator.PropertyType.QuaternionCompression, Decorator.PropertyType.Selection, Decorator.PropertyType.StrictCompare);
      EmitInterpolationSettings(expr, statements);
    }
  }
}

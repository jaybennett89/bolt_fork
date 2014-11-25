using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyCodeEmitterVector : PropertyCodeEmitter<PropertyDecoratorVector> {
    public override string StorageField {
      get { return "Vector3"; }
    }
    public override void AddSettings(CodeExpression expr, CodeStatementCollection statements) {
      EmitVectorSettings(expr, statements, Decorator.PropertyType.Compression, Decorator.PropertyType.Selection);
      EmitInterpolationSettings(expr, statements);
    }
  }
}

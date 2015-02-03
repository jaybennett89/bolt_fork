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

    protected override void EmitSetPropertyValidator(CodeStatementCollection stmts, CodeTypeDeclaration type, CodeSnippetExpression storage, CodeTypeReference interfaceType, bool changed, string name) {
      var ac = Decorator.PropertyType.Compression;

      for (int i = 0; i < ac.Length; ++i) {
        var c = ac[i];

        if (c.Enabled) {
          var axis = (char)(120 + i);

#if DEBUG
          stmts.If("value.{2} < {0}f || value.{2} > {1}f".Expr(c.MinValue, c.MaxValue, axis), ifBody => {
            ifBody.Expr("BoltLog.Warn(\"Axis '{3}' of property '{0}' is being set to a value larger than the compression settings, it will be clamped to [{1}f, {2}f]\")", Decorator.Definition.Name, c.MinValue.ToStringSigned(), c.MaxValue.ToStringSigned(), axis);
          });
#endif

          stmts.Expr("value.{2} = UnityEngine.Mathf.Clamp(value.{2}, {0}, {1})", c.MinValue, c.MaxValue, axis);
        }
      }
    }
  }
}

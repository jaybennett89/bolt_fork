using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyCodeEmitterInteger : PropertyCodeEmitter<PropertyDecoratorInteger> {
    public override string StorageField {
      get { return "Int0"; }
    }

    public override bool AllowSetter {
      get {
        var s = Decorator.Definition.StateAssetSettings;
        if (s != null) {
          return s.MecanimMode == MecanimMode.Disabled || s.MecanimDirection == MecanimDirection.UsingBoltProperties;
        }

        return true;
      }
    }

    public override void AddSettings(CodeExpression expr, CodeStatementCollection statements) {
      if (Decorator.PropertyType.CompressionEnabled) {
        statements.Call(expr, "Settings_Integer",
          "Bolt.PropertyIntCompressionSettings.Create({0}, {1})".Expr( Decorator.PropertyType.BitsRequired, -Decorator.PropertyType.MinValue)
        );
      }
      else {
        statements.Call(expr, "Settings_Integer", "Bolt.PropertyIntCompressionSettings.Create()".Expr());
      }

    }

    protected override void EmitSetPropertyValidator(CodeStatementCollection stmts, CodeTypeDeclaration type, CodeSnippetExpression storage, CodeTypeReference interfaceType, bool changed, string name) {
      var p = Decorator.PropertyType;

      if (p.CompressionEnabled && p.BitsRequired < 32) {
#if DEBUG
        stmts.If("value < {0} || value > {1}".Expr(p.MinValue, p.MaxValue), ifBody => {
          ifBody.Expr("BoltLog.Warn(\"Property '{0}' is being set to a value larger than the compression settings, it will be clamped to [{1}, {2}]\")", Decorator.Definition.Name, p.MinValue.ToStringSigned(), p.MaxValue.ToStringSigned());
        });
#endif

        stmts.Expr("value = UnityEngine.Mathf.Clamp(value, {0}, {1})", p.MinValue, p.MaxValue);
      }
    }
  }
}

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterArray : PropertyCodeEmitter<PropertyDecoratorArray> {
    void DeclareProperty(CodeTypeDeclaration type, bool emitSetter) {
      Action<CodeStatementCollection> getter = get => {
        get.Expr("return new {0}(data, offset + {1}, {2})", Decorator.ClrType, Decorator.ByteOffset, Decorator.PropertyType.ElementCount);
      };

      Action<CodeStatementCollection> setter = set => {
        set.Expr("if (value.length != {0}) throw new ArgumentOutOfRangeException()", Decorator.PropertyType.ElementCount);
        set.Expr("Buffer.BlockCopy(value.data, value.offset, this.data, this.offset + {0}, {1})", Decorator.ByteOffset, Decorator.ByteSize);
      };

      type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, getter, emitSetter ? setter : null);
    }

    public override void EmitShimMembers(CodeTypeDeclaration type) {
      DeclareProperty(type, false);
    }

    public override void EmitModifierMembers(CodeTypeDeclaration type) {
      DeclareProperty(type, true);
    }
  }
}

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterFloat : PropertyCodeEmitter<PropertyDecoratorFloat> {
    void DeclareProperty(CodeTypeDeclaration type, bool emitSetter) {
      Action<CodeStatementCollection> getter = get => {
        get.Expr("return Bolt.Blit.ReadF32(frame.Data, offsetBytes + {0})", Decorator.ByteOffset);
      };

      Action<CodeStatementCollection> setter = set => {
        set.Expr("Bolt.Blit.PackF32(frame.Data, offsetBytes + {0}, value)", Decorator.ByteOffset);
      };

      type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, getter, emitSetter ? setter : null);

      if (Decorator.EmitChangedCallback) {
        EmitChangedCallbackProperty(type, true);
      }
    }

    public override void EmitShimMembers(CodeTypeDeclaration type) {
      DeclareProperty(type, false);
    }

    public override void EmitModifierMembers(CodeTypeDeclaration type) {
      DeclareProperty(type, true);
    }

    public override CodeExpression CreatePropertyArrayInitializerExpression(int byteOffset, int objectOffset) {
      return "new Bolt.PropertySerializerFloat({0}, {1}, {2}, {3})".Expr(byteOffset, Decorator.ByteSize, objectOffset, Decorator.Definition.Priority);
    }
  }
}

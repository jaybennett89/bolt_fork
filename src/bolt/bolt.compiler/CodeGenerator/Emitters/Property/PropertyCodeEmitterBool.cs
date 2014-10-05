using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyCodeEmitterBool : PropertyCodeEmitter<PropertyDecoratorBool> {
    void DeclareProperty(CodeTypeDeclaration type, bool emitSetter) {
      Action<CodeStatementCollection> getter = get => {
        get.Expr("return Bolt.Blit.ReadI32(frame.Data, offsetBytes + {0}) != 0", Decorator.ByteOffset);
      };

      Action<CodeStatementCollection> setter = set => {
        set.Expr("Bolt.Blit.PackI32(frame.Data, offsetBytes + {0}, value ? 1 : 0)", Decorator.ByteOffset);
      };

      type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, getter, emitSetter ? setter : null);
    }

    public override void EmitStructMembers(CodeTypeDeclaration type) {
      DeclareProperty(type, false);
    }

    public override void EmitModifierMembers(CodeTypeDeclaration type) {
      DeclareProperty(type, true);
    }

    public override void EmitCommandMembers(CodeTypeDeclaration type, string bytes, string implType) {
      var property =
        type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, get => {
          get.Expr("return Bolt.Blit.ReadI32({0}, {1}) != 0", bytes, Decorator.ByteOffset);
        }, set => {
          set.Expr("Bolt.Blit.PackI32({0}, {1}, value ? 1 : 0)", bytes, Decorator.ByteOffset);
        });

      property.PrivateImplementationType = new CodeTypeReference(implType);
    }
  }
}

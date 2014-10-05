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
          get.Expr("return Bolt.Blit.ReadF32({0}, {1})", bytes, Decorator.ByteOffset);
        }, set => {
          set.Expr("Bolt.Blit.PackF32({0}, {1}, value)", bytes, Decorator.ByteOffset);
        });

      property.PrivateImplementationType = new CodeTypeReference(implType);
    }
  }
}
 
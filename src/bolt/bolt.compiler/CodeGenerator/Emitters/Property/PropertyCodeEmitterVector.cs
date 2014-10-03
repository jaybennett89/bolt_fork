using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyCodeEmitterVector : PropertyCodeEmitter<PropertyDecoratorVector> {
    void DeclareProperty(CodeTypeDeclaration type, bool emitSetter) {
      Action<CodeStatementCollection> getter = null;
      Action<CodeStatementCollection> setter = null;

      getter = get => {
        get.Expr("return Bolt.Blit.ReadVector3(frame.Data, offsetBytes + {0})", Decorator.ByteOffset, Decorator.ClrType.Substring(3));
      };

      setter = set => {
        set.Expr("Bolt.Blit.PackVector3(frame.Data, offsetBytes + {0}, value)", Decorator.ByteOffset, Decorator.ClrType.Substring(3));
      };

      type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, getter, emitSetter ? setter : null);
    }

    public override void EmitStructMembers(CodeTypeDeclaration type) {
      DeclareProperty(type, false);
    }

    public override void EmitModifierMembers(CodeTypeDeclaration type) {
      DeclareProperty(type, true);
    }
  }
}

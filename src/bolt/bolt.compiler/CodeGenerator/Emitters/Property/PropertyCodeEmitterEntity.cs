using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler{
  class PropertyCodeEmitterEntity : PropertyCodeEmitter<PropertyDecoratorEntity> {
    void DeclareProperty(CodeTypeDeclaration type, bool emitSetter) {
      Action<CodeStatementCollection> getter = get => {
        get.Expr("return BoltCore.FindEntity(new Bolt.InstanceId(Bolt.Blit.ReadI32(frame.Data, offsetBytes + {0}))).UnityObject", Decorator.ByteOffset);
      };

      Action<CodeStatementCollection> setter = set => {
        set.Expr("Bolt.Blit.PackI32(frame.Data, offsetBytes + {0}, value.Entity.InstanceId.Value)", Decorator.ByteOffset);
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

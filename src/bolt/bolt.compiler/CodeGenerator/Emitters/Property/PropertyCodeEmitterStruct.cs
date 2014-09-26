using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterStruct : PropertyCodeEmitter<PropertyDecoratorStruct> {
    void DeclareProperty(CodeTypeDeclaration type, bool emitSetter) {
      Action<CodeStatementCollection> getter = get => {
        get.Expr("return new {0}(frame.Data, offsetBytes + {1}, offsetObjects + {2})", Decorator.ClrType, Decorator.ByteOffset, Decorator.ObjectOffset);
      };

      Action<CodeStatementCollection> setter = set => {
        set.Expr("Array.Copy(value.frame.Data, value.offsetBytes, this.frame.Data, this.offsetBytes + {0}, {1})", Decorator.ByteOffset, Decorator.ByteSize);
        set.Expr("Array.Copy(value.frame.Objects, value.offsetObjects, this.frame.Objects, this.offsetObjects + {0}, {1})", Decorator.ObjectOffset, Decorator.ObjectSize);
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

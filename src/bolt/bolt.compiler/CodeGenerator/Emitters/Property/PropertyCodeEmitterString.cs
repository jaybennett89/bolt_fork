using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterString : PropertyCodeEmitter<PropertyDecoratorString> {
    void DeclareProperty(CodeTypeDeclaration type, bool emitSetter) {
      Action<CodeStatementCollection> getter = get => {
        get.Expr("return Encoding.{0}.GetString(frame.Data, offsetBytes + 4 + {1}, Bolt.Blit.ReadI32(frame.Data, offsetBytes + {1}))", Decorator.PropertyType.Encoding, Decorator.ByteOffset);
      };

      Action<CodeStatementCollection> setter = set => {
        // clamp length
        set.Expr("if (value.Length > {0}) value = value.SubString(0, {0})", Decorator.PropertyType.MaxLength);

        // pack byte length
        set.Expr("Bolt.Blit.PackI32(frame.Data, offsetBytes + {0}, Encoding.{1}.GetByteCount(value))", Decorator.ByteOffset, Decorator.PropertyType.Encoding);

        // pack string data
        set.Expr("int bytes = Encoding.{1}.GetBytes(value, 0, value.Length, frame.Data, offsetBytes + 4 + {0})", Decorator.ByteOffset, Decorator.PropertyType.Encoding);

        // verify size
        set.Expr("Assert.True(bytes >= 0 && bytes <= {0})", Decorator.PropertyType.EncodingClass.GetMaxByteCount(Decorator.PropertyType.MaxLength));
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

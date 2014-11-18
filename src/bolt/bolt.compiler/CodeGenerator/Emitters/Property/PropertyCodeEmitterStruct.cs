using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterStruct : PropertyCodeEmitter<PropertyDecoratorStruct> {
    void DeclareProperty(CodeTypeDeclaration type, bool emitSetter) {
      Action<CodeStatementCollection> getter = get => {
        get.Expr("return ({0})(State.Objects[this.OffsetObjects + {1}])", Decorator.ClrType, Decorator.ObjectOffset);
      };

      type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, getter, null);
    }

    public override void EmitObjectMembers(CodeTypeDeclaration type) {
      DeclareProperty(type, false);
    }

    public override void EmitPropertySetup(DomBlock block, string group, string path) {
      block.Stmts.Expr("{0}.PropertySetup({1}, {2})", Decorator.Struct.Name, group, path);
    }
  }
}

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterArray : PropertyCodeEmitter<PropertyDecoratorArray> {
    public override void EmitObjectMembers(CodeTypeDeclaration type) {
      Action<CodeStatementCollection> getter = get => {
        get.Expr("return ({0}) (State.Objects[this.OffsetObjects + {1}])", Decorator.ClrType, Decorator.OffsetObjects);
      };

      type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, getter, null);
    }

    public override void EmitStateMembers(StateDecorator decorator, CodeTypeDeclaration type) {
      EmitForwardStateMember(decorator, type, false);
    }

    public override void EmitStateInterfaceMembers(CodeTypeDeclaration type) {
      EmitSimpleIntefaceMember(type, true, false);
    }

    public override void EmitPropertySetup(DomBlock block, string group, string path) {
      var tmp = block.TempVar();

      block.Stmts.Expr("path.Push(\"{0}[]\")", Decorator.Definition.Name);

      block.Stmts.For(tmp, tmp + " < " + Decorator.PropertyType.ElementCount, body => {
        PropertyTypeStruct sp = Decorator.PropertyType.ElementType as PropertyTypeStruct;

        if (sp != null) {
          body.Expr("{0}.PropertySetup({1}, {2})", Generator.FindStruct(sp.StructGuid).Name, group, path);
        }
        else {
          PropertyCodeEmitter.Create(Decorator.ElementDecorator).EmitPropertySetup(new DomBlock(body, tmp + "_"), group, path);
        }
      });

      block.Stmts.Expr("path.Pop()", Decorator.Definition.Name);
    }
  }
}

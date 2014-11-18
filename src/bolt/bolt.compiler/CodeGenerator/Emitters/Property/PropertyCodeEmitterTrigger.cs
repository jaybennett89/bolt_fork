using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterTrigger : PropertyCodeEmitter<PropertyDecoratorTrigger> {
    public override void EmitStateInterfaceMembers(CodeTypeDeclaration type) {
      type.DeclareProperty(typeof(System.Action).FullName, Decorator.Definition.Name, get => { }, set => { });

      if (Generator.AllowStatePropertySetters) {
        type.DeclareMethod(typeof(void).FullName, Decorator.Definition.Name + "Trigger", mtd => { });
      }
    }

    public override void EmitStateMembers(StateDecorator decorator, CodeTypeDeclaration type) {
      type.DeclareProperty(typeof(System.Action).FullName, Decorator.Definition.Name, get => {
        get.Expr("return _Root.{0}", Decorator.Definition.Name);
      }, set => {
        set.Expr("_Root.{0} = value", Decorator.Definition.Name);
      });
    }

    public override void EmitObjectMembers(CodeTypeDeclaration type) {
      // callback property
      type.DeclareProperty(typeof(System.Action).FullName, Decorator.Definition.Name, get => {
        get.Expr("return state.Objects[this.OffsetObjects + {0}].Action", Decorator.OffsetObjects);
      }, set => {
        set.Expr("state.Objects[this.OffsetObjects + {0}].Action = value", Decorator.OffsetObjects);
      });
    }
  }
}

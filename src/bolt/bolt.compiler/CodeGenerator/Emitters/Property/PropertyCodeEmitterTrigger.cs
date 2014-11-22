using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterTrigger : PropertyCodeEmitter<PropertyDecoratorTrigger> {
    public override string StorageField
    {
      get { return "Action"; }
    }

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

      type.DeclareMethod(typeof(void).FullName, Decorator.TriggerMethod, method => {
        method.Statements.Expr("_Root.{0}();", Decorator.TriggerMethod);
      });
    }

    public override void EmitObjectMembers(CodeTypeDeclaration type) {
      EmitSimplePropertyMembers(type, new CodeSnippetExpression("Storage"), null, false);

      type.DeclareMethod(typeof(void).FullName, Decorator.TriggerMethod, method => {
        method.Statements.Expr("Storage.Values[this.OffsetStorage + {0}].TriggerLocal.Update(BoltCore.frame, true)", Decorator.OffsetStorage);

        // flag this property as changed
        EmitPropertyChanged(method.Statements, new CodeSnippetExpression("Storage"));
      });
    }
  }
}

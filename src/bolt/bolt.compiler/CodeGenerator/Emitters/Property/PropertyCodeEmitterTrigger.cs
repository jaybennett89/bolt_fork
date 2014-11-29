using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterTrigger : PropertyCodeEmitter<PropertyDecoratorTrigger> {
    public override string StorageField {
      get { return "Action"; }
    }

    public override void EmitObjectMembers(CodeTypeDeclaration type) {
      EmitSimplePropertyMembers(type, new CodeSnippetExpression("Storage"), null, false, Decorator.TriggerListener);

      type.DeclareMethod(typeof(void).FullName, Decorator.TriggerMethod, method => {
        method.Statements.Expr("Storage.Values[this.OffsetStorage + {0}].TriggerLocal.Update(BoltCore.frame, true)", Decorator.OffsetStorage);

        // flag this property as changed
        EmitPropertyChanged(method.Statements, new CodeSnippetExpression("Storage"));
      });
    }
  }
}

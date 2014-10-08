using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterTrigger : PropertyCodeEmitter<PropertyDecoratorTrigger> {
    public override CodeExpression EmitCustomSerializerInitilization(CodeExpression expression) {
      return null;
    }
    public override void EmitModifierInterfaceMembers(CodeTypeDeclaration type) {
      type.DeclareMethod(typeof(void).FullName, Decorator.SetMethodName, method => { });
    }

    public override void EmitModifierMembers(CodeTypeDeclaration type) {
      // trigger method
      type.DeclareMethod(typeof(void).FullName, Decorator.SetMethodName, method => {
        method.Statements.Expr("Bolt.Blit.SetTrigger(frame.Data, frame.Number, offsetBytes + {0}, true)", Decorator.ByteOffset + 8);
      });
    }

    public override void EmitStateInterfaceMembers(CodeTypeDeclaration type) {
      type.DeclareProperty(typeof(System.Action).FullName, Decorator.Definition.Name, get => { }, set => { });
    }

    public override string EmitSetPropertyDataArgument() {
      if (Decorator.DefiningAsset is StateDecorator || Decorator.DefiningAsset is StructDecorator) {
        var s = Decorator.Definition.StateAssetSettings;
        return string.Format(
          "new Bolt.PropertyMecanimData {{ Mode = Bolt.MecanimMode.{0}, OwnerDirection = Bolt.MecanimDirection.{1}, ControllerDirection = Bolt.MecanimDirection.{2}, OthersDirection = Bolt.MecanimDirection.{3}, Layer = {4}, Damping = {5}f }}",
          s.MecanimMode,
          s.MecanimOwnerDirection,
          s.MecanimControllerDirection,
          s.MecanimOthersDirection,
          s.MecanimLayer,
          s.MecanimDamping
        );
      }
      else {
        return null;
      }
    }

    public override void EmitStateMembers(StateDecorator decorator, CodeTypeDeclaration type) {
      type.DeclareProperty(typeof(System.Action).FullName, Decorator.Definition.Name, get => {
        get.Expr("return (new {0}(Frames.first, 0, 0)).{1}", decorator.RootStruct.Name, Decorator.Definition.Name);
      }, set => {
        set.Expr(" (new {0}(Frames.first, 0, 0)).{1} = value", decorator.RootStruct.Name, Decorator.Definition.Name);
      });
    }

    public override void EmitStructMembers(CodeTypeDeclaration type) {
      // callback property
      type.DeclareProperty(typeof(System.Action).FullName, Decorator.Definition.Name, get => {
        get.Expr("return (System.Action) frame.Objects[offsetObjects + {0}]", Decorator.ObjectOffset);
      }, set => {
        set.Expr("frame.Objects[offsetObjects + {0}] = value", Decorator.ObjectOffset);
      });
    }
  }
}

using System.CodeDom;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterTransform : PropertyCodeEmitter<PropertyDecoratorTransform> {
    public override void EmitModifierMembers(CodeTypeDeclaration type) {

    }

    public override void EmitModifierInterfaceMembers(CodeTypeDeclaration type) {

    }

    public override void EmitStateMembers(StateDecorator decorator, CodeTypeDeclaration type) {

    }

    public override void EmitStateInterfaceMembers(CodeTypeDeclaration type) {

    }

    public override void EmitStructMembers(CodeTypeDeclaration type) {

    }

    public override CodeExpression CreatePropertyArrayInitializerExpression(StateDecoratorProperty p) {
      return base.CreatePropertyArrayInitializerExpression(p);
    }
  }
}

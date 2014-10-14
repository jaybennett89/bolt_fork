using System.CodeDom;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterTransform : PropertyCodeEmitter<PropertyDecoratorTransform> {
    public override void EmitModifierMembers(CodeTypeDeclaration type) {

    }

    public override void EmitModifierInterfaceMembers(CodeTypeDeclaration type) {

    }

    public override string[] EmitSetPropertyDataArgument() {
      return new[] { string.Format("new Bolt.PropertySerializerTransformData {{ Algorithm = Bolt.SmoothingAlgorithms.{0} }}", Decorator.Definition.StateAssetSettings.SmoothingAlgorithm) };
    }

    public override CodeExpression EmitCustomSerializerInitilization(CodeExpression expression) {
      return expression;
    }

    public override void EmitStateMembers(StateDecorator decorator, CodeTypeDeclaration type) {
      type.DeclareProperty("Bolt.TransformData", Decorator.Definition.Name, get => {
        get.Expr("return (Bolt.TransformData) Frames.first.Objects[{0}]", Decorator.ObjectOffset);
      }, set => {
        set.Expr("Frames.first.Objects[{0}] = value", Decorator.ObjectOffset);
      });
    }

    public override void EmitStateInterfaceMembers(CodeTypeDeclaration type) {
      type.DeclareProperty("Bolt.TransformData", Decorator.Definition.Name, get => {

      }, (set) => {

      });
    }

    public override void EmitStructMembers(CodeTypeDeclaration type) {

    }
  }
}

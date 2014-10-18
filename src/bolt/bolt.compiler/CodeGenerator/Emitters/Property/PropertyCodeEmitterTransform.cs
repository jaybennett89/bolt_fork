using System.CodeDom;
using System.Collections.Generic;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterTransform : PropertyCodeEmitter<PropertyDecoratorTransform> {
    public override void EmitModifierMembers(CodeTypeDeclaration type) {

    }

    public override void EmitModifierInterfaceMembers(CodeTypeDeclaration type) {

    }

    public override void GetAddSettingsArgument(List<string> settings) {
      var position = Generator.CreateVectorCompressionExpression(Decorator.PropertyType.PositionCompression, Decorator.PropertyType.PositionSelection);
      var rotation = Generator.CreateRotationCompressionExpression(Decorator.PropertyType.RotationCompression, Decorator.PropertyType.RotationCompressionQuaternion, Decorator.PropertyType.RotationSelection);
      settings.Add(string.Format("Bolt.PropertyTransformCompressionSettings.Create({0}, {1})", position, rotation));
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

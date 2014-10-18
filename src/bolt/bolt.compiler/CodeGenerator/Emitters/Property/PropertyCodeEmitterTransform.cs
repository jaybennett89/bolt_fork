using System.CodeDom;
using System.Collections.Generic;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterTransform : PropertyCodeEmitter<PropertyDecoratorTransform> {
    public override void EmitModifierMembers(CodeTypeDeclaration type) {

    }

    public override void EmitModifierInterfaceMembers(CodeTypeDeclaration type) {

    }

    public override void GetAddSettingsArgument(List<string> settings) {
      List<string> args = new List<string>();

      args.Add(Generator.CreateFloatCompressionExpression(Decorator.PropertyType.PositionCompression[Axis.X], (Decorator.PropertyType.PositionSelection & AxisSelections.X) == AxisSelections.X)); 
      args.Add(Generator.CreateFloatCompressionExpression(Decorator.PropertyType.PositionCompression[Axis.Y], (Decorator.PropertyType.PositionSelection & AxisSelections.Y) == AxisSelections.Y));
      args.Add(Generator.CreateFloatCompressionExpression(Decorator.PropertyType.PositionCompression[Axis.Z], (Decorator.PropertyType.PositionSelection & AxisSelections.Z) == AxisSelections.Z));

      if (Decorator.PropertyType.RotationSelection == AxisSelections.XYZ) {
        args.Add(Generator.CreateFloatCompressionExpression(Decorator.PropertyType.RotationCompressionQuaternion));
      }
      else {
        args.Add(Generator.CreateFloatCompressionExpression(Decorator.PropertyType.RotationCompression[Axis.X], (Decorator.PropertyType.RotationSelection & AxisSelections.X) == AxisSelections.X));
        args.Add(Generator.CreateFloatCompressionExpression(Decorator.PropertyType.RotationCompression[Axis.Y], (Decorator.PropertyType.RotationSelection & AxisSelections.Y) == AxisSelections.Y));
        args.Add(Generator.CreateFloatCompressionExpression(Decorator.PropertyType.RotationCompression[Axis.Z], (Decorator.PropertyType.RotationSelection & AxisSelections.Z) == AxisSelections.Z));
      }

      settings.Add(string.Format("Bolt.PropertyTransformCompressionSettings.Create({0})", args.Join(", ")));
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

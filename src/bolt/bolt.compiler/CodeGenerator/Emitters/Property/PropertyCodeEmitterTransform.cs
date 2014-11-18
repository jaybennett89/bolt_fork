using System.CodeDom;
using System.Collections.Generic;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterTransform : PropertyCodeEmitter<PropertyDecoratorTransform> {
    public override void AddSettingsArgument(List<string> settings) {
      var position = Generator.CreateVectorCompressionExpression(Decorator.PropertyType.PositionCompression, Decorator.PropertyType.PositionSelection);
      var rotation = Generator.CreateRotationCompressionExpression(Decorator.PropertyType.RotationCompression, Decorator.PropertyType.RotationCompressionQuaternion, Decorator.PropertyType.RotationSelection);
      settings.Add(string.Format("Bolt.PropertyTransformCompressionSettings.Create({0}, {1})", position, rotation));
      settings.Add(Generator.CreateSmoothingSettings(Decorator.Definition));
    }

    public override void EmitStateInterfaceMembers(CodeTypeDeclaration type) {
      type.DeclareProperty("Bolt.TransformData", Decorator.Definition.Name, get => {

      });
    }

    public override void EmitObjectMembers(CodeTypeDeclaration type) {
      type.DeclareProperty("Bolt.TransformData", Decorator.Definition.Name, get => {
        get.Expr("return (Bolt.TransformData) CurrentFrame.Objects[this.OffsetObjects + {0}]", Decorator.ObjectOffset);
      });
    }
  }
}

using System.CodeDom;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterTransform : PropertyCodeEmitter<PropertyDecoratorTransform> {
    public override void EmitModifierMembers(CodeTypeDeclaration type) {

    }

    public override void EmitModifierInterfaceMembers(CodeTypeDeclaration type) {

    }

    public override CodeExpression EmitCustomSerializerInitilization(CodeExpression expression) {
      return expression;  
    }

    public override void EmitStateMembers(StateDecorator decorator, CodeTypeDeclaration type) {
      type.DeclareProperty("UE.Transform", Decorator.Definition.Name, get => {
        get.Expr("return Frames.first.Objects[{0}] as UE.Transform", Decorator.ObjectOffset);
      }, set => {
        set.Expr("Frames.first.Objects[{0}] = value", Decorator.ObjectOffset);
      });
    }

    public override void EmitStateInterfaceMembers(CodeTypeDeclaration type) {
      type.DeclareProperty("UE.Transform", Decorator.Definition.Name, get => {

      }, (set) => {

      });
    }

    public override void EmitStructMembers(CodeTypeDeclaration type) {

    }

    //internal struct TransformConfiguration {
    //  public Axis[] PositionAxes;
    //  public Axis[] RotationAxes;
    //  public TransformModes TransformMode;
    //  public TransformSpaces Space;
    //  public TransformRotationMode RotationMode;
    //  public FloatCompression QuaternionCompression;
    //}
  }
}

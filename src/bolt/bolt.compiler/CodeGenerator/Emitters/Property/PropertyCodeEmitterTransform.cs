using System.CodeDom;
using System.Collections.Generic;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterTransform : PropertyCodeEmitter<PropertyDecoratorTransform> {
    public override string StorageField {
      get { return "Transform"; }
    }

    public override void AddSettings(CodeExpression expr, CodeStatementCollection stmts) {
      var pt = Decorator.PropertyType;

      EmitVectorSettings(expr, stmts, pt.PositionCompression, pt.PositionSelection);
      EmitQuaternionSettings(expr, stmts, pt.RotationCompression, pt.RotationCompressionQuaternion, pt.RotationSelection);

      switch (Decorator.Definition.StateAssetSettings.SmoothingAlgorithm) {
        case SmoothingAlgorithms.Interpolation:
          EmitInterpolationSettings(expr, stmts);
          break;

        case SmoothingAlgorithms.Extrapolation:
          EmitExtrapolationSettings(expr, stmts);
          break;
      }
    }

    public override void EmitStateInterfaceMembers(CodeTypeDeclaration type) {
      EmitSimpleIntefaceMember(type, true, false);
    }

    public override void EmitStateMembers(StateDecorator decorator, CodeTypeDeclaration type) {
      EmitForwardStateMember(decorator, type, false);
    }

    public override void EmitObjectMembers(CodeTypeDeclaration type) {
      type.DeclareProperty("Bolt.NetworkTransform", Decorator.Definition.Name, get => {
        get.Expr("return Storage.Values[this.OffsetStorage + {0}].Transform", Decorator.OffsetStorage);
      });
    }

    void EmitExtrapolationSettings(CodeExpression expr, CodeStatementCollection stmts) {
      throw new System.NotImplementedException();
    }
  }
}

using System.CodeDom;
using System.Collections.Generic;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterTransform : PropertyCodeEmitter<PropertyDecoratorTransform> {
    public override string StorageField {
      get { return "Transform"; }
    }

    public override void AddSettings(CodeExpression expr, CodeStatementCollection stmts) {
      var pt = Decorator.PropertyType;

      EmitVectorSettings(expr, stmts, pt.PositionCompression, pt.PositionSelection, pt.PositionStrictCompare);
      EmitQuaternionSettings(expr, stmts, pt.RotationCompression, pt.RotationCompressionQuaternion, pt.RotationSelection, pt.RotationStrictCompare);

      switch (Decorator.Definition.StateAssetSettings.SmoothingAlgorithm) {
        case SmoothingAlgorithms.Interpolation:
          EmitInterpolationSettings(expr, stmts);
          break;

        case SmoothingAlgorithms.Extrapolation:
          EmitExtrapolationSettings(expr, stmts);
          break;
      }
    }

    public override void EmitObjectMembers(CodeTypeDeclaration type) {
      type.DeclareProperty("Bolt.NetworkTransform", Decorator.Definition.Name, get => {
        get.Expr("return Storage.Values[this.OffsetStorage + {0}].Transform", Decorator.OffsetStorage);
      });
    }

    void EmitExtrapolationSettings(CodeExpression expr, CodeStatementCollection stmts) {
      var s = Decorator.Definition.StateAssetSettings;

      stmts.Call(expr, "Settings_Extrapolation", 
        "Bolt.PropertyExtrapolationSettings".Expr().Call("Create",
          s.ExtrapolationMaxFrames.Literal(),
          s.ExtrapolationErrorTolerance.Literal(),
          s.SnapMagnitude.Literal(),
          Decorator.PropertyType.ExtrapolationVelocityMode.Literal()
        )
      );
    }
  }
}

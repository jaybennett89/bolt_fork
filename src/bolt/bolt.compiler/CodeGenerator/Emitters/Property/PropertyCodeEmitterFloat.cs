using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterFloat : PropertyCodeEmitterSimple<PropertyDecoratorFloat> {
    public override string ReadMethod {
      get { return "ReadF32"; }
    }

    public override string PackMethod {
      get { return "PackF32"; }
    }

    public override void GetAddSettingsArgument(List<string> settings) {
      var c = Decorator.PropertyType.Compression;
      if (c.Enabled) {
        settings.Add(string.Format("new Bolt.PropertyFloatCompressionSettings({0}, {1}f, {2}f, {3}f)", c.BitsRequired, c.Shift, c.Pack, c.Read));
      }
      else {
        settings.Add(string.Format("Bolt.PropertyFloatCompressionSettings.CreateNoCompression()"));
      }

      var stateSettings = Decorator.Definition.StateAssetSettings;
      if (stateSettings != null) {
        switch (stateSettings.SmoothingAlgorithm) {
          case SmoothingAlgorithms.Interpolation:
            settings.Add("Bolt.PropertySmoothingSettings.CreateInterpolation()");
            break;

          case SmoothingAlgorithms.Extrapolation:
            settings.Add(
              string.Format("Bolt.PropertySmoothingSettings.CreateExtrapolation({0}, {1}, {2}f)",
              stateSettings.ExtrapolationMaxFrames,
              stateSettings.ExtrapolationCorrectionFrames,
              stateSettings.ExtrapolationErrorTolerance)
            );
            break;
        }
      }
    }
  }
}

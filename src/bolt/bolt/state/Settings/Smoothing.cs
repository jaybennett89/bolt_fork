using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal struct PropertySmoothingSettings {
    public SmoothingAlgorithms Algorithm;
    public float SnapMagnitude;
    public int ExtrapolationMaxFrames;
    public int ExtrapolationCorrectionFrames;
    public float ExtrapolationErrorTolerance;
    public ExtrapolationVelocityModes ExtrapolationVelocityMode;

    public static PropertySmoothingSettings CreateInterpolation(float snapMagnitude) {
      return new PropertySmoothingSettings {
        Algorithm = SmoothingAlgorithms.Interpolation,
        SnapMagnitude = snapMagnitude
      };
    }

    public static PropertySmoothingSettings CreateExtrapolation(int maxFrames, int correctionFrames, float errorTolerance, float snapMagnitude, ExtrapolationVelocityModes velocityMode) {
      return new PropertySmoothingSettings {
        Algorithm = SmoothingAlgorithms.Extrapolation,
        SnapMagnitude = snapMagnitude,
        ExtrapolationMaxFrames = maxFrames,
        ExtrapolationCorrectionFrames = correctionFrames,
        ExtrapolationErrorTolerance = errorTolerance,
        ExtrapolationVelocityMode = velocityMode
      };
    }
  }
}

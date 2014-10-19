﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal struct PropertySmoothingSettings {
    public SmoothingAlgorithms Algorithm;
    public int ExtrapolationMaxFrames;
    public int ExtrapolationCorrectionFrames;
    public float ExtrapolationErrorTolerance;
    public ExtrapolationVelocityModes ExtrapolationVelocityMode;

    public static PropertySmoothingSettings CreateInterpolation() {
      return new PropertySmoothingSettings {
        Algorithm = SmoothingAlgorithms.Interpolation
      };
    }

    public static PropertySmoothingSettings CreateExtrapolation(int maxFrames, int correctionFrames, float errorTolerance, ExtrapolationVelocityModes velocityMode) {
      return new PropertySmoothingSettings {
        Algorithm = SmoothingAlgorithms.Extrapolation,
        ExtrapolationMaxFrames = maxFrames,
        ExtrapolationCorrectionFrames = correctionFrames,
        ExtrapolationErrorTolerance = errorTolerance,
        ExtrapolationVelocityMode = velocityMode
      };
    }
  }
}
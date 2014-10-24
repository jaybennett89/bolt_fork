using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  struct PropertyCommandSettings {
    public bool SmoothCorrections;
    public float SnapMagnitude;

    public static PropertyCommandSettings Create(bool smoothCorrections, float snapMagnitude) {
      PropertyCommandSettings s;
      s.SmoothCorrections = smoothCorrections;
      s.SnapMagnitude = snapMagnitude;
      return s;
    }
  }
}

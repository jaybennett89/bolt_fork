using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal struct Axis {
    public const int X = 0;
    public const int Y = 1;
    public const int Z = 2;
    public const int W = 4;

    public bool Enabled;
    public FloatCompression Compression;
  }
}

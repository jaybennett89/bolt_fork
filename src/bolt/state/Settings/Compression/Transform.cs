using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  struct PropertyTransformCompressionSettings {
    public PropertyVectorCompressionSettings Position;
    public PropertyQuaternionCompression Rotation;

    public static PropertyTransformCompressionSettings Create(PropertyVectorCompressionSettings position, PropertyQuaternionCompression rotation) {
      return new PropertyTransformCompressionSettings {
        Position = position,
        Rotation = rotation,
      };
    }
  }
}

using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyTypeTransformDeadReckoningSettings {
    [ProtoMember(1)]
    public bool InferVelocity;

    [ProtoMember(2)]
    public bool InferAcceleration;
  }

  [ProtoContract]
  public class PropertyTypeTransform : PropertyType {
    [ProtoMember(1)]
    public TransformSpaces Space;

    [ProtoMember(2, OverwriteList = true)]
    public Axis[] PositionAxes = new[] {
      new Axis { Component = VectorComponents.X, Compression = FloatCompression.Default(), Enabled = true },
      new Axis { Component = VectorComponents.Y, Compression = FloatCompression.Default(), Enabled = true },
      new Axis { Component = VectorComponents.Z, Compression = FloatCompression.Default(), Enabled = true },
    };

    [ProtoMember(3, OverwriteList = true)]
    public Axis[] RotationAxesEuler = new[] {
      new Axis { Component = VectorComponents.X, Compression = FloatCompression.DefaultAngle(), Enabled = true },
      new Axis { Component = VectorComponents.Y, Compression = FloatCompression.DefaultAngle(), Enabled = true },
      new Axis { Component = VectorComponents.Z, Compression = FloatCompression.DefaultAngle(), Enabled = true },
    };

    [ProtoMember(4)]
    public FloatCompression RotationCompressionQuaternion = new FloatCompression { Fractions = 1000, MinValue = -1, MaxValue = +1 };

    [ProtoMember(5)]
    public TransformRotationMode RotationMode;

    public override bool InterpolateAllowed {
      get { return true; }
    }

    public override bool CallbackAllowed {
      get { return false; }
    }

    public override PropertyDecorator CreateDecorator() {
      return new PropertyDecoratorTransform();
    }
  }
}

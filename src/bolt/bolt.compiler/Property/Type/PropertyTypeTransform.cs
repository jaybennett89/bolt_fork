﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyTypeTransform : PropertyType {
    [ProtoMember(2, OverwriteList = true)]
    public Axis[] PositionAxes = new[] {
      new Axis { Component = VectorComponents.X, Compression = FloatCompression.Default(), Enabled = true },
      new Axis { Component = VectorComponents.Y, Compression = FloatCompression.Default(), Enabled = true },
      new Axis { Component = VectorComponents.Z, Compression = FloatCompression.Default(), Enabled = true },
    };

    [ProtoMember(3, OverwriteList = true)]
    public Axis[] RotationAxes = new[] {
      new Axis { Component = VectorComponents.X, Compression = FloatCompression.DefaultAngle(), Enabled = true },
      new Axis { Component = VectorComponents.Y, Compression = FloatCompression.DefaultAngle(), Enabled = true },
      new Axis { Component = VectorComponents.Z, Compression = FloatCompression.DefaultAngle(), Enabled = true },
    };

    [ProtoMember(4)]
    public FloatCompression RotationCompressionQuaternion = new FloatCompression { MinValue = -1, MaxValue = +1, Accuracy = 0.01f };

    public override bool InterpolateAllowed {
      get { return true; }
    }

    public override bool CallbackAllowed {
      get { return false; }
    }

    public override bool HasSettings {
      get { return true; }
    }

    public Axis GetPositionAxis(VectorComponents component) {
      return PositionAxes[(int)component];
    }
       
    public override PropertyDecorator CreateDecorator() {
      return new PropertyDecoratorTransform();
    }
  }
}

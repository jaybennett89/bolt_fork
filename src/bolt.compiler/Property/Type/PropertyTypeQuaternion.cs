using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyTypeQuaternion : PropertyType {
    [ProtoMember(16)]
    public AxisSelections Selection = AxisSelections.XYZ;

    [ProtoMember(19)]
    public bool StrictEquality;

    [ProtoMember(18, OverwriteList = true)]
    public FloatCompression[] EulerCompression = new FloatCompression[3] {
      new FloatCompression { MinValue = 0, MaxValue = +360, Accuracy = 1f },
      new FloatCompression { MinValue = 0, MaxValue = +360, Accuracy = 1f },
      new FloatCompression { MinValue = 0, MaxValue = +360, Accuracy = 1f },
    };

    [ProtoMember(17)]
    public FloatCompression QuaternionCompression = new FloatCompression { MinValue = -1, MaxValue = +1, Accuracy = 0.01f };

    public override bool StrictCompare {
      get { return StrictEquality; }
    }

    public override bool InterpolateAllowed {
      get { return true; }
    }

    public override bool HasSettings {
      get { return true; }
    }

    public override bool CanSmoothCorrections {
      get { return true; }
    }

    public override void OnCreated() {
      Selection = AxisSelections.XYZ;
    }

    public override PropertyDecorator CreateDecorator() {
      return new PropertyDecoratorQuaternion();
    }
  }
}

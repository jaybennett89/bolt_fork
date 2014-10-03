using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  class PropertySerializerVector : PropertySerializer {
    public PropertySerializerVector(PropertyMetaData info)
      : base(info) {
    }

    public override int CalculateBits(State state, State.Frame frame) {
      return 32 * 8;
    }

    public override bool Pack(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteVector3(frame.Data.ReadVector3(MetaData.ByteOffset));
      return true;
    }

    public override void Read(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      frame.Data.PackVector3(MetaData.ByteOffset, stream.ReadVector3());
    }
  }
}

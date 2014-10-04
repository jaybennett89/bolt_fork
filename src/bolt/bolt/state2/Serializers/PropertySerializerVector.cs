using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  class PropertySerializerVector : PropertySerializer {
    public PropertySerializerVector(StatePropertyMetaData info)
      : base(info) {
    }

    public override int StateBits(State state, State.Frame frame) {
      return 32 * 8;
    }

    public override bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteVector3(frame.Data.ReadVector3(StateData.ByteOffset));
      return true;
    }

    public override void StateRead(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      frame.Data.PackVector3(StateData.ByteOffset, stream.ReadVector3());
    }
  }
}

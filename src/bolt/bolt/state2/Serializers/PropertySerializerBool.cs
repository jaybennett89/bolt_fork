using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  class PropertySerializerBool : PropertySerializer {
    public PropertySerializerBool(StatePropertyMetaData info)
      : base(info) {
    }

    public override int StateBits(State state, State.Frame frame) {
      return 1;
    }

    public override void OnSimulateAfter(State state) {
      if (state.Animator && StateData.Mecanim) {
        state.Animator.SetBool(StateData.PropertyName, state.Frames.first.Data.ReadI32(StateData.ByteOffset) != 0);
      }
    }

    public override bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteBool(frame.Data.ReadI32(StateData.ByteOffset) != 0);
      return true;
    }

    public override void StateRead(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      if (stream.ReadBool()) {
        Blit.PackI32(frame.Data, StateData.ByteOffset, 1);
      }
      else {
        Blit.PackI32(frame.Data, StateData.ByteOffset, 0);
      }
    }
  }
}

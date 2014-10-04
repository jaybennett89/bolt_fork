using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  class PropertySerializerFloat : PropertySerializer {
    public PropertySerializerFloat(StatePropertyMetaData info)
      : base(info) {
    }

    public override int StateBits(State state, State.Frame frame) {
      return 32;
    }

    public override void OnSimulateAfter(State state) {
      if (state.Animator && StateData.Mecanim) {
        state.Animator.SetFloat(StateData.PropertyName, Blit.ReadF32(state.Frames.first.Data, StateData.ByteOffset), StateData.MecanimDamping, BoltCore.frameDeltaTime);
      }
    }

    public override bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteFloat(Blit.ReadF32(frame.Data, StateData.ByteOffset));
      return true;
    }

    public override void StateRead(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      Blit.PackF32(frame.Data, StateData.ByteOffset, stream.ReadFloat());
    }
  }
}

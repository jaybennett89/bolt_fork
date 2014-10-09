using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  class PropertySerializerInteger : PropertySerializerMecanim {
    public PropertySerializerInteger(StatePropertyMetaData info) : base(info) { }
    public PropertySerializerInteger(EventPropertyMetaData meta) : base(meta) { }
    public PropertySerializerInteger(CommandPropertyMetaData meta) : base(meta) { }

    public override int StateBits(State state, State.Frame frame) {
      return 32;
    }

    public override object GetDebugValue(State state) {
      return Blit.ReadI32(state.Frames.first.Data, StateData.ByteOffset);
    }

    protected override void PushMecanimValue(State state) {
      state.Animator.SetInteger(StateData.PropertyName, Blit.ReadI32(state.Frames.first.Data, StateData.ByteOffset));
    }

    protected override void PullMecanimValue(State state) {
      Blit.PackI32(state.Frames.first.Data, StateData.ByteOffset, state.Animator.GetInteger(StateData.PropertyName));
    }

    protected override bool Pack(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      stream.WriteInt(Blit.ReadI32(data, offset));
      return true;
    }

    protected override void Read(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      Blit.PackI32(data, offset, stream.ReadInt());
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {

    }
  }
}

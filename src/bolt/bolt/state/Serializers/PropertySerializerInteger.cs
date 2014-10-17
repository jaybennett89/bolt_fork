using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  class PropertySerializerInteger : PropertySerializerMecanim {
    public override int StateBits(State state, State.Frame frame) {
      return 32;
    }

    public override object GetDebugValue(State state) {
      return Blit.ReadI32(state.Frames.first.Data, Settings.ByteOffset);
    }

    protected override void PushMecanimValue(State state) {
      state.Animator.SetInteger(Settings.PropertyName, Blit.ReadI32(state.Frames.first.Data, Settings.ByteOffset));
    }

    protected override void PullMecanimValue(State state) {
      Blit.PackI32(state.Frames.first.Data, Settings.ByteOffset, state.Animator.GetInteger(Settings.PropertyName));
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  class PropertySerializerBool : PropertySerializerMecanim {
    public override int StateBits(State state, State.NetworkFrame frame) {
      return 1;
    }

    public override object GetDebugValue(State state) {
      return state.Frames.first.Data.ReadBool(SettingsOld.ByteOffset);
    }

    protected override void PullMecanimValue(State state) {
      state.Frames.first.Data.PackBool(SettingsOld.ByteOffset, state.Animator.GetBool(SettingsOld.PropertyName));
    }

    protected override void PushMecanimValue(State state) {
      state.Animator.SetBool(SettingsOld.PropertyName, state.Frames.first.Data.ReadBool(SettingsOld.ByteOffset));
    }

    protected override bool Pack(byte[] data, BoltConnection connection, UdpPacket stream) {
      stream.WriteBool(Blit.ReadBool(data, SettingsOld.ByteOffset));
      return true;
    }

    protected override void Read(byte[] data, BoltConnection connection, UdpPacket stream) {
      Blit.PackBool(data, SettingsOld.ByteOffset, stream.ReadBool());
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {

    }
  }
}

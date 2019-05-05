using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  class PropertySerializerBool : PropertySerializerMecanim {
    public override int StateBits(State state, NetworkFrame frame) {
      return 1;
    }

    public override object GetDebugValue(State state) {
      return state.CurrentFrame.Storage[Settings.OffsetStorage].Bool;
    }

    protected override void PullMecanimValue(State state) {
      state.CurrentFrame.Storage[Settings.OffsetStorage].Bool = state.Animator.GetBool(Settings.PropertyName);
    }

    protected override void PushMecanimValue(State state) {
      state.Animator.SetBool(Settings.PropertyName, state.CurrentFrame.Storage[Settings.OffsetStorage].Bool);
    }

    protected override bool Pack(NetworkValue[] storage, BoltConnection connection, UdpPacket stream) {
      stream.WriteBool(storage[Settings.OffsetStorage].Bool);
      return true;
    }

    protected override void Read(NetworkValue[] storage, BoltConnection connection, UdpPacket stream) {
      storage[Settings.OffsetStorage].Bool = stream.ReadBool();
    }

    public override void CommandSmooth(NetworkValue[] from, NetworkValue[] to, NetworkValue[] into, float t) {

    }
  }
}

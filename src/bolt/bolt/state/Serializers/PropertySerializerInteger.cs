using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  class PropertySerializerInteger : PropertySerializerMecanim {
    PropertyIntCompressionSettings IntCompression;

    public void AddSettings(PropertyIntCompressionSettings intCompression) {
      IntCompression = intCompression;
    }

    public override int StateBits(State state, NetworkFrame frame) {
      return IntCompression.BitsRequired;
    }

    public override object GetDebugValue(State state) {
      return state.CurrentFrame.Storage[Settings.OffsetStorage].Int0;
    }

    protected override void PushMecanimValue(State state) {
      state.Animator.SetInteger(Settings.PropertyName, state.CurrentFrame.Storage[Settings.OffsetStorage].Int0);
    }

    protected override void PullMecanimValue(State state) {
      state.CurrentFrame.Storage[Settings.OffsetStorage].Int0 = state.Animator.GetInteger(Settings.PropertyName);
    }

    protected override bool Pack(NetworkValue[] data, BoltConnection connection, UdpPacket stream) {
      IntCompression.Pack(stream, data[Settings.OffsetStorage].Int0);
      return true;
    }

    protected override void Read(NetworkValue[] data, BoltConnection connection, UdpPacket stream) {
      data[Settings.OffsetStorage].Int0 = IntCompression.Read(stream);
    }

    public override void CommandSmooth(NetworkValue[] from, NetworkValue[] to, NetworkValue[] into, float t) {
      
    }
  }
}
 
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

    public override int StateBits(State state, State.NetworkFrame frame) {
      return IntCompression.BitsRequired;
    }

    public override object GetDebugValue(State state) {
      return Blit.ReadI32(state.Frames.first.Data, SettingsOld.ByteOffset);
    }

    protected override void PushMecanimValue(State state) {
      state.Animator.SetInteger(SettingsOld.PropertyName, Blit.ReadI32(state.Frames.first.Data, SettingsOld.ByteOffset));
    }

    protected override void PullMecanimValue(State state) {
      Blit.PackI32(state.Frames.first.Data, SettingsOld.ByteOffset, state.Animator.GetInteger(SettingsOld.PropertyName));
    }

    protected override bool Pack(byte[] data, BoltConnection connection, UdpPacket stream) {
      IntCompression.Pack(stream, Blit.ReadI32(data, SettingsOld.ByteOffset));
      return true;
    }

    protected override void Read(byte[] data,BoltConnection connection, UdpPacket stream) {
      Blit.PackI32(data, SettingsOld.ByteOffset, IntCompression.Read(stream));
    }
  }
}
 
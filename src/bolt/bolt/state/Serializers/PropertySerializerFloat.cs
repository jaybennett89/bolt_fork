using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UE = UnityEngine;
using UdpKit;

namespace Bolt {
  class PropertySerializerFloat : PropertySerializerMecanim {
    PropertyFloatCompressionSettings CompressionSettings;

    public void AddSettings(PropertyFloatCompressionSettings compressionSettings) {
      CompressionSettings = compressionSettings;
    }

    public override int StateBits(State state, State.Frame frame) {
      return 32;
    }

    public override object GetDebugValue(State state) {
      return Blit.ReadF32(state.Frames.first.Data, Settings.ByteOffset);
    }

    protected override void PullMecanimValue(State state) {
      state.Frames.first.Data.PackF32(Settings.ByteOffset, state.Animator.GetFloat(Settings.PropertyName));
    }

    protected override void PushMecanimValue(State state) {
      state.Animator.SetFloat(Settings.PropertyName, Blit.ReadF32(state.Frames.first.Data, Settings.ByteOffset), MecanimSettings.Damping, BoltCore.frameDeltaTime);
    }

    protected override bool Pack(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      stream.WriteFloat(Blit.ReadF32(data, offset));
      return true;
    }

    protected override void Read(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      Blit.PackF32(data, offset, stream.ReadFloat());
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {
      float v0 = from.ReadF32(Settings.ByteOffset);
      float v1 = to.ReadF32(Settings.ByteOffset);
      into.PackF32(Settings.ByteOffset, UE.Mathf.Lerp(v0, v1, t));
    }
  }
}

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

    public new void AddSettings(PropertyStateSettings stateSettings) {
      Assert.True(stateSettings.ByteLength == 8);
      StateSettings = stateSettings;
      StateSettings.ByteLength = 4;
    }

    public override void OnSimulateBefore(State state) {
      if (state.Entity.IsDummy) {
        var f = state.Frames.first;

        switch (SmoothingSettings.Algorithm) {
          case SmoothingAlgorithms.Interpolation:
            f.Data.PackF32(SettingsOld.ByteOffset, Bolt.Math.InterpolateFloat(state.Frames, SettingsOld.ByteOffset + 4, state.Entity.Frame));
            break;

          case SmoothingAlgorithms.Extrapolation:
            f.Data.PackF32(SettingsOld.ByteOffset, Bolt.Math.ExtrapolateFloat(state.Frames, SettingsOld.ByteOffset + 4, state.Entity.Frame, SmoothingSettings, f.Data.ReadF32(SettingsOld.ByteOffset)));
            break;
        }
      }
    }

    public override int StateBits(State state, State.NetworkFrame frame) {
      return CompressionSettings.BitsRequired;
    }

    public override object GetDebugValue(State state) {
      return Blit.ReadF32(state.Frames.first.Data, SettingsOld.ByteOffset);
    }

    protected override void PullMecanimValue(State state) {
      state.Frames.first.Data.PackF32(SettingsOld.ByteOffset, state.Animator.GetFloat(SettingsOld.PropertyName));
    }

    protected override void PushMecanimValue(State state) {
      state.Animator.SetFloat(SettingsOld.PropertyName, Blit.ReadF32(state.Frames.first.Data, SettingsOld.ByteOffset), MecanimSettings.Damping, BoltCore.frameDeltaTime);
    }

    protected override bool Pack(byte[] data, BoltConnection connection, UdpPacket stream) {
      CompressionSettings.Pack(stream, Blit.ReadF32(data, SettingsOld.ByteOffset));
      return true;
    }

    protected override void Read(byte[] data, BoltConnection connection, UdpPacket stream) {
      int offset = SettingsOld.ByteOffset;

      if (SettingsOld.PropertyMode == PropertyModes.State && SmoothingSettings.Algorithm != SmoothingAlgorithms.None) {
        offset += 4;
      }

      Blit.PackF32(data, offset, CompressionSettings.Read(stream));
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {
      float v0 = from.ReadF32(SettingsOld.ByteOffset);
      float v1 = to.ReadF32(SettingsOld.ByteOffset);
      into.PackF32(SettingsOld.ByteOffset, UE.Mathf.Lerp(v0, v1, t));
    }
  }
}

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

    public override void OnSimulateBefore(State state) {
      if (state.Entity.IsDummy) {
        var f = state.CurrentFrame;

        switch (SmoothingSettings.Algorithm) {
          case SmoothingAlgorithms.Interpolation:
            f.Storage[Settings.OffsetStorage].Float0 = Bolt.Math.InterpolateFloat(state.Frames, Settings.OffsetStorage, state.Entity.Frame);
            break;
        }
      }
    }

    public override int StateBits(State state, NetworkFrame frame) {
      return CompressionSettings.BitsRequired;
    }

    public override object GetDebugValue(State state) {
      return state.CurrentFrame.Storage[Settings.OffsetStorage].Float0;
    }

    protected override void PullMecanimValue(State state) {
      state.CurrentFrame.Storage[Settings.OffsetStorage].Float0 = state.Animator.GetFloat(Settings.PropertyName);
    }

    protected override void PushMecanimValue(State state) {
      state.Animator.SetFloat(Settings.PropertyName, state.CurrentFrame.Storage[Settings.OffsetStorage].Float0, MecanimSettings.Damping, BoltCore.frameDeltaTime);
    }

    protected override bool Pack(NetworkValue[] storage, BoltConnection connection, UdpPacket stream) {
      CompressionSettings.Pack(stream, storage[Settings.OffsetStorage].Float0);
      return true;
    }

    protected override void Read(NetworkValue[] storage, BoltConnection connection, UdpPacket stream) {
      float vale = CompressionSettings.Read(stream);

      if (Settings.PropertyMode == PropertyModes.State && SmoothingSettings.Algorithm != SmoothingAlgorithms.None) {
        storage[Settings.OffsetStorage].Float1 = vale;
      }
      else {
        storage[Settings.OffsetStorage].Float0 = vale;
      }
    }

    public override void CommandSmooth(NetworkValue[] from, NetworkValue[] to, NetworkValue[] into, float t) {
      float v0 = from[Settings.OffsetStorage].Float0;
      float v1 = to[Settings.OffsetStorage].Float0;
      into[Settings.OffsetStorage].Float0 = UE.Mathf.Lerp(v0, v1, t);
    }
  }
}

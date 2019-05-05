﻿using UdpKit;

namespace Bolt {
  internal class NetworkProperty_Integer : NetworkProperty_Mecanim {
    PropertyIntCompressionSettings Compression;

    public void Settings_Integer(PropertyIntCompressionSettings compression) {
      Compression = compression;
    }

    public override int BitCount(NetworkObj obj) {
      return Compression.BitsRequired;
    }

    public override void SetDynamic(NetworkObj obj, object value) {
      if (MecanimDirection == Bolt.MecanimDirection.UsingAnimatorMethods) {
        BoltLog.Error("Can't call SetDynamic on an integer in 'UsingAnimatorMethods' mode");
        return;
      }

      var v = (int)value;

      if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Int0, v)) {
        obj.Storage.Values[obj[this]].Int0 = v;
        obj.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);
      }
    }

    public override object GetDynamic(NetworkObj obj) {
      return obj.Storage.Values[obj[this]].Int0;
    }

    public override object DebugValue(NetworkObj obj, NetworkStorage storage) {
      return storage.Values[obj[this]].Int0;
    }

    public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      Compression.Pack(packet, storage.Values[obj[this]].Int0);
      return true;
    }

    public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      storage.Values[obj[this]].Int0 = Compression.Read(packet);
    }

    protected override void PullMecanimValue(NetworkState state) {
      if(state.Animator == null) {
        return;
      }

      int newValue = state.Animator.GetInteger(PropertyName);
      int oldValue = state.Storage.Values[state[this]].Int0;

      state.Storage.Values[state[this]].Int0 = newValue;

      if (NetworkValue.Diff(newValue, oldValue)) {
        state.Storage.PropertyChanged(state.OffsetProperties + this.OffsetProperties);
      }
    }

    protected override void PushMecanimValue(NetworkState state) {
      for (int i = 0; i < state.Animators.Count; ++i) {
        state.Animators[i].SetInteger(PropertyName, state.Storage.Values[state[this]].Int0);
      }
    }
  }
}

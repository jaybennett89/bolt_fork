using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  internal class NetworkProperty_Integer : NetworkProperty_Mecanim {
    PropertyIntCompressionSettings Compression;

    public void Settings_Integer(PropertyIntCompressionSettings compression) {
      Compression = compression;
    }

    public override int BitCount(NetworkObj obj) {
      return Compression.BitsRequired;
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
      state.Storage.Values[state[this]].Int0 = state.Animator.GetInteger(PropertyName);
    }

    protected override void PushMecanimValue(NetworkState state) {
      for (int i = 0; i < state.Animators.Count; ++i)
      {
        state.Animators[i].SetInteger(PropertyName, state.Storage.Values[state[this]].Int0);
      }
    }
  }
}

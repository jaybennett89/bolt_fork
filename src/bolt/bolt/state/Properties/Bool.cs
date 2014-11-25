using UdpKit;

namespace Bolt {
  internal class NetworkProperty_Bool : NetworkProperty_Mecanim {
    public override int BitCount(NetworkObj obj) {
      return 1;
    }

    public override object DebugValue(NetworkObj obj, NetworkStorage storage) {
      return storage.Values[obj[this]].Bool;
    }

    public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      packet.WriteBool(storage.Values[obj[this]].Bool);
      return true;
    }

    public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      storage.Values[obj[this]].Bool = packet.ReadBool();
    }

    protected override void PullMecanimValue(NetworkState state) {
      state.Storage.Values[state[this]].Bool = state.Animator.GetBool(PropertyName);
    }

    protected override void PushMecanimValue(NetworkState state) {
      for (int i = 0; i < state.Animators.Count; ++i)
      {
        state.Animators[i].SetBool(PropertyName, state.Storage.Values[state[this]].Bool);
      }
    }
  }
}

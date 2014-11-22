namespace Bolt {
  internal class NetworkProperty_Trigger : NetworkProperty_Mecanim {
    public override object DebugValue(NetworkObj obj, NetworkStorage storage) {
      return "TRIGGER";
    }

    public override int BitCount(NetworkObj obj) {
      return 32;
    }

    public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpKit.UdpPacket packet) {
      // adjust trigger
      storage.Values[obj[this]].TriggerSend.Update(BoltCore.frame, false);

      // write history
      packet.WriteInt(storage.Values[obj[this]].TriggerSend.History);
      return true;
    }

    public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpKit.UdpPacket packet) {
      storage.Values[obj[this]].TriggerLocal.Frame = storage.Frame;
      storage.Values[obj[this]].TriggerLocal.History = packet.ReadInt();
    }

    public override void OnSimulateAfter(NetworkObj obj) {
      var it = obj.RootState.Frames.GetIterator();

      while (it.Next()) {
        if (InvokeForFrame(obj, it.val) == false) {
          break;
        }
      }
    }

    bool InvokeForFrame(NetworkObj obj, NetworkStorage storage) {
      if (MecanimMode == MecanimMode.Disabled) {
        return MecanimPushOrNone(obj, storage, false);
      }
      else {
        if (ShouldPullDataFromMecanim(obj.RootState)) {
          return MecanimPull(obj, storage);
        }
        else {
          return MecanimPushOrNone(obj, storage, true);
        }
      }
    }

    bool MecanimPull(NetworkObj obj, NetworkStorage storage) {
      if (obj.RootState.Animator.GetBool(PropertyName) && (obj.RootState.Animator.IsInTransition(MecanimLayer) == false)) {
        storage.Values[obj[this]].TriggerSend.Update(BoltCore.frame, true);

        var cb = obj.Storage.Values[obj[this]].Action;
        if (cb != null) {
          cb();
        }
      }

      return false;
    }

    bool MecanimPushOrNone(NetworkObj obj, NetworkStorage storage, bool push) {
      var t_frame = storage.Values[obj[this]].TriggerLocal.Frame;
      var t_history = storage.Values[obj[this]].TriggerLocal.History;
      var t_callback = storage.Values[obj[this]].Action;

      for (int i = 31; (i >= 0) && (t_history != 0); --i) {
        if (t_frame - i > obj.RootState.Entity.Frame) {
          return false;
        }

        int b = 1 << i;

        if ((t_history & b) == b) {
          // clear history for this bit
          storage.Values[obj[this]].TriggerLocal.History = t_history = (t_history & ~b);

          // update send trigger
          storage.Values[obj[this]].TriggerSend.Update(BoltCore.frame, true);

          if (push)
          {
            for (int a = 0; a < obj.RootState.Animators.Count; ++a)
            {
              obj.RootState.Animators[a].SetTrigger(PropertyName);
            }
          }

          if (t_callback != null)
          {
            t_callback();
          }
        }
      }

      return true;
    }
  }
}

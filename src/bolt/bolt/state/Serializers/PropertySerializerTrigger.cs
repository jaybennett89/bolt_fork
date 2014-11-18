using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  class PropertySerializerTrigger : PropertySerializerMecanim {
    public override void SetDynamic(NetworkFrame frame, object value) {
      frame.Storage[Settings.OffsetStorage].TriggerLocal.Update(BoltCore.frame, true);
    }

    public override object GetDebugValue(State state) {
      return "TRIGGER";
    }

    public override int StateBits(State state, NetworkFrame frame) {
      return 32;
    }

    public override bool StatePack(State state, NetworkFrame frame, BoltConnection connection, UdpPacket stream) {
      // adjust send trigger
      frame.Storage[Settings.OffsetStorage].TriggerSend.Update(BoltCore.frame, false);

      // write history into packet
      stream.WriteInt(frame.Storage[Settings.OffsetStorage].TriggerSend.History);
      return true;
    }

    public override void StateRead(State state, NetworkFrame frame, BoltConnection connection, UdpPacket stream) {
      frame.Storage[Settings.OffsetStorage].TriggerLocal.Frame = frame.Number;
      frame.Storage[Settings.OffsetStorage].TriggerLocal.History = stream.ReadInt();
    }

    public override void OnSimulateAfter(State state) {
      var it = state.Frames.GetIterator();

      while (it.Next()) {
        if (InvokeForFrame(state, it.val) == false) {
          break;
        }
      }
    }

    bool MecanimPushOrNone(State state, NetworkFrame f, bool push) {
      var t_frame = f.Storage[Settings.OffsetStorage].TriggerLocal.Frame;
      var t_history = f.Storage[Settings.OffsetStorage].TriggerLocal.History;

      if (t_history != 0) {
        var cb = (System.Action)state.Objects[Settings.OffsetObjects];

        for (int i = 31; i >= 0; --i) {
          if (t_frame - i > state.Entity.Frame) {
            return false;
          }

          int b = 1 << i;
          if ((t_history & b) == b) {
            f.Storage[Settings.OffsetStorage].TriggerLocal.History = t_history = (t_history & ~b);

            // push to send index
            f.Storage[Settings.OffsetStorage].TriggerSend.Update(BoltCore.frame, true);

            // apply to mecanim
            if (push) {
              state.Animator.SetTrigger(Settings.PropertyName);
            }

            // perform callback
            if (cb != null) {
              cb();
            }
          }
        }
      }

      return true;
    }

    bool InvokeForFrame(State state, NetworkFrame f) {
      if (MecanimSettings.Enabled && state.Animator) {
        if (ShouldPullDataFromMecanim(state)) {
          return MecanimPull(state, f);
        }
        else {
          return MecanimPushOrNone(state, f, true);
        }
      }
      else {
        return MecanimPushOrNone(state, f, false);
      }
    }

    bool MecanimPull(State state, NetworkFrame f) {
      if ((state.Animator.GetBool(Settings.PropertyName) == true) && (state.Animator.IsInTransition(MecanimSettings.Layer) == false)) {
        f.Storage[Settings.OffsetStorage].TriggerSend.Update(BoltCore.frame, true);

        var cb = (System.Action)state.Objects[Settings.OffsetObjects];
        if (cb != null) {
          cb();
        }
      }

      return false;
    }
  }
}

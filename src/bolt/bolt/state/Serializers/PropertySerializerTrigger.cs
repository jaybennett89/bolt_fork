﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  class PropertySerializerTrigger : PropertySerializerMecanim {
    int LocalOffset {
      get { return StateData.ByteOffset + 8; }
    }

    int SendOffset {
      get { return StateData.ByteOffset; }
    }

    public PropertySerializerTrigger(StatePropertyMetaData meta)
      : base(meta) {
      Assert.True(meta.ByteLength == 16);
      meta.ByteLength = 8;
    }

    public override object GetDebugValue(State state) {
      return "TRIGGER";
    }

    public override int StateBits(State state, State.Frame frame) {
      return BoltCore.localSendRate * state.Entity.UpdateRate;
    }

    public override bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpStream stream) {
      // shift data so it aligns with our local frame
      state.Frames.first.Data.SetTrigger(BoltCore.frame, SendOffset, false);

      int triggerFrame = frame.Data.ReadI32(SendOffset);
      int triggerBits = frame.Data.ReadI32(SendOffset + 4);

      stream.WriteInt(triggerBits, BoltCore.localSendRate * state.Entity.UpdateRate);
      return true;
    }

    public override void StateRead(State state, State.Frame frame, BoltConnection connection, UdpStream stream) {
      int triggerBits = stream.ReadInt(BoltCore.remoteSendRate * state.Entity.UpdateRate);

      frame.Data.PackI32(LocalOffset, frame.Number);
      frame.Data.PackI32(LocalOffset + 4, triggerBits);
    }

    public override void OnSimulateAfter(State state) {
      var it = state.Frames.GetIterator();

      while (it.Next()) {
        if (InvokeForFrame(state, it.val) == false) {
          break;
        }
      }
    }

    bool MecanimPushOrNone(State state, State.Frame f, bool push) {
      var cb = (System.Action)state.Frames.first.Objects[StateData.ObjectOffset];
      int frame = f.Data.ReadI32(LocalOffset);
      int bits = f.Data.ReadI32(LocalOffset + 4);

      for (int i = 31; i >= 0; --i) {
        if (frame - i > state.Entity.Frame) {
          return false;
        }

        int b = 1 << i;
        if ((bits & b) == b) {
          // clear out bit
          f.Data.PackI32(LocalOffset + 4, bits & ~b);

          // push to send index
          state.Frames.first.Data.SetTrigger(BoltCore.frame, SendOffset, true);

          // apply to mecanim
          if (push) {
            state.Animator.SetTrigger(StateData.PropertyName);
          }

          // perform callback
          if (cb != null) {
            cb();
          }
        }
      }

      return true;
    }

    bool InvokeForFrame(State state, State.Frame f) {
      if (MecanimData.Enabled && state.Animator) {
        switch (GetMecanimDirection(state)) {
          case MecanimDirection.Push:
            return MecanimPushOrNone(state, f, true);

          case MecanimDirection.Pull:
            if ((state.Animator.GetBool(StateData.PropertyName) == true) && (state.Animator.IsInTransition(MecanimData.Layer) == false)) {
              state.Frames.first.Data.SetTrigger(BoltCore.frame, SendOffset, true);

              var cb = (System.Action)state.Frames.first.Objects[StateData.ObjectOffset];

              if (cb != null) {
                cb();
              }
            }
            return false;

          default:
            return MecanimPushOrNone(state, f, false);
        }
      }
      else {
        return MecanimPushOrNone(state, f, false);
      }
    }
  }
}

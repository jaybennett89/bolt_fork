using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  class PropertySerializerTrigger : PropertySerializer {
    int LocalOffset {
      get { return StateData.ByteOffset; }
    }

    int SendOffset {
      get { return StateData.ByteOffset + 8; }
    }

    public PropertySerializerTrigger(StatePropertyMetaData meta)
      : base(meta) {
    }

    public override int StateBits(State state, State.Frame frame) {
      return BoltCore.localSendRate * state.Entity.UpdateRate;
    }

    public override bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpStream stream) {
      int triggerFrame = frame.Data.ReadI32(SendOffset);
      int triggerBits = frame.Data.ReadI32(SendOffset + 4);

      Assert.True(triggerFrame == BoltCore.frame, "{0} == {1}", triggerFrame, BoltCore.frame);

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

      // make sure we always shift our send data according to our local frame
      state.Frames.first.Data.SetTrigger(BoltCore.frame, SendOffset, false);
    }

    bool InvokeForFrame(State state, State.Frame f) {
      var cb = (System.Action)state.Frames.first.Objects[StateData.ObjectOffset];
      var mecanim = state.Animator && StateData.Mecanim;
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
          if (mecanim) {
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
  }
}

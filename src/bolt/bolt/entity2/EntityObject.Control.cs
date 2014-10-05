using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  partial class EntityObject {
    internal void TakeControl() {
      if (IsOwner) {
        if (HasControl) {
          BoltLog.Warn("You already have control of {0}", this);
        }
        else {
          // revoke any existing control
          RevokeControl();

          // take control locally
          TakeControlInternal();
        }
      }
      else {
        BoltLog.Error("Only the owner of {0} can take control of it", this);
      }
    }

    internal void TakeControlInternal() {
      Assert.False(Flags & EntityFlags.HAS_CONTROL);

      Flags |= EntityFlags.HAS_CONTROL;
      CommandQueue.Clear();
      CommandSequence = 0;

      // raise user event
      BoltCallbacksBase.ControlOfEntityGainedInvoke(UnityObject);

      // call to user behaviours
      foreach (IEntityBehaviour eb in Behaviours) {
        eb.ControlGained();
      }
    }

    internal void ReleaseControl() {
      if (IsOwner) {
        if (HasControl) {
          ReleaseControlInternal();
        }
        else {
          BoltLog.Warn("You are not controlling {0}", this);
        }
      }
      else {
        BoltLog.Error("You can not release control of {0}, you are not the owner", this);
      }
    }

    internal void ReleaseControlInternal() {
      Assert.True(Flags & EntityFlags.HAS_CONTROL);

      Flags &= ~EntityFlags.HAS_CONTROL;
      CommandQueue.Clear();
      CommandSequence = 0;

      // call to user behaviours
      foreach (IEntityBehaviour eb in Behaviours) {
        eb.ControlLost();
      }

      // call user event
      BoltCallbacksBase.ControlOfEntityLostInvoke(UnityObject);
    }

    internal void AssignControl(BoltConnection connection) {
      if (IsOwner) {
        if (connection._entityChannel.CreateOnRemote(this)) {
          CommandSequence = 0;
          CommandQueue.Clear();

          Controller = connection;
          Controller._entityChannel.ForceSync(this);

          // clear idle state for the controller
          SetIdle(Controller, false);
        }
        else {
          BoltLog.Error("Could not create {0} on {1}, control not assigned", this, connection);
        }

      }
      else {
        BoltLog.Error("You can not assign control of {0}, you are not the owner", this);
      }
    }

    internal void RevokeControl() {
      if (IsOwner) {
        if (Controller) {
          // force a replication of this
          Controller._entityChannel.ForceSync(this);
          Controller = null;

          // clear out everything
          CommandSequence = 0;
          CommandQueue.Clear();
        }
      }
      else {
        BoltLog.Error("You can not revoke control of {0}, you are not the owner", this);
        return;
      }
    }

    internal bool QueueCommand(Command cmd) {
      if (HasControl) {
        if (CommandQueue.count < BoltCore._config.commandQueueSize) {
          cmd.Frame = BoltCore.serverFrame;
          cmd.Sequence = CommandSequence = UdpMath.SeqNext(CommandSequence, Command.SEQ_MASK);

          CommandQueue.AddLast(cmd);
          return true;
        }
        else {
          BoltLog.Error("Command queue for {0} is full", this);
          return false;
        }
      }
      else {
        BoltLog.Error("You can not queue commands to {0}, you are not the controller", this);
        return false;
      }
    }
  }
}

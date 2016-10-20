using UdpKit;

namespace Bolt {
  partial class Entity {
    internal void TakeControl(IProtocolToken token) {
      if (IsOwner) {
        if (HasControl) {
          BoltLog.Warn("You already have control of {0}", this);
        }
        else {
          // revoke any existing control
          RevokeControl(token);

          // take control locally
          TakeControlInternal(token);

          // de-freeze
          Freeze(false);
        }
      }
      else {
        BoltLog.Error("Only the owner of {0} can take control of it", this);
      }
    }

    internal void TakeControlInternal(IProtocolToken token) {
      Assert.False(Flags & EntityFlags.HAS_CONTROL);

      Flags |= EntityFlags.HAS_CONTROL;

      CommandQueue.Clear();
      CommandSequence = 0;
      CommandLastExecuted = null;

      ControlGainedToken = token;
      ControlLostToken = null;

      // call to serializer
      Serializer.OnControlGained();

      // raise user event
      BoltInternal.GlobalEventListenerBase.ControlOfEntityGainedInvoke(UnityObject);

      // call to user behaviours
      foreach (IEntityBehaviour eb in Behaviours) {
        if (ReferenceEquals(eb.entity, this.UnityObject)) {
          eb.ControlGained();
        }
      }

      Freeze(false);
    }

    internal void ReleaseControl(IProtocolToken token) {
      if (IsOwner) {
        if (HasControl) {
          ReleaseControlInternal(token);

          // un-freeze
          Freeze(false);
        }
        else {
          BoltLog.Warn("You are not controlling {0}", this);
        }
      }
      else {
        BoltLog.Error("You can not release control of {0}, you are not the owner", this);
      }
    }

    internal void ReleaseControlInternal(IProtocolToken token) {
      Assert.True(Flags & EntityFlags.HAS_CONTROL);

      Flags &= ~EntityFlags.HAS_CONTROL;
      CommandQueue.Clear();
      CommandSequence = 0;
      CommandLastExecuted = null;

      ControlLostToken = token;
      ControlGainedToken = null;

      // call to serializer
      Serializer.OnControlLost();

      // call to user behaviours
      foreach (IEntityBehaviour eb in Behaviours) {
        if (ReferenceEquals(eb.entity, this.UnityObject)) {
          eb.ControlLost();
        }
      }

      // call user event
      BoltInternal.GlobalEventListenerBase.ControlOfEntityLostInvoke(UnityObject);

      // de-freeze
      Freeze(false);
    }

    internal void AssignControl(BoltConnection connection, IProtocolToken token) {
      if (IsOwner) {
        if (HasControl) {
          ReleaseControl(token);
        }

        EntityProxy proxy;

        CommandLastExecuted = null;
        CommandSequence = 0;
        CommandQueue.Clear();

        Controller = connection;
        Controller._controlling.Add(this);
        Controller._entityChannel.CreateOnRemote(this, out proxy);
        Controller._entityChannel.ForceSync(this);

        // set token 
        proxy.ControlTokenLost = null;
        proxy.ControlTokenGained = token;

        Freeze(false);
      }
      else {
        BoltLog.Error("You can not assign control of {0}, you are not the owner", this);
      }
    }

    internal void RevokeControl(IProtocolToken token) {
      if (IsOwner) {
        if (Controller) {
          EntityProxy proxy;

          // force a replication of this
          Controller._controlling.Remove(this);
          Controller._entityChannel.ForceSync(this, out proxy);
          Controller = null;

          // clear out everything
          CommandLastExecuted = null;
          CommandSequence = 0;
          CommandQueue.Clear();

          // set token
          if (proxy != null) {
            proxy.ControlTokenLost = token;
            proxy.ControlTokenGained = null;
          }

          Freeze(false);
        }
      }
      else {
        BoltLog.Error("You can not revoke control of {0}, you are not the owner", this);
        return;
      }
    }

    internal bool QueueInput(Command cmd) {
      if (_canQueueCommands) {
        Assert.True(HasControl);

        if (CommandQueue.count < BoltCore._config.commandQueueSize) {
          cmd.ServerFrame = BoltCore.serverFrame;
          cmd.Sequence = CommandSequence = UdpMath.SeqNext(CommandSequence, Command.SEQ_MASK);
        }
        else {
          BoltLog.Error("Input queue for {0} is full", this);
          return false;
        }

        CommandQueue.AddLast(cmd);
        return true;
      }
      else {
        BoltLog.Error("You can only queue commands to in the 'SimulateController' callback");
        return false;
      }
    }
  }
}

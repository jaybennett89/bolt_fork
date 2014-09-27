using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UE = UnityEngine;

namespace Bolt {
  internal class EntityObject : IBoltListNode {
    internal const int COMMAND_SEQ_BITS = 8;
    internal const int COMMAND_SEQ_SHIFT = 16 - COMMAND_SEQ_BITS;
    internal const int COMMAND_SEQ_MASK = (1 << COMMAND_SEQ_BITS) - 1;

    static int _instanceIdCounter;

    internal PrefabId PrefabId;
    internal InstanceId InstanceId;
    internal EntityFlags Flags;

    internal UE.Vector3 SpawnPosition;
    internal UE.Quaternion SpawnRotation;

    internal BoltEntity UserToken;
    internal BoltConnection Source;
    internal BoltConnection Controller;

    internal IEntitySerializer Serializer;
    internal IEntityBehaviour[] Behaviours;

    internal int UpdateRate;
    internal bool ClientPrediction;
    internal ushort CommandSequence = 0;

    internal BoltEventDispatcher EventDispatcher = new BoltEventDispatcher();
    internal BoltDoubleList<BoltCommand> CommandQueue = new BoltDoubleList<BoltCommand>();

    internal int SendRate {
      get {
        if (IsOwner) {
          return UpdateRate * BoltCore.localSendRate;
        }
        else {
          return UpdateRate * BoltCore.remoteSendRate;
        }
      }
    }

    internal bool IsOwner {
      get { return ReferenceEquals(Source, null); }
    }

    internal bool HasControl {
      get { return Flags & EntityFlags.HAS_CONTROL; }
    }

    public bool PersistsOnSceneLoad {
      get { return Flags & EntityFlags.PERSIST_ON_LOAD; }
    }

    object IBoltListNode.prev { get; set; }
    object IBoltListNode.next { get; set; }
    object IBoltListNode.list { get; set; }

    internal void Attach() {
      Assert.NotNull(UserToken);
      Assert.True(InstanceId.Value == 0);

      // set instance id
      InstanceId = new InstanceId(++_instanceIdCounter);

      // add to entities list
      BoltCore._entities.AddLast(this);

      // call out to user
      BoltCallbacksBase.EntityAttachedInvoke(this.UserToken);

      // call out to behaviours
      foreach (IEntityBehaviour eb in Behaviours) {
        eb.Attached();
      }

      // log
      BoltLog.Debug("Attached {0}", this);

      // create on all connections
      var it = BoltCore._connections.GetIterator();

      while (it.Next()) {
        it.val._entityChannel.CreateOnRemote(this);
      }
    }

    internal void Detach() {
      Assert.NotNull(UserToken);
      Assert.True(InstanceId.Value != 0);

      // destroy on all connections
      var it = BoltCore._connections.GetIterator();

      while (it.Next()) {
        it.val._entityChannel.DestroyOnRemote(this, BoltEntityDestroyMode.LocalDestroy);
      }

      // call out to behaviours
      foreach (IEntityBehaviour eb in Behaviours) {
        eb.Detached();
      }

      // call out to user
      BoltCallbacksBase.EntityDetachedInvoke(this.UserToken);

      // remove from entities list
      BoltCore._entities.Remove(this);

      // log
      BoltLog.Debug("Detached {0}", this);
    }

    internal void TakeControl() {
      if (!IsOwner) {
        BoltLog.Error("can't take control of {0}, it is proxied", this);
        return;
      }

      // revoke any existing control
      RevokeControl();

      // take control locally
      TakeControlInternal();
    }

    internal bool IsController(BoltConnection connection) {
      return connection != null && Controller != null && ReferenceEquals(Controller, connection);
    }

    internal void Render() {
      throw new NotImplementedException();
    }

    internal void OnPrepareSend(EntityProxy entityProxy) {
      throw new NotImplementedException();
    }

    internal void TakeControlInternal() {
      Assert.True(!(Flags & EntityFlags.HAS_CONTROL));

      CommandQueue.Clear();
      CommandSequence = 0;

      // raise user event
      BoltCallbacksBase.ControlOfEntityGainedInvoke(UserToken);

      // call to user behaviours
      foreach (IEntityBehaviour eb in Behaviours) {
        eb.ControlGained();
      }
    }

    internal void ReleaseControl() {
      Assert.True(Flags & EntityFlags.HAS_CONTROL);

      Flags &= ~EntityFlags.HAS_CONTROL;
      CommandQueue.Clear();
      CommandSequence = 0;

      // call to user behaviours
      foreach (IEntityBehaviour eb in Behaviours) {
        eb.ControlLost();
      }

      // call user event
      BoltCallbacksBase.ControlOfEntityLostInvoke(UserToken);
    }

    internal void GiveControl(BoltConnection cn) {
      throw new NotImplementedException();
      //if (_flags & BoltEntity.FLAG_IS_PROXY) {
      //  BoltLog.Error("can't give control of {0} to {1}, it is proxied", this, cn);
      //  return;
      //}

      //if (cn._entityChannel.CreateOnRemote(this)) {
      //  _flags |= BoltEntity.FLAG_REMOTE_CONTROLLED;
      //  _commands.Clear();
      //  _commandSequence = 0;
      //  _remoteController = cn;
      //  _remoteController._entityChannel.ForceSync(this);

      //}
      //else {
      //  BoltLog.Error("couldn't create {0} on {1}, control not given", this, cn);
      //}
    }

    internal void RevokeControl() {
      throw new NotImplementedException();
      //if (_flags & BoltEntity.FLAG_IS_PROXY) {
      //  BoltLog.Error("can't revoke control of {0}, it is proxied", this);
      //  return;
      //}

      //if (_remoteController) {
      //  BoltConnection cn = _remoteController;

      //  _flags &= ~BoltEntity.FLAG_REMOTE_CONTROLLED;
      //  _commands.Clear();
      //  _commandSequence = 0;
      //  _remoteController = null;

      //  cn._entityChannel.ForceSync(this);
      //}
    }

    internal bool QueueCommand(BoltCommand cmd) {
      throw new NotImplementedException();
      //if (hasControl == false) {
      //  BoltLog.Error("queue of {0} to {1} failed, you are not controlling this entity", cmd, this);
      //  return false;
      //}

      //if (_commands.count > BoltCore._config.commandQueueSize) {
      //  return false;
      //}

      //cmd._serverFrame = BoltCore.serverFrame;
      //cmd._sequence = _commandSequence = UdpMath.SeqNext(_commandSequence, COMMAND_SEQ_MASK);

      //// put on command buffer
      //_commands.AddLast(cmd);

      //return true;
    }

    internal void SetIdle(BoltConnection cn, bool idle) {
      if (ReferenceEquals(cn, Controller)) {
        BoltLog.Error("can't idle an entity on the connection which is controlling it");
        return;
      }

      cn._entityChannel.SetIdle(this, idle);
    }

    internal void Raise(IBoltEvent ev) {
      throw new NotImplementedException();
      //BoltEventBase evnt = (BoltEventBase)ev;

      //if (evnt._isEntityEvent == false) {
      //  throw new BoltException("you can't send global events to entities"); ;
      //}

      //if (evnt._deliveryMode == BoltEventDeliveryMode.Reliable) {
      //  throw new BoltException("you can't send reliable events to entities");
      //}

      //evnt._entity = this;
      //BoltEventBase.Invoke(evnt);
    }

    internal void Simulate() {
      BoltCommand cmd;
      BoltIterator<BoltCommand> itr;

      Serializer.OnSimulateBefore();

      if (IsOwner) {
        foreach (IEntityBehaviour eb in Behaviours) {
          eb.SimulateOwner();
        }
      }

      if (HasControl) {
        Assert.Null(Controller);

        // execute all old commands (in order)
        cmd = null;
        itr = CommandQueue.GetIterator();

        while (itr.Next(out cmd)) {
          Assert.True(cmd._hasExecuted);

          // exec old command
          ExecuteCommand(cmd, ReferenceEquals(cmd, CommandQueue.first));
        }

        foreach (IEntityBehaviour eb in Behaviours) {
          eb.SimulateController();
        }

        // execute all new commands (in order)
        cmd = null;
        itr = CommandQueue.GetIterator();

        while (itr.Next(out cmd)) {
          if (cmd._hasExecuted == false) {
            ExecuteCommand(cmd, false);
            Assert.True(cmd._hasExecuted);
          }
        }

        // if this is a local entity we are controlling
        // we should dispose all commands except one
        if (IsOwner) {
          while (CommandQueue.count > 0) {
            CommandQueue.RemoveFirst().Dispose();
          }
        }
      }
      else {
        if (Controller != null) {
          Assert.True(IsOwner);

          do {
            cmd = null;
            itr = CommandQueue.GetIterator();

            while (itr.Next(out cmd)) {
              if (cmd._hasExecuted == false) {
                try {
                  ExecuteCommand(cmd, false);
                  break;

                }
                finally {
                  cmd._stateSent = false;
                }
              }
            }
          } while (UnexecutedCommandCount() > BoltCore._config.commandDejitterDelay);
        }
      }

      Serializer.OnSimulateAfter();
    }

    void ExecuteCommand(BoltCommand cmd, bool resetState) {
      try {
        foreach (IEntityBehaviour eb in Behaviours) {
          eb.ExecuteCommand(cmd, resetState);
        }
      }
      finally {
        cmd._hasExecuted = true;
      }
    }

    int UnexecutedCommandCount() {
      int count = 0;
      var it = CommandQueue.GetIterator();

      while (it.Next()) {
        if (it.val._hasExecuted == false) {
          count += 1;
        }
      }

      return count;
    }


    internal static EntityObject CreateFrom(BoltEntity entity, TypeId serializerId) {
      EntityObject en;

      en = new EntityObject();
      en.PrefabId = new PrefabId(entity._prefabId);
      en.UpdateRate = entity._updateRate;
      en.Serializer = BoltFactory.CreateSerializer(serializerId);
      en.ClientPrediction = entity._clientPredicted;
      en.Flags =
        entity._persistanceMode == BoltEntityPersistanceMode.PersistOnLoad
        ? EntityFlags.PERSIST_ON_LOAD
        : EntityFlags.ZERO;

      return en;
    }

    public static implicit operator bool(EntityObject entity) {
      return entity != null;
    }


  }
}

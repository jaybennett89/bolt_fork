using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  partial class EntityObject : IBoltListNode {
    internal const int COMMAND_SEQ_BITS = 8;
    internal const int COMMAND_SEQ_SHIFT = 16 - COMMAND_SEQ_BITS;
    internal const int COMMAND_SEQ_MASK = (1 << COMMAND_SEQ_BITS) - 1;

    static int _instanceIdCounter;

    internal PrefabId PrefabId;
    internal InstanceId InstanceId;
    internal EntityFlags Flags;

    internal UE.Vector3 SpawnPosition;
    internal UE.Quaternion SpawnRotation;

    internal BoltEntity UnityObject;
    internal BoltConnection Source;
    internal BoltConnection Controller;

    internal IEntitySerializer Serializer;
    internal IEntityBehaviour[] Behaviours;

    internal int UpdateRate;
    internal bool ControllerLocalPrediction;
    internal ushort CommandSequence = 0;

    internal BoltEventDispatcher EventDispatcher = new BoltEventDispatcher();
    internal BoltDoubleList<BoltCommand> CommandQueue = new BoltDoubleList<BoltCommand>();
    internal BoltDoubleList<EntityProxy> Proxies = new BoltDoubleList<EntityProxy>();

    internal int Frame {
      get {
        if (IsOwner) {
          return BoltCore.frame;
        }

        if (HasControl && ControllerLocalPrediction) {
          return BoltCore.frame;
        }

        return Source.remoteFrame;
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

    public override string ToString() {
      return string.Format("[Entity {0} {1} {2}]", InstanceId, PrefabId, Serializer);
    }

    internal EntityProxy CreateProxy() {
      EntityProxy p;

      p = new EntityProxy();
      p.Entity = this;
      p.Mask = Serializer.GetDefaultMask();

      // add to list
      Proxies.AddLast(p);

      // let serializer init
      Serializer.InitProxy(p);

      return p;
    }

    internal void Attach() {
      Assert.NotNull(UnityObject);
      Assert.True(InstanceId.Value != 0);

      // add to entities list
      BoltCore._entities.AddLast(this);

      // call out to user
      BoltCallbacksBase.EntityAttachedInvoke(this.UnityObject);

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
      Assert.NotNull(UnityObject);
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
      BoltCallbacksBase.EntityDetachedInvoke(this.UnityObject);

      // remove from entities list
      BoltCore._entities.Remove(this);

      // clear from unity object
      UnityObject.Entity = null;

      // log
      BoltLog.Debug("Detached {0}", this);
    }

    internal void AddEventListener(UE.MonoBehaviour behaviour) {
      EventDispatcher.Add(behaviour);
    }

    internal void RemoveEventListener(UE.MonoBehaviour behaviour) {
      EventDispatcher.Remove(behaviour);
    }

    internal bool IsController(BoltConnection connection) {
      return connection != null && Controller != null && ReferenceEquals(Controller, connection);
    }

    internal void Render() {
      Serializer.OnRender();
    }

    internal void Initialize() {
      Assert.True(InstanceId.Value == 0);

      // grab all behaviours
      Behaviours = UnityObject.GetComponentsInChildren(typeof(IEntityBehaviour)).Select(x => x as IEntityBehaviour).Where(x => x != null).ToArray();

      // set instance id
      InstanceId = new InstanceId(++_instanceIdCounter);

      // assign usertokens
      UnityObject.Entity = this;

      // call into serializer
      Serializer.OnInitialized();
    }


    internal void SetIdle(BoltConnection connection, bool idle) {
      if (IsController(connection)) {
        BoltLog.Error("Can't idle {0} on {1}, it is the controller for this entity", this, connection);
        return;
      }

      connection._entityChannel.SetIdle(this, idle);
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
      Serializer.OnSimulateBefore();

      BoltCommand cmd;
      BoltIterator<BoltCommand> itr;

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
      EntityObject eo;

      eo = new EntityObject();
      eo.PrefabId = new PrefabId(entity._prefabId);
      eo.UpdateRate = entity._updateRate;
      eo.ControllerLocalPrediction = entity._clientPredicted;
      eo.Flags = entity._persistThroughSceneLoads ? EntityFlags.PERSIST_ON_LOAD : EntityFlags.ZERO;

      // create serializer
      eo.Serializer = BoltFactory.NewSerializer(serializerId);
      eo.Serializer.OnCreated(eo);

      return eo;
    }

    public static implicit operator bool(EntityObject entity) {
      return entity != null;
    }

  }
}

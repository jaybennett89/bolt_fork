using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  partial class Entity : IBoltListNode, IPriorityCalculator {
    static int _instanceIdCounter;

    bool _canQueueCommands = false;

    internal UniqueId UniqueId;
    internal PrefabId PrefabId;
    internal InstanceId InstanceId;
    internal EntityFlags Flags;

    internal UE.Vector3 SpawnPosition;
    internal UE.Quaternion SpawnRotation;

    internal Entity Parent;
    internal BoltEntity UnityObject;
    internal BoltConnection Source;
    internal BoltConnection Controller;

    internal IEntitySerializer Serializer;
    internal IEntityBehaviour[] Behaviours;
    internal Bolt.IPriorityCalculator PriorityCalculator;

    internal int UpdateRate;
    internal ushort CommandSequence = 0;
    internal Command CommandLastExecuted = null;

    internal EventDispatcher EventDispatcher = new EventDispatcher();
    internal BoltDoubleList<Command> CommandQueue = new BoltDoubleList<Command>();
    internal BoltDoubleList<EntityProxy> Proxies = new BoltDoubleList<EntityProxy>();

    internal int Frame {
      get {
        if (IsOwner) {
          return BoltCore.frame;
        }

        if (HasPredictedControl) {
          return BoltCore.frame;
        }

        return Source.remoteFrame;
      }
    }

    internal bool IsSceneObject {
      get { return Flags & EntityFlags.SCENE_OBJECT; }
    }

    internal bool HasParent {
      get { return Parent != null && Parent.IsAttached; }
    }

    internal bool IsAttached {
      get { return Flags & EntityFlags.ATTACHED; }
    }

    internal bool IsOwner {
      get { return ReferenceEquals(Source, null); }
    }

    internal bool HasControl {
      get { return Flags & EntityFlags.HAS_CONTROL; }
    }

    internal bool HasPredictedControl {
      get { return HasControl && (Flags & EntityFlags.CONTROLLER_LOCAL_PREDICTION); }
    }

    public bool PersistsOnSceneLoad {
      get { return Flags & EntityFlags.PERSIST_ON_LOAD; }
    }

    internal bool CanQueueCommands {
      get { return _canQueueCommands; }
    }

    object IBoltListNode.prev { get; set; }
    object IBoltListNode.next { get; set; }
    object IBoltListNode.list { get; set; }

    public override string ToString() {
      return string.Format("[Entity {0} {1} {2}]", InstanceId, PrefabId, Serializer);
    }

    internal void SetParent(Entity entity) {
      if (IsOwner || HasPredictedControl) {
        SetParentInternal(entity);
      }
      else {
        BoltLog.Error("You are not allowed to assign the parent of this entity, only the owner or a controller with local prediction can");
      }
    }

    internal void SetParentInternal(Entity entity) {
      if (entity != Parent) {
        if ((entity != null) && (entity.IsAttached == false)) {
          BoltLog.Error("You can't assign a detached entity as the parent of another entity");
          return;
        }

        try {
          // notify serializer
          Serializer.OnParentChanging(entity, Parent);
        }
        finally {
          // set parent
          Parent = entity;
        }
      }
    }

    internal void SetUniqueId(UniqueId id) {
      if (IsOwner) {
        if (Proxies.count == 0) {
          if (UniqueId.IsNone) {
            UniqueId = id;
          }
          else {
            BoltLog.Error("You can not change the UniqueId of {0} after it has been set.", this);
          }
        }
        else {
          BoltLog.Error("You can not set UniqueId of {0} after it has been replicated to other computers.", this);
        }
      }
      else {
        BoltLog.Error("You can not set UniqueId of {0}, you are not the owner of this entity.", this);
      }
    }

    internal void SetScopeAll(bool inScope) {
      var it = BoltCore._connections.GetIterator();

      while (it.Next()) {
        SetScope(it.val, inScope);
      }
    }

    internal void SetScope(BoltConnection connection, bool inScope) {
      connection._entityChannel.SetScope(this, inScope);
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
      Assert.False(IsAttached);
      Assert.True(InstanceId.Value != 0);

      // add to entities list
      BoltCore._entities.AddLast(this);

      // mark as attached
      Flags |= EntityFlags.ATTACHED;

      // call out to user
      BoltInternal.GlobalEventListenerBase.EntityAttachedInvoke(this.UnityObject);

      // call out to behaviours
      foreach (IEntityBehaviour eb in Behaviours) {
        eb.Attached();
      }

      // log
      BoltLog.Debug("Attached {0}", this);
    }

    internal void Detach() {
      Assert.NotNull(UnityObject);
      Assert.True(IsAttached);
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
      BoltInternal.GlobalEventListenerBase.EntityDetachedInvoke(this.UnityObject);

      // clear out attached flag
      Flags &= ~EntityFlags.ATTACHED;

      // remove from entities list
      BoltCore._entities.Remove(this);

      // clear from unity object
      UnityObject._entity = null;

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
      PriorityCalculator = UnityObject.GetComponentInChildren(typeof(IPriorityCalculator)) as IPriorityCalculator;

      // use the default priority calculator if none is available
      if (PriorityCalculator == null) {
        PriorityCalculator = this;
      }

      // set instance id
      InstanceId = new InstanceId(++_instanceIdCounter);

      // assign usertokens
      UnityObject._entity = this;

      // call into serializer
      Serializer.OnInitialized();

      // call to behaviours (this happens BEFORE attached)
      foreach (IEntityBehaviour eb in Behaviours) {
        eb.Initialized();
      }
    }

    internal void SetIdle(BoltConnection connection, bool idle) {
      if (idle && IsController(connection)) {
        BoltLog.Error("You can not idle {0} on {1}, as it is the controller for this entity", this, connection);
        return;
      }

      connection._entityChannel.SetIdle(this, idle);
    }

    internal void Simulate() {
      Serializer.OnSimulateBefore();

      BoltIterator<Command> it;

      if (IsOwner) {
        foreach (IEntityBehaviour eb in Behaviours) {
          eb.SimulateOwner();
        }
      }

      if (HasControl) {
        Assert.Null(Controller);

        // execute all old commands (in order)
        it = CommandQueue.GetIterator();

        while (it.Next()) {
          Assert.True(it.val.Flags & CommandFlags.HAS_EXECUTED);

          var resetState = ReferenceEquals(it.val, CommandQueue.first);
          if (resetState) {
            it.val.SmoothCorrection();
          }

          // exec old command
          ExecuteCommand(it.val, resetState);
        }

        try {
          _canQueueCommands = true;

          foreach (IEntityBehaviour eb in Behaviours) {
            eb.SimulateController();
          }
        }
        finally {
          _canQueueCommands = false;
        }

        // execute all new commands (in order)
        it = CommandQueue.GetIterator();

        while (it.Next()) {
          if (it.val.Flags & CommandFlags.HAS_EXECUTED) {
            continue;
          }

          ExecuteCommand(it.val, false);
        }

        // if this is a local entity we are controlling
        // we should dispose all commands except one
        if (IsOwner) {
          while (CommandQueue.count > 0) {
            CommandQueue.RemoveFirst().Free();
          }
        }
      }
      else {
        if (Controller != null) {
          int commands = ExecuteCommandsFromRemote();
          if (commands == 0) {
            if (CommandQueue.count > 0) {
              MissingCommand(CommandQueue.last);
            }
            else {
              MissingCommand(null);
            }

            ExecuteCommandsFromRemote();
          }
        }
      }

      Serializer.OnSimulateAfter();
    }

    void MissingCommand(Command previous) {
      try {
        _canQueueCommands = true;

        for (int i = 0; i < Behaviours.Length; ++i) {
          Behaviours[i].MissingCommand(previous);
        }
      }
      finally {
        _canQueueCommands = false;
      }
    }

    int ExecuteCommandsFromRemote() {
      int commandExecuted = 0;

      Assert.True(IsOwner);

      do {
        var it = CommandQueue.GetIterator();

        while (it.Next()) {
          if (it.val.Flags & CommandFlags.HAS_EXECUTED) {
            continue;
          }

          try {
            ExecuteCommand(it.val, false);
            commandExecuted += 1;
            break;
          }
          finally {
            it.val.Flags |= CommandFlags.SEND_STATE;
          }
        }
      } while (UnexecutedCommandCount() > BoltCore._config.commandDejitterDelay);

      return commandExecuted;
    }

    void ExecuteCommand(Command cmd, bool resetState) {
      try {
        foreach (IEntityBehaviour eb in Behaviours) {
          eb.ExecuteCommand(cmd, resetState);
        }
      }
      finally {
        cmd.Flags |= CommandFlags.HAS_EXECUTED;
      }
    }

    int UnexecutedCommandCount() {
      int count = 0;
      var it = CommandQueue.GetIterator();

      while (it.Next()) {
        if (it.val.IsFirstExecution) {
          count += 1;
        }
      }

      return count;
    }

    internal static Entity CreateFor(PrefabId prefabId, TypeId serializerId, UE.Vector3 position, UE.Quaternion rotation) {
      return CreateFor(BoltCore.PrefabPool.Instantiate(prefabId, position, rotation), prefabId, serializerId);
    }

    internal static Entity CreateFor(UE.GameObject instance, PrefabId prefabId, TypeId serializerId) {
      return CreateFor(instance, prefabId, serializerId, EntityFlags.ZERO);
    }

    internal static Entity CreateFor(UE.GameObject instance, PrefabId prefabId, TypeId serializerId, EntityFlags flags) {
      Entity eo;

      eo = new Entity();
      eo.UnityObject = instance.GetComponent<BoltEntity>();
      eo.PrefabId = prefabId;
      eo.UpdateRate = eo.UnityObject._updateRate;
      eo.Flags = flags;

      if (eo.UnityObject._persistThroughSceneLoads) { eo.Flags |= EntityFlags.PERSIST_ON_LOAD; }
      if (eo.UnityObject._clientPredicted) { eo.Flags |= EntityFlags.CONTROLLER_LOCAL_PREDICTION; }

      // create serializer
      eo.Serializer = Factory.NewSerializer(serializerId);
      eo.Serializer.OnCreated(eo);


      // done
      return eo;
    }

    public static implicit operator bool(Entity entity) {
      return entity != null;
    }

    public static bool operator ==(Entity a, Entity b) {
      return ReferenceEquals(a, b);
    }

    public static bool operator !=(Entity a, Entity b) {
      return ReferenceEquals(a, b) == false;
    }

    float IPriorityCalculator.CalculateStatePriority(BoltConnection connection, BitArray mask, int skipped) {
      return skipped;
    }

    float IPriorityCalculator.CalculateEventPriority(BoltConnection connection, Event evnt) {
      if (HasControl) {
        return 3;
      }

      if (IsController(connection)) {
        return 2;
      }

      return 1;
    }
  }
}

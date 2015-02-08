using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {

  public delegate void CommandCallback(Command created, Command current);

  enum CommandCallbackModes {
    InvokeOnce,
    InvokeRepeating
  }

  struct CommandCallbackItem {
    public int End;
    public int Start;
    public Command Command;
    public CommandCallback Callback;
    public CommandCallbackModes Mode;
  }

  partial class Entity : IBoltListNode, IPriorityCalculator {
    bool _canQueueCommands = false;
    bool _canQueueCallbacks = false;

    internal UniqueId SceneId;
    internal NetworkId NetworkId;
    internal PrefabId PrefabId;
    internal EntityFlags Flags;

    internal UE.Vector3 SpawnPosition;
    internal UE.Quaternion SpawnRotation;

    internal Entity Parent;
    internal BoltEntity UnityObject;
    internal BoltConnection Source;
    internal BoltConnection Controller;

    internal IProtocolToken DetachToken;
    internal IProtocolToken AttachToken;

    internal IEntitySerializer Serializer;
    internal IEntityBehaviour[] Behaviours;
    internal Bolt.IPriorityCalculator PriorityCalculator;

    internal int UpdateRate;
    internal int LastFrameReceived;
    internal int AutoFreezeProxyFrames;
    internal bool CanFreeze = true;
    internal Command CommandLastExecuted = null;
    internal ushort CommandSequence = 0;


    internal EventDispatcher EventDispatcher = new EventDispatcher();
    internal BoltDoubleList<Command> CommandQueue = new BoltDoubleList<Command>();
    internal List<CommandCallbackItem> CommandCallbacks = new List<CommandCallbackItem>();
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

    internal bool IsSceneObject {
      get { return Flags & EntityFlags.SCENE_OBJECT; }
    }

    internal bool HasParent {
      get { return Parent != null && Parent.IsAttached; }
    }

    internal bool IsAttached {
      get { return Flags & EntityFlags.ATTACHED; }
    }

    internal bool IsOwner;

    internal bool IsDummy {
      get { return !IsOwner && !HasPredictedControl; }
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

    public bool IsFrozen {
      get { return BoltCore._entitiesFrozen.Contains(this); }
    }

    object IBoltListNode.prev { get; set; }
    object IBoltListNode.next { get; set; }
    object IBoltListNode.list { get; set; }

    public override string ToString() {
      return string.Format("[Entity {0} {1}]", NetworkId, Serializer);
    }

    public override bool Equals(object obj) {
      return ReferenceEquals(this, obj);
    }

    public override int GetHashCode() {
      return NetworkId.GetHashCode();
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

    internal void SetScopeAll(bool inScope) {
      var it = BoltCore._connections.GetIterator();

      while (it.Next()) {
        SetScope(it.val, inScope);
      }
    }

    internal void SetScope(BoltConnection connection, bool inScope) {
      connection._entityChannel.SetScope(this, inScope);
    }

    internal void Freeze(bool freeze) {
      if (freeze) {
        if (CanFreeze) {
          if (BoltCore._entities.Contains(this)) {
            BoltCore._entities.Remove(this);
            BoltCore._entitiesFrozen.AddLast(this);
            BoltLog.Debug("FROZEN: {0}", this);
          }
        }
      }
      else {
        if (BoltCore._entitiesFrozen.Contains(this)) {
          BoltCore._entitiesFrozen.Remove(this);
          BoltCore._entities.AddLast(this);
          BoltLog.Debug("THAWED: {0}", this);
        }
      }
    }

    internal EntityProxy CreateProxy() {
      EntityProxy p;

      p = new EntityProxy();
      p.Entity = this;
      p.Combine(Serializer.GetDefaultMask());

      // add to list
      Proxies.AddLast(p);

      // let serializer init
      Serializer.InitProxy(p);

      return p;
    }

    internal void Attach() {
      Assert.NotNull(UnityObject);
      Assert.False(IsAttached);
      Assert.True((NetworkId.Packed == 0UL) || (Source != null));

      // mark as don't destroy on load
      UE.GameObject.DontDestroyOnLoad(UnityObject.gameObject);

      // assign network id
      if (Source == null) {
        NetworkId = NetworkIdAllocator.Allocate();
      }

      // add to entities list
      BoltCore._entities.AddLast(this);

      // mark as attached
      Flags |= EntityFlags.ATTACHED;

      // call out to behaviours
      foreach (IEntityBehaviour eb in Behaviours) {
        try {
          eb.Attached();
        }
        catch (Exception exn) {
          BoltLog.Error("User code threw exception inside Attached callback");
          BoltLog.Exception(exn);
        }
      }

      // call out to user
      try {
        BoltInternal.GlobalEventListenerBase.EntityAttachedInvoke(this.UnityObject);
      }
      catch (Exception exn) {
        BoltLog.Error("User code threw exception inside Attached callback");
        BoltLog.Exception(exn);
      }

      if (AttachToken != null) {
        // call out to behaviours
        foreach (IEntityBehaviour eb in Behaviours) {
          try {
            eb.Attached(AttachToken);
          }
          catch (Exception exn) {
            BoltLog.Error("User code threw exception inside Attached callback");
            BoltLog.Exception(exn);
          }
        }

        // call out to user
        try {
          BoltInternal.GlobalEventListenerBase.EntityAttachedInvoke(this.UnityObject, AttachToken);
        }
        catch (Exception exn) {
          BoltLog.Error("User code threw exception inside Attached callback");
          BoltLog.Exception(exn);
        }
      }

      // log
      BoltLog.Debug("Attached {0}", this);
    }

    internal void Detach() {
      Assert.NotNull(UnityObject);
      Assert.True(IsAttached);
      Assert.True(NetworkId.Packed != 0UL);

      // destroy on all connections
      var it = BoltCore._connections.GetIterator();

      while (it.Next()) {
        it.val._entityChannel.DestroyOnRemote(this);
      }

      // call out to behaviours
      foreach (IEntityBehaviour eb in Behaviours) {
        try {
          eb.Detached();
          eb.entity = null;
        }
        catch (Exception exn) {
          BoltLog.Error("User code threw exception inside Detach callback");
          BoltLog.Exception(exn);
        }
      }

      // call out to user
      try {
        BoltInternal.GlobalEventListenerBase.EntityDetachedInvoke(this.UnityObject);
      }
      catch (Exception exn) {
        BoltLog.Error("User code threw exception inside Detach callback");
        BoltLog.Exception(exn);
      }

      if (DetachToken != null) {
        // call out to behaviours
        foreach (IEntityBehaviour eb in Behaviours) {
          try {
            eb.Detached(DetachToken);
            eb.entity = null;
          }
          catch (Exception exn) {
            BoltLog.Error("User code threw exception inside Detach callback");
            BoltLog.Exception(exn);
          }
        }

        // call out to user
        try {
          BoltInternal.GlobalEventListenerBase.EntityDetachedInvoke(this.UnityObject, DetachToken);
        }
        catch (Exception exn) {
          BoltLog.Error("User code threw exception inside Detach callback");
          BoltLog.Exception(exn);
        }
      }

      // clear out attached flag
      Flags &= ~EntityFlags.ATTACHED;

      // remove from entities list
      if (IsFrozen) {
        BoltCore._entitiesFrozen.Remove(this);
      }
      else {
        BoltCore._entities.Remove(this);
      }

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
      IsOwner = ReferenceEquals(Source, null);

      // make this a bit faster
      Behaviours = UnityObject.GetComponentsInChildren(typeof(IEntityBehaviour)).Select(x => x as IEntityBehaviour).Where(x => x != null).ToArray();

      // try to find a priority calculator
      PriorityCalculator = UnityObject.GetComponentInChildren(typeof(IPriorityCalculator)) as IPriorityCalculator;

      // use the default priority calculator if none is available
      if (PriorityCalculator == null) {
        PriorityCalculator = this;
      }
      else {
        BoltLog.Debug("Using Priority Calculator {0} for {1}", PriorityCalculator.GetType(), UnityObject.gameObject.name);
      }

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

    void RemoveOldCommandCallbacks(int number) {
      for (int i = 0; i < CommandCallbacks.Count; ++i) {
        if (CommandCallbacks[i].End < number) {
          // remove this index
          CommandCallbacks.RemoveAt(i);

          // 
          --i;
        }
      }
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
        // we should dispose all commands (there is no need to store them)
        if (IsOwner) {
          while (CommandQueue.count > 0) {
            CommandQueue.RemoveFirst();
          }

          //RemoveOldCommandCallbacks(CommandSequence);
        }
        else {
          //if (CommandQueue.count > 0) {
          //  RemoveOldCommandCallbacks(CommandQueue.first.Sequence);
          //}
        }
      }
      else {
        if (Controller != null) {
          //if (CommandQueue.count > 0) {
          //  RemoveOldCommandCallbacks(CommandQueue.first.Sequence);
          //}

          if (ExecuteCommandsFromRemote() == 0) {
            Command cmd = CommandQueue.lastOrDefault;

            for (int i = 0; i < Behaviours.Length; ++i) {
              Behaviours[i].MissingCommand(cmd);
            }
          }
        }
      }

      Serializer.OnSimulateAfter();
    }

    int ExecuteCommandsFromRemote() {
      int commandsExecuted = 0;

      Assert.True(IsOwner);

      do {
        var it = CommandQueue.GetIterator();

        while (it.Next()) {
          if (it.val.Flags & CommandFlags.HAS_EXECUTED) {
            continue;
          }

          try {
            ExecuteCommand(it.val, false);
            commandsExecuted += 1;
            break;
          }
          finally {
            it.val.Flags |= CommandFlags.SEND_STATE;
          }
        }
      } while (UnexecutedCommandCount() > BoltCore._config.commandDejitterDelay);

      return commandsExecuted;
    }

    //void ExecuteCommandCallback(CommandCallbackItem cb, Command cmd) {
    //  try {
    //    cb.Callback(cb.Command, cmd);
    //  }
    //  catch (Exception exn) {
    //    BoltLog.Exception(exn);
    //  }
    //}

    void ExecuteCommand(Command cmd, bool resetState) {
      try {
        // execute all command callbacks
        //for (int i = 0; i < CommandCallbacks.Count; ++i) {
        //  var cb = CommandCallbacks[i];

        //  switch (cb.Mode) {
        //    case CommandCallbackModes.InvokeOnce:
        //      if (cmd.Sequence == cb.End) {
        //        ExecuteCommandCallback(cb, cmd);
        //      }
        //      break;

        //    case CommandCallbackModes.InvokeRepeating:
        //      if (cmd.Sequence >= cb.Start && cmd.Sequence <= cb.End) {
        //        ExecuteCommandCallback(cb, cmd);
        //      }
        //      break;
        //  }
        //}

        _canQueueCallbacks = cmd.IsFirstExecution;

        foreach (IEntityBehaviour eb in Behaviours) {
          eb.ExecuteCommand(cmd, resetState);
        }
      }
      finally {
        // flag this so it can't queue more callbacks
        _canQueueCallbacks = false;

        // flag this as executed
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

    internal void InvokeOnce(Command command, CommandCallback callback, int delay) {
      Assert.True(delay > 0);

      if (!_canQueueCallbacks) {
        BoltLog.Error("Can only queue callbacks when commands with 'IsFirstExecution' set to true are executing");
        return;
      }

      //CommandCallbacks.Add(new CommandCallbackItem { Command = command, Callback = callback, Start = -1, End = command.Number + delay, Mode = CommandCallbackModes.InvokeOnce });
    }

    internal void InvokeRepeating(Command command, CommandCallback callback, int period) {
      Assert.True(period > 0);

      if (!_canQueueCallbacks) {
        BoltLog.Error("Can only queue callbacks when commands with 'IsFirstExecution' set to true are executing");
        return;
      }

      //CommandCallbacks.Add(new CommandCallbackItem { Command = command, Callback = callback, Start = command.Number + 1, End = command.Number + period, Mode = CommandCallbackModes.InvokeRepeating });
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
      eo.UpdateRate = eo.UnityObject._updateRate;
      eo.AutoFreezeProxyFrames = eo.UnityObject._autoFreezeProxyFrames;
      eo.PrefabId = prefabId;
      eo.Flags = flags;

      if (prefabId.Value == 0) {
        eo.Flags |= EntityFlags.SCENE_OBJECT;
        eo.SceneId = eo.UnityObject.sceneGuid;
      }

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

    bool IPriorityCalculator.Always {
      get { return false; }
    }

    float IPriorityCalculator.CalculateStatePriority(BoltConnection connection, int skipped) {
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

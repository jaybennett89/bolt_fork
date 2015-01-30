using Bolt;
using System;
using System.Collections;
using BoltInternal;
using UE = UnityEngine;


/// <summary>
/// Modifier for bolt entity settings before it's attached
/// </summary>
/// <example>
/// *Example:* Attaching an entity with custom settings
/// 
/// ```csharp
/// if (BoltNetwork.isServer) {
///   BoltEntity entity = gameObject.AddComponent&ltBoltEntity&gt();
///   
///   using (var mod = entity.ModifySettings()) {
///     mod.persistThroughSceneLoads = true;
///     mod.allowInstantiateOnClient = false;
///     mod.clientPredicted = false;
///     mod.prefabId = prefabId;
///     mod.updateRate = 1;
///     mod.sceneId = Bolt.UniqueId.None;
///     mod.serializerId = state;
///   }
///   
///   BoltNetwork.Attach(entity.gameObject);
/// }
/// ```
/// </example>
[Documentation]
public class BoltEntitySettingsModifier : IDisposable {
  BoltEntity _entity;

  internal BoltEntitySettingsModifier(BoltEntity entity) {
    _entity = entity;
  }

  /// <summary>
  /// The prefab identifier
  /// </summary>
  public PrefabId prefabId {
    get { return _entity.prefabId; }
    set { _entity.VerifyNotAttached(); _entity._prefabId = value.Value; }
  }

  /// <summary>
  /// A unique identifier present on scene entities
  /// </summary>
  public UniqueId sceneId {
    get { return _entity.sceneGuid; }
    set { _entity.VerifyNotAttached(); _entity.sceneGuid = value; }
  }

  /// <summary>
  /// A unique identifier of this entity state serializer
  /// </summary>
  public UniqueId serializerId {
    get { return _entity.serializerGuid; }
    set { _entity.VerifyNotAttached(); _entity.serializerGuid = value; }
  }

  /// <summary>
  /// The network update rate for this entity
  /// </summary>
  public int updateRate {
    get { return _entity._updateRate; }
    set { _entity.VerifyNotAttached(); _entity._updateRate = value; }
  }

  /// <summary>
  /// Enable or disable client prediction on the entity
  /// </summary>
  public bool clientPredicted {
    get { return _entity._clientPredicted; }
    set { _entity.VerifyNotAttached(); _entity._clientPredicted = value; }
  }

  /// <summary>
  /// Enable or disable instantiation of the entity by clients
  /// </summary>
  public bool allowInstantiateOnClient {
    get { return _entity._allowInstantiateOnClient; }
    set { _entity.VerifyNotAttached(); _entity._allowInstantiateOnClient = value; }
  }

  /// <summary>
  /// Whether the entity is persistence between scenes
  /// </summary>
  public bool persistThroughSceneLoads {
    get { return _entity._persistThroughSceneLoads; }
    set { _entity.VerifyNotAttached(); _entity._persistThroughSceneLoads = value; }
  }

  /// <summary>
  /// True if the entity should be destroyed when detached
  /// </summary>
  public bool sceneObjectDestroyOnDetach {
    get { return _entity._sceneObjectDestroyOnDetach; }
    set { _entity.VerifyNotAttached(); _entity._sceneObjectDestroyOnDetach = value; }
  }

  /// <summary>
  /// True if bolt should automatically attach the entity during instantiation
  /// </summary>
  public bool sceneObjectAutoAttach {
    get { return _entity._sceneObjectAutoAttach; }
    set { _entity.VerifyNotAttached(); _entity._sceneObjectAutoAttach = value; }
  }

  /// <summary>
  /// True if this entity is always owned by the server
  /// </summary>
  public bool alwaysProxy {
    get { return _entity._alwaysProxy; }
    set { _entity.VerifyNotAttached(); _entity._alwaysProxy = value; }
  }

  void IDisposable.Dispose() {

  }
}

/// <summary>
/// A game entity within the bolt simulation
/// </summary>
/// <example>
/// *Example:* Instantiating and taking control of a new ```BoltEntity``` that will replicate to all connected clients.
/// 
/// ```csharp
/// public void InstantiateEntity() {
///   BoltEntity entity = BoltNetwork.Instantiate(BoltPrefabs.Player, RandomSpawn(), Quaternion.identity);
///   
///   entity.TakeControl();
/// }
/// ```
/// </example>
[Documentation]
[UE.ExecuteInEditMode]
[BoltExecutionOrder(-2500)]
public class BoltEntity : UE.MonoBehaviour, IBoltListNode {
  internal Entity _entity;

  [UE.SerializeField]
  internal string _sceneGuid;

  [UE.SerializeField]
  internal string _serializerGuid;

  [UE.SerializeField]
  internal int _prefabId = -1;

  [UE.SerializeField]
  internal int _updateRate = 1;

  [UE.SerializeField]
  internal bool _clientPredicted = true;

  [UE.SerializeField]
  internal bool _allowInstantiateOnClient = true;

  [UE.SerializeField]
  internal bool _persistThroughSceneLoads = false;

  [UE.SerializeField]
  internal bool _sceneObjectDestroyOnDetach = false;

  [UE.SerializeField]
  internal bool _sceneObjectAutoAttach = true;

  [UE.SerializeField]
  internal bool _alwaysProxy = false;

  internal Entity Entity {
    get {
      if (_entity == null) {
        throw new BoltException("You can't access any Bolt specific methods or properties on an entity which is detached");
      }

      return _entity;
    }
  }

  internal UniqueId sceneGuid {
    get { return UniqueId.Parse(_sceneGuid); }
    set { _sceneGuid = value.guid.ToString(); }
  }

  internal UniqueId serializerGuid {
    get { return UniqueId.Parse(_serializerGuid); }
    set { _serializerGuid = value.guid.ToString(); }
  }

  object IBoltListNode.prev { get; set; }
  object IBoltListNode.next { get; set; }
  object IBoltListNode.list { get; set; }

  /// <summary>
  /// The prefabId used to instantiate this entity
  /// </summary>
  /// <example>
  /// *Example:* Cloning an entity with ```prefabId```.
  /// 
  /// ```csharp
  /// BoltEntity Replicate(BoltEntity entity) {
  ///   return Instantiate(entity.prefabId, entity.transform.position, entity.transform.rotation);
  /// }
  /// ```
  /// </example>
  public PrefabId prefabId {
    get { return new PrefabId(_prefabId); }
  }

  /// <summary>
  /// If this entity was created on another computer, contains the connection we received this entity from, otherwise null
  /// </summary>
  /// <example>
  /// *Example:* Using ```source``` to determine if an entity was originally instantiated on a remote host. 
  /// 
  /// ```csharp
  /// bool IsRemoteEntity(BoltEntity entity) {
  ///   return entity.source != null;
  /// }
  /// ```
  /// </example>
  public BoltConnection source {
    get { return Entity.Source; }
  }

  /// <summary>
  /// The unique id of this entity
  /// </summary>
  public NetworkId networkId {
    get { return Entity.NetworkId; }
  }

  /// <summary>
  /// Whether the entity can be paused / frozen
  /// </summary>
  /// <example>
  /// *Example:* Pausing all entities that can be frozen.
  /// 
  /// ```csharp
  /// void Pause() { 
  ///   foreach(BoltEntity entity in BoltNetwork.entities) {
  ///     if(entity.canFreeze) {
  ///       entity.Freeze(true);
  ///     }
  ///   }
  /// }
  /// ```
  /// </example>
  public bool canFreeze {
    get { return Entity.CanFreeze; }
    set { Entity.CanFreeze = value; }
  }

  /// <summary>
  /// If this entity is controlled by a remote connection it contains that connection, otherwise null
  /// </summary>
  /// <example>
  /// *Example:* Disconnecting a client when they run out of lives.
  /// 
  /// ```csharp
  /// void OnDeath(BoltEntity entity) {
  ///   if(entity.GetState%ltILivingEntity&gt().lives == 0) {
  ///     entity.controller.Disconnect(new ServerMessage("Game Over", "Zero Lives Remaining"));
  ///   }
  /// }
  /// ```
  /// </example>
  public BoltConnection controller {
    get { return Entity.Controller; }
  }

  /// <summary>
  /// If this entity is attached to Bolt or not
  /// </summary>
  /// <example>
  /// *Example:* Checking if an entity is still attached before trying to modify the state. This can occur if entities are
  /// destroyed immediately upon death.
  /// 
  /// void DealDamage(BoltEntity entity, AttackData atkData) {
  ///   if(entity.isAttached) {
  ///     entity.GetState&ltILivingEntity&gt().Modify().hp -= atkData.damage;
  ///   }
  /// }
  /// </example>
  public bool isAttached {
    get { return (_entity != null) && _entity.IsAttached; }
  }

  /// <summary>
  /// If this entity is currently paused
  /// </summary>
  /// <example>
  /// *Example:* Unpausing all frozen entities.
  /// 
  /// ```csharp
  /// void Unpause() {
  ///   foreach(BoltEntity entity in BoltNetwork.entities) {
  ///     if(entity.isFrozen) {
  ///       entity.Freeze(false);
  ///     }
  ///   }
  /// }
  /// ```
  /// </example>
  public bool isFrozen {
    get { return isAttached && BoltCore._entitiesFrozen.Contains(Entity); }
  }

  /// <summary>
  /// This is a scene object placed in the scene in the Unity editor
  /// </summary>
  /// <example>
  /// *Example:* Configuring the state of initial buildings and minions during startup.
  /// 
  /// ```csharp
  /// public override void BoltStarted() {
  ///   foreach(BoltEntity entity in BoltNetwork.entities) {
  ///     if(entity.isSceneObject) {
  ///       if(entity.StateIs&ltIStructure&gt()) {
  ///         StructureFactory.Configure(entity);
  ///       }
  ///       else if(entity.StateIs&ltIMinion&gt()) {
  ///         MinionFactory.Configure(entity);
  ///       }
  ///     }
  ///   }
  /// }
  /// ```
  /// </example>
  public bool isSceneObject {
    get { return Entity.IsSceneObject; }
  }

  /// <summary>
  /// Did the local computer create this entity or not?
  /// </summary>
  /// <example>
  /// *Example:* Implementing an authoritative weapon firing method.
  /// 
  /// ```csharp
  /// void FireWeapon(PlayerCommand cmd) {
  ///    if (activeWeapon.fireFrame + activeWeapon.refireRate <= BoltNetwork.serverFrame) {
  ///      activeWeapon.fireFrame = BoltNetwork.serverFrame;
  ///
  ///      state.Fire();
  ///
  ///      if (entity.isOwner) {
  ///        activeWeapon.OnOwner(cmd, entity);
  ///      }
  ///    }
  /// }
  /// ```
  /// </example>
  public bool isOwner {
    get { return Entity.IsOwner; }
  }

  /// <summary>
  /// Do we have control of this entity?
  /// </summary>
  /// <example>
  /// *Example:* Implementing a method to turn controlled entities green on the minimap.
  /// 
  /// ```csharp
  /// public void AddKnownEntity(BoltEntity entity) {
  ///   _minimap.AddNewEntity(entity.networkId, entity);
  ///   
  ///   if(entity.hasControl) {
  ///     _minimap.entities[entity.networkId].color = Color.Green;
  ///   }
  /// }
  /// ```
  /// </example>
  public bool hasControl {
    get { return Entity.HasControl; }
  }

  /// <summary>
  /// Do we have control of this entity and are we using client side prediction
  /// </summary>
  public bool hasControlWithPrediction {
    get { return Entity.HasPredictedControl; }
  }

  /// <summary>
  /// Should this entity persist between scene loads
  /// </summary>
  /// <example>
  /// *Example:* Pausing all persistent entities before changing scenes
  /// 
  /// ```csharp
  /// public override void SceneLoadLocalBegin(string map) {
  ///   foreach(BoltEntity entity in BoltNetwork.entities) {
  ///     if(entity.persistsOnSceneLoad) {
  ///       entity.Freeze(true);
  ///     }
  ///   }
  /// }
  /// ```
  /// </example>
  public bool persistsOnSceneLoad {
    get { return Entity.PersistsOnSceneLoad; }
  }

  /// <summary>
  /// Creates an object which lets you modify the internal settings of an entity before it is attached to Bolt.
  /// </summary>
  /// <returns>The object used to modify entity settings</returns>
  public BoltEntitySettingsModifier ModifySettings() {
    VerifyNotAttached();
    return new BoltEntitySettingsModifier(this);
  }

  /// <summary>
  /// Sets the scope of all currently active connections for this entity. Only usable if Scope Mode has been set to Manual.
  /// </summary>
  /// <param name="inScope">If this entity should be in scope or not</param>
  /// <example>
  /// *Example:* A method which renders an entity invisible to all active client connections.
  /// 
  /// ```csharp
  /// public void Disappear(BoltEntity entity) {
  ///   entity.SetScopeAll(false);
  /// } 
  /// ```
  /// </example>
  public void SetScopeAll(bool inScope) {
    Entity.SetScopeAll(inScope);
  }

  /// <summary>
  /// Sets the scope for the connection passed in for this entity. Only usable if Scope Mode has been set to Manual.
  /// </summary>
  /// <param name="connection">The connection being scoped</param>
  /// <param name="inScope">If this entity should be in scope or not</param>
  /// <example>
  /// *Example:* A coroutine that renders a player invisible to enemies for a given time in seconds.
  /// 
  /// ```csharp
  /// IEnumerator Stealth(BoltEntity entity, float stealthInterval) {
  ///   int team = entity.GetState&ltIPlayerState&gt().team;
  ///   
  ///   foreach(BoltEntity adjEntity in BoltNetwork.entities) {
  ///     if(adjEntity == entity) continue;
  ///     
  ///     if(!adjEntity.StateIs%ltIPlayerState%gt()) continue;
  ///     
  ///     int adjTeam = adjEntity.GetState&ltIPlayerState&gt().team;
  ///     if(team != adjTeam) {
  ///       entity.SetScope(adjEntity.controller, false);
  ///     }
  ///   }
  ///   
  ///   yield return new WaitForSeconds(stealthInterval);
  ///   
  ///   entity.SetScopeAll(true);
  /// }
  /// 
  /// ```
  /// </example>
  public void SetScope(BoltConnection connection, bool inScope) {
    Entity.SetScope(connection, inScope);
  }

  /// <summary>
  /// Sets the parent of this entity
  /// </summary>
  /// <param name="parent">The parent of this entity</param>
  /// <example>
  /// *Example:* Instantiating a vehicle and taking control of it.
  /// 
  /// ```csharp
  /// public void SpawnPlayerVehicle(BoltEntity playerEntity) {
  ///   var car = BoltNetwork.Instantiate(BoltPrefabs.Vehicle, playerEntity.transform.position, playerEntity.transform.rotation);
  ///   
  ///   car.AssignContro(playerEntity.controller);
  ///   playerEntity.RevokeControl();
  ///   playerEntity.SetParent(car);
  /// }
  /// ```
  /// </example>
  public void SetParent(BoltEntity parent) {
    if (parent && parent.isAttached) {
      Entity.SetParent(parent.Entity);
    }
    else {
      Entity.SetParent(null);
    }
  }

  /// <summary>
  /// Takes local control of this entity
  /// </summary>
  /// <example>
  /// *Example:* Spawning a server-side player entity.
  /// 
  /// ```csharp
  /// void SpawnServerPlayer() {
  ///   var entity = Instantiate(BoltPrefabs.Player);
  ///   
  ///   entity.TakeControl();
  /// }
  /// ```
  /// </example>
  public void TakeControl() {
    Entity.TakeControl(null);
  }

  /// <summary>
  /// Takes local control of this entity
  /// </summary>
  /// <param name="token">A data token of max size 512 bytes</param>
  /// <example>
  /// *Example:* Spawning a server-side player entity and initializing it with a local hotkey configuration.
  /// 
  /// ```csharp
  /// HotkeyConfig hotkeys;
  /// 
  /// void SpawnServerPlayer() {
  ///   var entity = Instantiate(BoltPrefabs.Player);
  ///   
  ///   entity.TakeControl(hotkeys);
  /// }
  /// ```
  /// </example>
  public void TakeControl(IProtocolToken token) {
    Entity.TakeControl(token);
  }

  /// <summary>
  /// Releases local control of this entity
  /// </summary> 
  /// <example>
  /// *Example:* Releasing control of a player entity when exiting the game.
  /// 
  /// ```csharp
  /// BoltEntity controlledEntity;
  /// 
  /// public override void ExitGame() {
  ///   controlledEntity.ReleaseControl();
  ///   BoltNetwork.server.Disconnect();
  /// }
  /// ```
  /// </example>
  public void ReleaseControl() {
    Entity.ReleaseControl(null);
  }

  /// <summary>
  /// Releases local control of this entity
  /// </summary>
  /// <param name="token">A data token of max size 512 bytes</param>
  /// <example>
  /// *Example:* Releasing control of a player entity when exiting the game.
  /// 
  /// ```csharp
  /// BoltEntity controlledEntity;
  /// 
  /// public override void ExitGame() {
  ///   ClientEvent evt = new ClientEvent();
  ///   evt.EvtCode = ClientEventCode.EXIT_GAME;
  ///   
  ///   controlledEntity.ReleaseControl(evt);
  ///   BoltNetwork.server.Disconnect();
  /// }
  /// ```
  /// </example>
  public void ReleaseControl(IProtocolToken token) {
    Entity.ReleaseControl(token);
  }

  /// <summary>
  /// Assigns control of this entity to a connection
  /// </summary>
  /// <param name="connection">The connection to assign control to</param>
  /// <example>
  /// *Example:* Instantiating and assigning control of an entity to a newly connected player.
  /// 
  /// ```csharp
  /// public override void Connected(BoltConnection connection) {
  ///   var playerEntity = BoltNetwork.Instantiate(BoltPrefabs.Player, RandomSpawn(), Quaternion.Identity);
  ///   
  ///   playerEntity.AssignControl(connection);
  /// }
  /// ```
  /// </example>
  public void AssignControl(BoltConnection connection) {
    Entity.AssignControl(connection, null);
  }

  /// <summary>
  /// Assigns control of this entity to a connection
  /// </summary>
  /// <param name="connection">The connection to assign control to</param>
  /// <param name="token">A data token of max size 512 bytes</param>
  /// <example>
  /// *Example:* Instantiating and assigning control of an entity to a newly connected player.
  /// 
  /// ```csharp
  /// public override void Connected(BoltConnection connection, IProtocolToken token) {
  ///   var playerEntity = BoltNetwork.Instantiate(BoltPrefabs.Player, RandomSpawn(), Quaternion.Identity);
  /// 
  ///   var fingerprint = ((UserInfo)token).fingerprint;
  ///   PlayerLoadout loadout;
  /// 
  ///   if(playerDatabase.Contains(fingerprint, out loadout)) {     
  ///     playerEntity.AssignControl(connection, loadout);
  ///   }
  ///   else {
  ///     playerEntity.AssignControl(connection, new BeginnerLoadout());
  ///   }
  /// }
  /// ```
  /// </example>
  public void AssignControl(BoltConnection connection, IProtocolToken token) {
    Entity.AssignControl(connection, token);
  }

  /// <summary>
  /// Revokes control of this entity from a connection
  /// </summary>
  /// <example>
  /// *Example:* A server-side stun routine that completely revokes control for the length of stun interval.
  /// 
  /// ```csharp
  /// IEnumerator Stun(BoltEntity entity, float stunInterval) {
  ///   var controller = entity.controller;
  ///   entity.RevokeControl();
  ///   entity.GetState&ltILivingEntity&gt().Modify().stunned = true;
  ///   
  ///   return new WaitForSeconds(stunInterval);
  ///   
  ///   entity.AssignControl(controller);
  ///   entity.GetState&ltILivingEntity&gt().Modify().stunned = false;
  /// }
  /// ```
  /// </example>
  public void RevokeControl() {
    Entity.RevokeControl(null);
  }

  /// <summary>
  /// Revokes control of this entity from a connection
  /// </summary>
  /// <param name="token">A data token of max size 512 bytes</param>
  /// <example>
  /// *Example:* A server-side stun routine that completely revokes control for the length of stun interval.
  /// 
  /// ```csharp
  /// IEnumerator Stun(BoltEntity entity, float stunInterval) {
  ///   var controller = entity.controller;
  ///   CombatEventData evtData = new CombatEventData();
  ///   evtData.stunInterval = stunInterval;
  ///   
  ///   entity.RevokeControl(evtData);
  ///   entity.GetState&ltILivingEntity&gt().Modify().stunned = true;
  ///   
  ///   return new WaitForSeconds(stunInterval);
  ///   
  ///   entity.AssignControl(controller);
  ///   entity.GetState&ltILivingEntity&gt().Modify().stunned = false;
  /// }
  /// ```
  /// </example>
  public void RevokeControl(IProtocolToken token) {
    Entity.RevokeControl(token);
  }

  /// <summary>
  /// Checks if this entity is being controlled by the connection
  /// </summary>
  /// <param name="connection">The connection to check</param>
  public bool IsController(BoltConnection connection) {
    return ReferenceEquals(Entity.Controller, connection);
  }

  /// <summary>
  /// Queue an input data on this entity for execution. This is called on a client which is 
  /// controlling a proxied entity. The data will be sent to the server for authoritative execution
  /// </summary>
  /// <param name="data">The input data to queue</param>
  /// <example>
  /// *Example:* A ```SimulateController()``` loop that queues WASD-style movement input.
  /// 
  /// ```csharp
  /// bool forward;
  /// bool backward;
  /// bool left;
  /// bool right;
  /// 
  /// public override void SimulateController() {
  ///   IPlayerCommandInput input = PlayerCommand.Create();
  /// 
  ///   PollKeys();
  ///   
  ///   input.forward = forward;
  ///   input.backward = backward;
  ///   input.left = left;
  ///   input.right = right;
  ///   
  ///   entity.QueueInput(input);
  /// }
  /// ```
  /// </example>
  public bool QueueInput(INetworkCommandData data) {
    return Entity.QueueInput(((NetworkCommand_Data)data).RootCommand);
  }

  /// <summary>
  /// Set this entity as idle on the supplied connection, this means that the connection 
  /// will not receive update state for this entity as long as it's idle.
  /// </summary>
  /// <param name="connection">The connection to idle the entity on</param>
  /// <param name="idle">If this should be idle or not</param>
  public void Idle(BoltConnection connection, bool idle) {
    Entity.SetIdle(connection, idle);
  }

  /// <summary>
  /// Freeze or unfreeze an entity
  /// </summary>
  /// <param name="pause">True if entity should freeze and false to unfreeze</param>
  /// <example>
  /// *Example:* Pausing all entities.
  /// 
  /// ```csharp
  /// void Pause() { 
  ///   foreach(BoltEntity entity in BoltNetwork.entities) {
  ///     if(entity.canFreeze) {
  ///       entity.Freeze(true);
  ///     }
  ///   }
  /// }
  /// ```
  /// </example>
  public void Freeze(bool pause) {
    Entity.Freeze(pause);
  }

  /// <summary>
  /// Add an event listener to this entity.
  /// </summary>
  /// <param name="behaviour">The behaviour to invoke event callbacks on</param>
  public void AddEventListener(UE.MonoBehaviour behaviour) {
    Entity.AddEventListener(behaviour);
  }

  /// <summary>
  /// Remove an event listern from this entity
  /// </summary>
  /// <param name="behaviour">The behaviour to remove</param>
  public void RemoveEventListener(UE.MonoBehaviour behaviour) {
    Entity.RemoveEventListener(behaviour);
  }

  /// <summary>
  /// Get the state if this entity
  /// </summary>
  /// <typeparam name="TState">The type of state to get</typeparam>
  /// <returns>The state</returns>
  /// <example>
  /// *Example:* Modifying the state of an entity to change its name.
  /// 
  /// ```csharp
  /// public void RenameEntity(BoltEntity entity, string name) {
  ///   entity.GetState&ltIPlayerState&gt().Modify().name = name;
  /// }
  /// ```
  /// </example>
  public TState GetState<TState>() {
    if (Entity.Serializer is TState) {
      return (TState)(object)Entity.Serializer;
    }

    BoltLog.Error("You are trying to access the state of {0} as '{1}'", Entity, typeof(TState));
    return default(TState);
  }

  /// <summary>
  /// A null safe way to look for a specific type of state on an entity
  /// </summary>
  /// <typeparam name="TState">The state type to search for</typeparam>
  /// <param name="state">Entity to search</param>
  /// <example>
  /// *Example:* Pausing all player entities using ```TryFindState()```.
  /// 
  /// ```csharp
  /// public void FreezeAllPlayers() {
  ///   foreach(BoltEntity entity in BoltNetwork.entities) {
  ///     IPlayerState state;
  ///     
  ///     if(entity.TryGetState&ltIPlayerState&gt(out state)) {
  ///       entity.Freeze(true);
  ///       state.Modify().pausedByServer = true;
  ///     }
  ///   }
  /// }
  /// ```
  /// </example>
  public bool TryFindState<TState>(out TState state) {
    if (Entity.Serializer is TState) {
      state = (TState)(object)Entity.Serializer;
      return true;
    }

    state = default(TState);
    return false;
  }

  /// <summary>
  /// Checks which type of state this entity has
  /// </summary>
  /// <typeparam name="TState">The type of state to check for</typeparam>
  /// <returns>True if this entity has a state of type TState otherwise false</returns>
  /// <example>
  /// *Example:* Discarding attack requests that do not target living entities.
  /// 
  /// ```csharp
  /// public override void FireOnOwner(BoltEntity entity, BoltEntity target, AttackData attack) {
  ///   if(entity.isOwner) {
  ///     if(!target.StateIs&ltILivingEntity&gt()) {
  ///       return;
  ///     }
  ///   
  ///     target.GetState&ltILivingEntity&gt().Modify().health -= attack.damage;
  ///   }
  /// }
  /// ```
  /// </example>
  public bool StateIs<TState>() {
    return Entity.Serializer is TState;
  }

  /// <summary>
  /// Checks which type of state this entity has
  /// </summary>
  /// <param name="t">The type of state to check for</param>
  /// <returns>True if this entity has a state of type TState otherwise false</returns>
  /// <example>
  /// *Example:* Discarding attack requests that do not target living entities.
  /// 
  /// ```csharp
  /// public override void FireOnOwner(BoltEntity entity, BoltEntity target, AttackData attack) {
  ///   if(entity.isOwner) {
  ///     if(!target.StateIs(typeof(ILivingEntity))) {
  ///       return;
  ///     }
  ///   
  ///     target.GetState&ltILivingEntity&gt().Modify().health -= attack.damage;
  ///   }
  /// }
  /// ```
  /// </example>
  public bool StateIs(Type t) {
    return t.IsAssignableFrom(Entity.Serializer.GetType());
  }

  /// <summary>
  /// String representation of the entity
  /// </summary>
  /// <example>
  /// *Example:* Logging new entities to the debug log.
  /// 
  /// ```csharp
  /// public override void EntityAttached(BoltEntity entity) {
  ///   Debug.Log("Entity Attached: " + entity.ToString());
  /// }
  /// ```
  /// </example>
  public override string ToString() {
    if (isAttached) {
      return Entity.ToString();
    }
    else {
      return string.Format("[DetachedEntity {2} SceneId={0} SerializerId={1} {3}]", sceneGuid, serializerGuid, prefabId, gameObject.name);
    }
  }

  /// <summary>
  /// Destroy this entity after a given delay
  /// </summary>
  /// <param name="time">The time interval to delay</param>
  /// <example>
  /// *Example:* A death routine that makes state changes and initiates a destruction with configurable delay.
  /// 
  /// ```csharp
  /// void OnDeath(BoltEntity entity) {
  ///   var state = entity.GetState&ltILivingEntity&gt();
  ///   
  ///   state.Modify().alive = false;
  ///   state.Modify().DeathTrigger();
  ///   
  ///   entity.DestroyDelayed(ServerConfig.DESTRUCT_DELAY);
  /// }
  /// ```
  /// </example>
  public void DestroyDelayed(float time) {
    StartCoroutine(DestroyDelayedInternal(time));
  }

  internal void VerifyNotAttached() {
    if (isAttached) {
      throw new InvalidOperationException("You can't modify a BoltEntity behaviour which is attached to Bolt");
    }
  }

  IEnumerator DestroyDelayedInternal(float time) {
    yield return new UE.WaitForSeconds(time);

    if (isAttached) {
      BoltNetwork.Destroy(gameObject);
    }
  }

  void Awake() {
    // only in the editor
    if ((UE.Application.isEditor == true) && (UE.Application.isPlaying == false)) {
      // check if we don't have a valid scene guid
      if (sceneGuid == UniqueId.None) {
        // set a new one
        sceneGuid = UniqueId.New();

        // tell editor to save us
        BoltCoreInternal.ChangedEditorEntities.Add(this);
      }
    }
  }

  void OnDisable() {
    if (UE.Application.isPlaying) {
      OnDestroy();
    }
  }

  void OnDestroy() {
    if (_entity && _entity.IsAttached && UE.Application.isPlaying) {
      if (_entity.IsOwner) {
        BoltLog.Warn("{0} is being destroyed/disabled without being detached, forcing detach", Entity);
      }
      else {
        BoltLog.Error("{0} is being destroyed/disabled without being detached by the owner, this will cause this peer to disconnect the next time it receives an update for this entity", Entity);
      }

      // force detach
      _entity.Detach();
      _entity = null;
    }
  }

  void OnDrawGizmos() {
    UE.Gizmos.DrawIcon(transform.position, "BoltEntity Gizmo", true);
  }

  //void Update() {
  //  if (isAttached && UE.Application.isPlaying) {
  //    Entity.Render();
  //  }
  //}

  public static implicit operator UE.GameObject(BoltEntity entity) {
    return entity == null ? null : entity.gameObject;
  }
}
using System;
using System.Collections.Generic;
using UdpKit;
using UE = UnityEngine;

public interface IBoltEntitySettingsModifier : IDisposable {
  Bolt.PrefabId prefabId { get; set; }
  Bolt.UniqueId defaultSerializerUniqueId { get; set; }

  int updateRate { get; set; }

  bool clientPredicted { get; set; }
  bool allowInstantiateOnClient { get; set; }
  bool persistThroughSceneLoads { get; set; }
}

/// <summary>
/// Enables a game object to be tracked by Bolt over the network
/// </summary>
[BoltExecutionOrder(-2500)]
public class BoltEntity : UE.MonoBehaviour, IBoltListNode {

  class SettingsModifier : IBoltEntitySettingsModifier {
    BoltEntity _entity;

    public SettingsModifier(BoltEntity entity) {
      _entity = entity;
    }

    Bolt.PrefabId IBoltEntitySettingsModifier.prefabId {
      get { return new Bolt.PrefabId(_entity._prefabId); }
      set { _entity.VerifyNotAttached(); _entity._prefabId = value.Value; }
    }

    Bolt.UniqueId IBoltEntitySettingsModifier.defaultSerializerUniqueId {
      get { return _entity.defaultSerializerId; }
      set { _entity.VerifyNotAttached(); _entity.defaultSerializerId = value; }
    }

    int IBoltEntitySettingsModifier.updateRate {
      get { return _entity._updateRate; }
      set { _entity.VerifyNotAttached(); _entity._updateRate = value; }
    }

    bool IBoltEntitySettingsModifier.clientPredicted {
      get { return _entity._clientPredicted; }
      set { _entity.VerifyNotAttached(); _entity._clientPredicted = value; }
    }

    bool IBoltEntitySettingsModifier.allowInstantiateOnClient {
      get { return _entity._allowInstantiateOnClient; }
      set { _entity.VerifyNotAttached(); _entity._allowInstantiateOnClient = value; }
    }

    bool IBoltEntitySettingsModifier.persistThroughSceneLoads {
      get { return _entity._persistThroughSceneLoads; }
      set { _entity.VerifyNotAttached(); _entity._persistThroughSceneLoads = value; }
    }

    void IDisposable.Dispose() {

    }
  }

  internal Bolt.Entity _entity;

  [UE.SerializeField]
  internal byte[] _sceneId;

  [UE.SerializeField]
  internal byte[] _defaultSerializerGuid;

  [UE.SerializeField]
  internal int _prefabId = -1;

  [UE.SerializeField]
  internal int _updateRate = 1;

  [UE.SerializeField]
  int _defaultSerializerTypeId = 0;

  [UE.SerializeField]
  internal bool _clientPredicted = true;

  [UE.SerializeField]
  internal bool _allowInstantiateOnClient = true;

  [UE.SerializeField]
  internal bool _persistThroughSceneLoads = false;

  internal Bolt.Entity Entity {
    get {
      if (_entity == null) {
        throw new BoltException("You can't access any Bolt specific methods or properties on an entity which is detached");
      }

      return _entity;
    }
  }

  internal Bolt.UniqueId sceneId {
    get {
      if (_sceneId == null || _sceneId.Length != 16) {
        return Bolt.UniqueId.None;
      }

      return new Bolt.UniqueId(_sceneId);
    }
    set {
      _sceneId = value.ToByteArray();
    }
  }

  internal Bolt.UniqueId defaultSerializerId {
    get {
      if (_defaultSerializerGuid == null || _defaultSerializerGuid.Length != 16) {
        return Bolt.UniqueId.None;
      }

      return new Bolt.UniqueId(_defaultSerializerGuid);
    }
    set {
      _defaultSerializerGuid = value.ToByteArray();
    }
  }

  object IBoltListNode.prev { get; set; }
  object IBoltListNode.next { get; set; }
  object IBoltListNode.list { get; set; }

  public Bolt.UniqueId uniqueId {
    get { return Entity.UniqueId; }
  }

  public BoltConnection source {
    get { return Entity.Source; }
  }

  public BoltConnection controller {
    get { return Entity.Controller; }
  }

  public bool isAttached {
    get { return (_entity != null) && _entity.IsAttached; }
  }

  public bool isSceneObject {
    get { return Entity.IsSceneObject; } 
  }

  public bool isOwner {
    get { return Entity.IsOwner; }
  }

  public bool hasControl {
    get { return Entity.HasControl; }
  }

  public bool hasControlWithPrediction {
    get { return Entity.HasPredictedControl; }
  }

  public bool persistsOnSceneLoad {
    get { return Entity.PersistsOnSceneLoad; }
  }

  public bool canQueueCommands {
    get { return Entity.CanQueueCommands; }
  }

  public IBoltEntitySettingsModifier ModifySettings() {
    VerifyNotAttached();
    return new SettingsModifier(this);
  }

  public void SetScopeAll(bool inScope) {
    Entity.SetScopeAll(inScope);
  }

  public void SetScope(BoltConnection connection, bool inScope) {
    Entity.SetScope(connection, inScope);
  }

  public void SetParent(BoltEntity entity) {
    Entity.SetParent(entity.Entity);
  }

  /// <summary>
  /// <p>Takes local control of this entity</p>
  /// </summary>
  public void TakeControl() {
    Entity.TakeControl();
  }

  /// <summary>
  /// Releases local control of this entity
  /// </summary>
  public void ReleaseControl() {
    Entity.ReleaseControl();
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  public void SetUniqueId(Bolt.UniqueId id) {
    Entity.SetUniqueId(id);
  }

  /// <summary>
  /// Assigns control of this entity to a connection
  /// </summary>
  /// <param name="connection">The connection to assign control to</param>
  public void AssignControl(BoltConnection connection) {
    Entity.AssignControl(connection);
  }

  /// <summary>
  /// Revokes control of this entity from a connection
  /// </summary>
  public void RevokeControl() {
    Entity.RevokeControl();
  }

  /// <summary>
  /// Checks if this entity is being controlled by the connection
  /// </summary>
  /// <param name="connection">The connection to check</param>
  public bool IsController(BoltConnection connection) {
    return ReferenceEquals(Entity.Controller, connection);
  }

  /// <summary>
  /// Queue a command not his entity for execution. This is called on a client which is 
  /// controlling a proxied entity the command will also be sent to the server.
  /// </summary>
  /// <param name="command">The command to queue</param>
  public bool QueueInput(Bolt.ICommandInput command) {
    return Entity.QueueInput((Bolt.Command)(object)command);
  }

  /// <summary>
  /// Set this entity as idle on the supplied connection, this means that the connection 
  /// will not receive update state for this entity as long as it's idle.
  /// </summary>
  /// <param name="connection">The connection to idle the entity on</param>
  public void Idle(BoltConnection connection) {
    Entity.SetIdle(connection, true);
  }

  /// <summary>
  /// Wakes this entity up from being idle on the supplied connection, this means that the
  /// connection will start receiving updated state for this entity
  /// </summary>
  /// <param name="connection">The connection to wake the entity up on</param>
  public void Wakeup(BoltConnection connection) {
    Entity.SetIdle(connection, false);
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
  public TState GetState<TState>() {
    if (Entity.Serializer is TState) {
      return (TState)(object)Entity.Serializer;
    }

    BoltLog.Error("You are trying to access the state of {0} as '{1}'", Entity, typeof(TState));
    return default(TState);
  }

  public bool StateIs<TState>() {
    return Entity.Serializer is TState;
  }

  public override string ToString() {
    return Entity.ToString();
  }

  internal void VerifyNotAttached() {
    if (isAttached) {
      throw new BoltException("You can't modify a BoltEntity behaviour which is attached to Bolt");
    }
  }

  void Awake() {
    DontDestroyOnLoad(gameObject);
  }

  void OnEnable() {
    DontDestroyOnLoad(gameObject);
  }

  void OnDisable() {
    OnDestroy();
  }

  void OnDestroy() {
    if (_entity) {
      // log that his is happening
      BoltLog.Warn("{0} is being destroyed or disabled without being detached, forcing detach", Entity);

      // force detach
      _entity.Detach();
      _entity = null;
    }
  }

  void OnDrawGizmos() {
    UE.Gizmos.DrawIcon(transform.position, "BoltEntity Gizmo", true);
  }

  void Update() {
    if (isAttached) {
      Entity.Render();
    }
  }
}
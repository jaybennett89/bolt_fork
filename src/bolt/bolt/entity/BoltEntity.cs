using System;
using System.Collections.Generic;
using UdpKit;
using UE = UnityEngine;

/// <summary>
/// Enables a game object to be tracked by Bolt over the network
/// </summary>
[BoltExecutionOrder(-2500)]
public class BoltEntity : UE.MonoBehaviour, IBoltListNode {
  [UE.SerializeField]
  internal int _prefabId = -1;

  [UE.SerializeField]
  internal int _updateRate = 1;

  [UE.SerializeField]
  internal int _defaultSerializerTypeId = 0;

  [UE.SerializeField]
  internal bool _clientPredicted = true;

  [UE.SerializeField]
  internal bool _allowInstantiateOnClient = true;

  [UE.SerializeField]
  internal bool _persistThroughSceneLoads = false;

  [UE.SerializeField]
  internal UE.Object[] _objects;

  // our link to Bolts internal entity object
  internal Bolt.EntityObject Entity;

  object IBoltListNode.prev { get; set; }
  object IBoltListNode.next { get; set; }
  object IBoltListNode.list { get; set; }

  public BoltConnection source {
    get { return Entity.Source; }
  }

  public BoltConnection controller {
    get { return Entity.Controller; }
  }

  public bool isAttached {
    get { return Entity != null; }
  }

  public bool isOwner {
    get { return Entity.IsOwner; }
  }

  public bool hasControl {
    get { return Entity.HasControl; }
  }

  public bool persistsOnSceneLoad {
    get { return Entity.PersistsOnSceneLoad; }
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
  /// Gives control of this entity to a connection
  /// </summary>
  /// <param name="connection">The connection to give control to</param>
  [Obsolete("Use BoltEntity.AssignControl instead")]
  public void GiveControl(BoltConnection connection) {
    Entity.AssignControl(connection);
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
  public bool IsControlledBy(BoltConnection connection) {
    return ReferenceEquals(Entity.Controller, connection);
  }

  /// <summary>
  /// Queue a command not his entity for execution. This is called on a client which is 
  /// controlling a proxied entity the command will also be sent to the server.
  /// </summary>
  /// <param name="command">The command to queue</param>
  public bool QueueCommand(BoltCommand command) {
    return Entity.QueueCommand(command);
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
  /// Raises an event on this entity. The event will be sent to all valid connections which has a proxy or
  /// is the owner of this entity
  /// </summary>
  /// <param name="ev">The event to raise</param>
  public void Raise(IBoltEvent ev) {
    Entity.Raise(ev);
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
  public TState GetState<TState>() where TState : Bolt.IState {
    return (TState)(object)Entity.Serializer;
  }

  public override string ToString() {
    return Entity.ToString();
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
    if (Entity) {
      // log that his is happening
      BoltLog.Warn("{0} is being destroyed or disabled without being detached, forcing detach", Entity);

      // force detach
      Entity.Detach();
      Entity = null;
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
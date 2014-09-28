using System;
using System.Collections.Generic;
using UdpKit;
using UnityEngine;

/// <summary>
/// Enables a game object to be tracked by Bolt over the network
/// </summary>
[BoltExecutionOrder(-2500)]
public class BoltEntity : MonoBehaviour, IBoltListNode {
  [SerializeField]
  internal int _prefabId = -1;

  [SerializeField]
  internal int _updateRate = 1;

  [SerializeField]
  internal int _defaultSerializerTypeId = 0;

  [SerializeField]
  internal bool _clientPredicted = true;

  [SerializeField]
  internal bool _allowInstantiateOnClient = true;

  [SerializeField]
  internal bool _persistThroughSceneLoads = false;

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
  /// <param name="cn">The connection to give control to</param>
  public void GiveControl(BoltConnection cn) {
    Entity.GiveControl(cn);
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
  /// <param name="cn">The connection to check</param>
  public bool IsControlledBy(BoltConnection cn) {
    return ReferenceEquals(Entity.Controller, cn);
  }

  /// <summary>
  /// Queue a command not his entity for execution. This is called on a client which is 
  /// controlling a proxied entity the command will also be sent to the server.
  /// </summary>
  /// <param name="cmd">The command to queue</param>
  public bool QueueCommand(BoltCommand cmd) {
    return Entity.QueueCommand(cmd);
  }

  /// <summary>
  /// Set this entity as idle on the supplied connection, this means that the connection 
  /// will not receive update state for this entity as long as it's idle.
  /// </summary>
  /// <param name="cn">The connection to idle the entity on</param>
  public void Idle(BoltConnection cn) {
    Entity.SetIdle(cn, true);
  }

  /// <summary>
  /// Wakes this entity up from being idle on the supplied connection, this means that the
  /// connection will start receiving updated state for this entity
  /// </summary>
  /// <param name="cn">The connection to wake the entity up on</param>
  public void Wakeup(BoltConnection cn) {
    Entity.SetIdle(cn, false);
  }

  /// <summary>
  /// Raises an event on this entity. The event will be sent to all valid connections which has a proxy or
  /// is the owner of this entity
  /// </summary>
  /// <param name="ev">The event to raise</param>
  public void Raise(IBoltEvent ev) {
    Entity.Raise(ev);
  }

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
    Gizmos.DrawIcon(transform.position, "BoltEntity Gizmo", true);
  }

  void Update() {
    if (Entity) {
      Entity.Render();
    }
  }

  ///// <summary>
  ///// Sets the origin transform of this entity
  ///// </summary>
  //public void SetOrigin(Transform origin) {
  //  if (!isOwner) {
  //    BoltLog.Error("Only the owner can set the origin of an entity");
  //    return;
  //  }

  //  SetOriginInternal(origin);
  //}

  //internal void SetOriginInternal(Transform origin) {
  //  if (origin != _origin) {
  //    try {
  //      boltSerializer.OriginChanging(_origin, origin);
  //    }
  //    catch (Exception exn) { BoltLog.Error(exn); }

  //    // set origin property
  //    _origin = origin;

  //    // set actual transform parent
  //    transform.parent = _origin;

  //    // log this
  //    BoltLog.Debug("Origin for {0} is now {1}", transform.name, origin == null ? "NULL" : origin.name);
  //  }
  //}

  ///// <summary>
  ///// Teleport this entity to a position and rotation
  ///// </summary>
  //[BoltDocsOwnerOnly]
  //public void Teleport (Vector3 position, Quaternion rotation) {
  //  if (isOwner == false) {
  //    BoltLog.Error("Only the owner of an entity can teleport it");
  //    return;
  //  }

  //  transform.localPosition = position;
  //  transform.localRotation = rotation;

  //  _teleportFlip = !_teleportFlip;
  //}

  ///// <summary>
  ///// Teleport this entity to a position
  ///// </summary>
  ///// <param name="position"></param>
  //[BoltDocsOwnerOnly]
  //public void Teleport (Vector3 position) {
  //  Teleport(position, transform.localRotation);
  //}

  //internal void TakeControlInternal() {
  //  // setup a clean controlling state
  //  _flags |= BoltEntity.FLAG_IS_CONTROLLING;
  //  _commands.Clear();
  //  _commandSequence = 0;

  //  // call to user code, from generic to specialized
  //  BoltCallbacksBase.ControlOfEntityGainedInvoke(this);

  //  foreach (BoltEntityBehaviourBase sp in _entityBehaviours) {
  //    sp.ControlGained();
  //  }

  //  _serializer.ControlGained();
  //}

  //internal void ReleaseControlInternal() {
  //  if ((_flags & BoltEntity.FLAG_IS_CONTROLLING) == false) {
  //    BoltLog.Error("can't release control of {0}, you are not controlling it", this);
  //    return;
  //  }

  //  // clear out state
  //  _flags &= ~BoltEntity.FLAG_IS_CONTROLLING;
  //  _commands.Clear();
  //  _commandSequence = 0;

  //  // call to user code (reverse order from control gained)
  //  _serializer.ControlLost();

  //  foreach (BoltEntityBehaviourBase sp in _entityBehaviours) {
  //    sp.ControlLost();
  //  }

  //  BoltCallbacksBase.ControlOfEntityLostInvoke(this);
  //}
}
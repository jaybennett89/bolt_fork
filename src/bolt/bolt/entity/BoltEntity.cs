using System;
using System.Collections.Generic;
using UdpKit;
using UnityEngine;

/// <summary>
/// Enables a game object to be tracked by Bolt over the network
/// </summary>
[BoltExecutionOrder(-2500)]
public class BoltEntity : MonoBehaviour, IBoltListNode {
  internal const uint FLAG_IS_PROXY = 1;
  internal const uint FLAG_IS_CONTROLLING = 2;
  internal const uint FLAG_DISABLE_PROXYING = 4;
  internal const uint FLAG_REMOTE_CONTROLLED = 8;
  internal const uint FLAG_PERSIST_ON_MAP_LOAD = 16;

  internal const int COMMAND_SEQ_BITS = 8;
  internal const int COMMAND_SEQ_SHIFT = 16 - COMMAND_SEQ_BITS;
  internal const int COMMAND_SEQ_MASK = (1 << COMMAND_SEQ_BITS) - 1;

  static uint _idCounter = 0;

  uint _id;
  bool _attached;

  [SerializeField]
  int _prefabId = -1;

  [SerializeField]
  BoltEntitySerializer _serializer = null;

  [SerializeField]
  BoltEntityPersistanceMode _persistanceMode = BoltEntityPersistanceMode.DestroyOnLoad;

  [SerializeField]
  int _updateRate = 1;

  [SerializeField]
  internal bool _sceneObject = false;

  [SerializeField]
  internal bool _clientPredicted = true;

  internal Bits _mask;
  internal Bits _flags;
  internal bool _teleportFlip;
  internal ushort _commandSequence = 0;
  internal Transform _origin;
  internal BoltConnection _source = null;
  internal BoltConnection _remoteController = null;
  internal BoltEventDispatcher _eventDispatcher = new BoltEventDispatcher();
  internal BoltDoubleList<BoltCommand> _commands = new BoltDoubleList<BoltCommand>();
  internal List<BoltEntityBehaviourBase> _entityBehaviours = new List<BoltEntityBehaviourBase>(32);

  object IBoltListNode.prev { get; set; }
  object IBoltListNode.next { get; set; }
  object IBoltListNode.list { get; set; }

  /// <summary>
  /// Returns the serializer for this entity
  /// </summary>
  public BoltEntitySerializer boltSerializer {
    get { return _serializer; }
  }

  /// <summary>
  /// Returns the connection, if any, that is the source of this entity
  /// </summary>
  public BoltConnection boltSource {
    get { return _source; }
  }

  /// <summary>
  /// Returns the connection, if any, that is remotely controlling this entity
  /// </summary>
  public BoltConnection boltRemoteController {
    get { return _remoteController; }
  }

  /// <summary>
  /// The persistance mode of this entity
  /// </summary>
  public BoltEntityPersistanceMode boltPersistanceMode {
    get { return _persistanceMode; }
  }

  /// <summary>
  /// Prefab id of this entity, this value is assigned by
  /// Bolt when the compiler runs.
  /// </summary>
  public int boltPrefabId {
    get { return _prefabId; }
  }

  /// <summary>
  /// Id of this entity, this value is an ever increasing sequence
  /// is guaranteed to be uqniue for every entity up until 2^32 entities
  /// have spawned.
  /// </summary>
  public uint boltId {
    get { return _id; }
  }

  /// <summary>
  /// How often this entity should be considered for packing by Bolt. 
  /// 1 means every packet, 2 means every other packet, etc.
  /// </summary>
  public int boltPackFrequency {
    get { return _updateRate; }
  }

  /// <summary>
  /// How often (in frames) we try to send states updates for this object
  /// </summary>
  public int boltSendRate {
    get {
      if (boltIsOwner) {
        return _updateRate * BoltCore.localSendRate;
      }
      else {
        return _updateRate * BoltCore.remoteSendRate;
      }
    }
  }

  /// <summary>
  /// Returns true if this entity is attached to bolt
  /// </summary>
  public bool boltIsAttached {
    get { return _attached; }
  }

  /// <summary>
  /// Returns true if this entity is a proxy which originates
  /// from a remote connection. The source connection can be found
  /// in BoltEntity.boltSource
  /// </summary>
  /// 
  [Obsolete("Use BoltEntity.hasAuthority instead")]
  public bool boltIsProxy {
    get { return _flags & FLAG_IS_PROXY; }
  }

  /// <summary>
  /// Returns true if we spawned this entity, false if it's from a remote connection
  /// </summary>
  [Obsolete("Use BoltEntity.spawnedRemotely instead")]
  public bool boltIsOwner {
    get { return ReferenceEquals(_source, null); }
  }

  /// <summary>
  /// Returns true if we are controlling this entity. This can be true on both
  /// an owned and proxied entity.
  /// </summary>
  [Obsolete("Use BoltEntity.hasControl instead")]
  public bool boltIsControlling {
    get { return _flags & FLAG_IS_CONTROLLING; }
  }

  /// <summary>
  /// Returns true if this entity is being controlled by a remote connection
  /// </summary>
  public bool boltIsRemoteControlled {
    get { return _flags & FLAG_REMOTE_CONTROLLED; }
  }

  /// <summary>
  /// Returns true if this entity was placed in the scene
  /// </summary>
  public bool boltIsSceneObject {
    get { return _sceneObject; }
    internal set { _sceneObject = value; }
  }
  
  public bool hasAuthority {
    get { return ReferenceEquals(_source, null); }
  }

  public bool hasControl {
    get { return _flags & FLAG_IS_CONTROLLING; }
  }

  public bool spawnedRemotely {
    get { return ReferenceEquals(_source, null) == false; }
  }

  public bool isDummy {
    get { return spawnedRemotely && !hasControl; }
  }

  /// <summary>
  /// Returns the simulation frame of this entity.
  /// If this is a locally ownd entity it returns the same as BoltNetwork.frame
  /// If this is a remote proxy which we are not in control of this returns BoltConnection.remoteFrameEstimated of the source connection
  /// If this is a remote proxy which we are controlling this returns BoltConnection.remoteFrameLatest of the source connection
  /// </summary>
  public int boltFrame {
    get {
      if (ReferenceEquals(boltSource, null)) {
        return BoltCore._frame;
      }
      else {
        if (boltIsControlling && _clientPredicted) {
          return boltSource.remoteFrameLatest;
        }
        else {
          return boltSource.remoteFrame;
        }
      }
    }
  }

  /// <summary>
  /// The origin transform of this entity
  /// </summary>
  public Transform origin {
    get { return _origin; }
  }

  /// <summary>
  /// Sets the bits in the dirty mask of this entity
  /// </summary>
  /// <example>
  /// // sets the three first bits in the dirty mask
  /// SetMaskBits(1 | 2 | 4)
  /// </example>
  /// <param name="bits">The bits to set</param>
  [BoltDocsOwnerOnly]
  public void SetMaskBits (Bits bits) {
    _mask |= bits;
  }

  /// <summary>
  /// Clear the bits in the dirty mask of this entity
  /// </summary>
  /// <example>
  /// // clears the three first bits in the dirty mask
  /// ClearMaskBits(1 | 2 | 4)
  /// </example>
  /// <param name="bits">The bits to clear</param>
  [BoltDocsOwnerOnly]
  public void ClearMaskBits (Bits bits) {
    _mask &= ~bits;
  }

  /// <summary>
  /// <p>Takes local control of this entity</p>
  /// </summary>
  [BoltDocsOwnerOnly]
  public void TakeControl () {
    if (_flags & BoltEntity.FLAG_IS_PROXY) {
      BoltLog.Error("can't take control of {0}, it is proxied", this);
      return;
    }

    // revoke any remote control of this actor
    RevokeControl();

    Assert.Null(_remoteController);
    Assert.False(_flags & BoltEntity.FLAG_REMOTE_CONTROLLED);

    TakeControlInternal();
  }

  /// <summary>
  /// Releases local control of this entity
  /// </summary>
  [BoltDocsOwnerOnly]
  public void ReleaseControl () {
    if (_flags & BoltEntity.FLAG_IS_PROXY) {
      BoltLog.Error("can't relase control of {0}, it is proxied", this);
      return;
    }

    ReleaseControlInternal();
  }

  /// <summary>
  /// Gives control of this entity to a connection
  /// </summary>
  /// <param name="cn">The connection to give control to</param>
  [BoltDocsOwnerOnly]
  public void GiveControl (BoltConnection cn) {
    if (_flags & BoltEntity.FLAG_IS_PROXY) {
      BoltLog.Error("can't give control of {0} to {1}, it is proxied", this, cn);
      return;
    }

    if (cn._entityChannel.CreateOnRemote(this)) {
      _flags |= BoltEntity.FLAG_REMOTE_CONTROLLED;
      _commands.Clear();
      _commandSequence = 0;
      _remoteController = cn;
      _remoteController._entityChannel.ForceSync(this);

    }
    else {
      BoltLog.Error("couldn't create {0} on {1}, control not given", this, cn);
    }
  }

  /// <summary>
  /// Revokes control of this entity from a connection
  /// </summary>
  [BoltDocsOwnerOnly]
  public void RevokeControl () {
    if (_flags & BoltEntity.FLAG_IS_PROXY) {
      BoltLog.Error("can't revoke control of {0}, it is proxied", this);
      return;
    }

    if (_remoteController) {
      BoltConnection cn = _remoteController;

      _flags &= ~BoltEntity.FLAG_REMOTE_CONTROLLED;
      _commands.Clear();
      _commandSequence = 0;
      _remoteController = null;

      cn._entityChannel.ForceSync(this);
    }
  }

  /// <summary>
  /// Checks if this entity is being controlled by the connection
  /// </summary>
  /// <param name="cn">The connection to check</param>
  [BoltDocsOwnerOnly]
  public bool IsControlledBy (BoltConnection cn) {
    return ReferenceEquals(cn, _remoteController) && (_flags & BoltEntity.FLAG_REMOTE_CONTROLLED);
  }

  /// <summary>
  /// Queue a command not his entity for execution. This is called on a client which is 
  /// controlling a proxied entity the command will also be sent to the server.
  /// </summary>
  /// <param name="cmd">The command to queue</param>
  [BoltDocsControllerOnly]
  public bool QueueCommand (BoltCommand cmd) {
    if (boltIsControlling == false) {
      BoltLog.Error("queue of {0} to {1} failed, you are not controlling this entity", cmd, this);
      return false;
    }

    if (_commands.count > BoltCore._config.commandQueueSize) {
      return false;
    }

    cmd._serverFrame = BoltCore.serverFrame;
    cmd._sequence = _commandSequence = UdpMath.SeqNext(_commandSequence, COMMAND_SEQ_MASK);

    // put on command buffer
    _commands.AddLast(cmd);

    return true;
  }

  /// <summary>
  /// Set this entity as idle on the supplied connection, this means that the connection 
  /// will not receive update state for this entity as long as it's idle.
  /// </summary>
  /// <param name="cn">The connection to idle the entity on</param>
  [BoltDocsOwnerOnly]
  public void Idle (BoltConnection cn) {
    if (ReferenceEquals(cn, _remoteController)) {
      BoltLog.Error("can't idle an entity on the connection which is controlling it");
      return;
    }

    cn._entityChannel.SetIdle(this, true);
  }

  /// <summary>
  /// Wakes this entity up from being idle on the supplied connection, this means that the
  /// connection will start receiving updated state for this entity
  /// </summary>
  /// <param name="cn">The connection to wake the entity up on</param>
  [BoltDocsOwnerOnly]
  public void Wakeup (BoltConnection cn) {
    cn._entityChannel.SetIdle(this, false);
  }

  /// <summary>
  /// Raises an event on this entity. The event will be sent to all valid connections which has a proxy or
  /// is the owner of this entity
  /// </summary>
  /// <param name="event">The event to raise</param>
  public void Raise (IBoltEvent @event) {
    BoltEvent evnt = (BoltEvent) @event;

    if (evnt._isEntityEvent == false) {
      throw new BoltException("you can't send global events to entities"); ;
    }

    if (evnt._deliveryMode == BoltEventDeliveryMode.Reliable) {
      throw new BoltException("you can't send reliable events to entities");
    }

    evnt._entity = this;
    BoltEvent.Invoke(evnt);
  }

  /// <summary>
  /// Sets the origin transform of this entity
  /// </summary>
  [BoltDocsOwnerOnly]
  public void SetOrigin (Transform origin) {
    if (boltIsProxy) {
      BoltLog.Error("Only the owner can set the origin of an entity");
      return;
    }

    SetOriginInternal(origin);
  }

  internal void SetOriginInternal (Transform origin) {
    if (origin != _origin) {
      try {
        boltSerializer.OriginChanging(_origin, origin);
      } catch (Exception exn) { BoltLog.Error(exn); }

      // set origin property
      _origin = origin;

      // set actual transform parent
      transform.parent = _origin;

      // log this
      BoltLog.Debug("Origin for {0} is now {1}", transform.name, origin == null ? "NULL" : origin.name);
    }
  }

  /// <summary>
  /// Teleport this entity to a position and rotation
  /// </summary>
  [BoltDocsOwnerOnly]
  public void Teleport (Vector3 position, Quaternion rotation) {
    if (boltIsOwner == false) {
      BoltLog.Error("Only the owner of an entity can teleport it");
      return;
    }

    transform.localPosition = position;
    transform.localRotation = rotation;

    _teleportFlip = !_teleportFlip;
  }

  /// <summary>
  /// Teleport this entity to a position
  /// </summary>
  /// <param name="position"></param>
  [BoltDocsOwnerOnly]
  public void Teleport (Vector3 position) {
    Teleport(position, transform.localRotation);
  }

  public override string ToString () {
    return string.Format("[Entity id={0} gameobject={1}]", boltId, gameObject.name);
  }

  internal void TakeControlInternal () {
    // setup a clean controlling state
    _flags |= BoltEntity.FLAG_IS_CONTROLLING;
    _commands.Clear();
    _commandSequence = 0;

    // call to user code, from generic to specialized
    BoltCallbacksBase.ControlOfEntityGainedInvoke(this);

    foreach (BoltEntityBehaviourBase sp in _entityBehaviours) {
      sp.ControlGained();
    }

    _serializer.ControlGained();
  }

  internal void ReleaseControlInternal () {
    if ((_flags & BoltEntity.FLAG_IS_CONTROLLING) == false) {
      BoltLog.Error("can't release control of {0}, you are not controlling it", this);
      return;
    }

    // clear out state
    _flags &= ~BoltEntity.FLAG_IS_CONTROLLING;
    _commands.Clear();
    _commandSequence = 0;
    
    // call to user code (reverse order from control gained)
    _serializer.ControlLost();

    foreach (BoltEntityBehaviourBase sp in _entityBehaviours) {
      sp.ControlLost();
    }

    BoltCallbacksBase.ControlOfEntityLostInvoke(this);
  }

  internal void SimulateStep () {

    BoltCommand cmd;
    BoltIterator<BoltCommand> cmdIter;

    _serializer.BeforeStep();

    if (boltIsProxy) {
      Assert.False(boltIsOwner);

      _serializer.SimulateProxy();

      foreach (BoltEntityBehaviourBase sp in _entityBehaviours) {
        sp.SimulateProxy();
      }
    }
    else {
      Assert.True(boltIsOwner);

      _serializer.SimulateOwner();

      foreach (BoltEntityBehaviourBase sa in _entityBehaviours) {
        sa.SimulateOwner();
      }
    }

    if (boltIsControlling) {
      Assert.False(boltIsRemoteControlled);

      // execute all old commands (in order)
      cmd = null;
      cmdIter = _commands.GetIterator();

      while (cmdIter.Next(out cmd)) {
        Assert.True(cmd._hasExecuted);

        // exec old command
        ExecuteCommand(cmd, ReferenceEquals(cmd, _commands.first));
      }

      _serializer.SimulateController();

      // simulate controller
      foreach (BoltEntityBehaviourBase sc in _entityBehaviours) {
        sc.SimulateController();
      }

      // execute all new commands (in order)
      cmd = null;
      cmdIter = _commands.GetIterator();

      while (cmdIter.Next(out cmd)) {
        if (cmd._hasExecuted == false) {
          // exec new command
          ExecuteCommand(cmd, false);
          Assert.True(cmd._hasExecuted);
        }
      }

      // if this is a local proxy we are controlling
      // we should dispose all commands except one
      if (boltIsOwner) {
        while (_commands.count > 0) {
          _commands.RemoveFirst().Dispose();
        }
      }
    }

    if (boltIsRemoteControlled) {
      Assert.False(boltIsProxy);
      Assert.False(boltIsControlling);

      do {
        cmd = null;
        cmdIter = _commands.GetIterator();

        while (cmdIter.Next(out cmd)) {
          if (cmd._hasExecuted == false) {
            try {
              ExecuteCommand(cmd, false);
              break;

            } finally {
              cmd._stateSent = false;
            }
          }
        }
      } while (UnexecutedCommandCount() > BoltCore._config.commandDejitterDelay);
    }

    _serializer.AfterStep();
  }

  internal void Attach (BoltConnection source, Bits flags) {
    if (_attached) {
      return;
    }

    if (!_serializer) {
      BoltLog.Warning("no entity serializer specified for '{0}', trying to find one...", gameObject.name);
      _serializer = GetComponentInChildren<BoltEntitySerializer>();

      if (!_serializer) {
        BoltLog.Error("no entity serializer attached to '{0}'", gameObject.name);
        return;
      }
    }

    // all entity destruction is controlled by bolt
    DontDestroyOnLoad(gameObject);

    _flags = flags;
    _source = source;

    // set persistance flag
    if (_persistanceMode == BoltEntityPersistanceMode.PersistOnLoad) {
      _flags |= FLAG_PERSIST_ON_MAP_LOAD;
    }

    // set attached flag
    _attached = true;

    // add to entity list
    BoltCore._entities.AddLast(this);

    // call attached on serializer
    _serializer._entity = this;
    _serializer.Attached();

    // attached callback on all behaviours
    foreach (BoltEntityBehaviourBase ac in _entityBehaviours) {
      ac.Attached();
    }

    // notify user
    BoltCallbacksBase.EntityAttachedInvoke(this);

    // last of all, update the scope of this entity
    UpdateScope();

    // and call into late attached
    _serializer.AttachedLate();

    // done!
    //BoltLog.Debug("attached {0}", this);
  }

  internal void UpdateScope () {
    BoltConnection cn;
    var cnIter = BoltCore._connections.GetIterator();

    while (cnIter.Next(out cn)) {
      // if this connection isn't allowed to proxy objects, skip it
      if (cn._flags & BoltConnection.FLAG_LOADING_MAP) { continue; }

      // if proxying is disabled for this object, skip it
      if (this._flags & BoltEntity.FLAG_DISABLE_PROXYING) { continue; }

      // if this object originates from this connection, skip it
      if (ReferenceEquals(this.boltSource, cn)) { continue; }

      // a controlling connection is always considered in scope
      bool scope = this.boltSerializer.InScope(cn) || ReferenceEquals(this._remoteController, cn);
      bool exists = cn._entityChannel.MightExistOnRemote(this);

      // if we DO exists on remote but ARE NOT in scope
      // anymore, we should mark the proxy for deletion
      if (exists && !scope) {
        cn._entityChannel.DestroyOnRemote(this, BoltEntityDestroyMode.OutOfScope);
      }

      // if we DO NOT exist on remote but ARE in scope
      // we should create a new proxy on this connection
      if (!exists && scope) {
        cn._entityChannel.CreateOnRemote(this);
      }
    }
  }

  internal void Detach () {
    if (!_attached) {
      BoltLog.Error("entity is not attached");
      return;
    }

    // notify all clients
    foreach (BoltConnection cn in BoltCore.clients) {
      cn._entityChannel.DestroyOnRemote(this, BoltEntityDestroyMode.LocalDestroy);
    }

    // notify user
    BoltCallbacksBase.EntityDetachedInvoke(this);

    // detached callback on all behaviours
    foreach (BoltEntityBehaviourBase ac in _entityBehaviours) {
      ac.Detached();
    }

    // detached callback on serializer
    _serializer.Detached();

    // remove from entity list
    BoltCore._entities.Remove(this);

    // log!
    //BoltLog.Debug("detached {0}", this);

    // reset
    Reset();
  }

  void Awake () {
    if (_sceneObject) {
      if (BoltCore.isServer == false) {
        // disable this object completely
        gameObject.SetActive(false);

        // destroy it
        Destroy(gameObject);
        return;
      }
    }

    _id = ++_idCounter;
    _updateRate = Mathf.Max(1, _updateRate);

    // passing the parameter to GetComponentsInChildren as true makes sure we get disabled behaviours also
    foreach (BoltEntityBehaviourBase behaviour in GetComponentsInChildren<BoltEntityBehaviourBase>(true)) { 
      if ((behaviour is BoltEntitySerializer) == false) {
        _entityBehaviours.Add(behaviour);
      }
    }
  }

  void Start () {
    if (_sceneObject) {
      if (BoltCore.isServer == false) {
        // disable this object completely
        gameObject.SetActive(false);

        // destroy it
        Destroy(gameObject);
        return;
      }

      BoltCore.Attach(this);
    }
  }

  void Update () {
    _serializer.UpdateRender();
  }

  void ExecuteCommand (BoltCommand cmd, bool resetState) {
    try {
      _serializer.ExecuteCommand(cmd, resetState);

      foreach (BoltEntityBehaviourBase ec in _entityBehaviours) {
        ec.ExecuteCommand(cmd, resetState);
      }
    } finally {
      cmd._hasExecuted = true;
    }
  }

  int UnexecutedCommandCount () {
    int count = 0;

    BoltCommand cmd;
    var cmdIter = _commands.GetIterator();

    while (cmdIter.Next(out cmd)) {
      if (cmd._hasExecuted == false) {
        count += 1;
      }
    }

    return count;
  }

  void Reset () {
    _id = uint.MaxValue;
    _mask = 0;
    _flags = 0;
    _source = null;
    _attached = false;
    _remoteController = null;
  }

  void OnDestroy () {
    if (_attached) {
      BoltLog.Warning("{0} is being destroyed or disabled without being detached, forcing detach", this);

      // force detach here
      Detach();
    }
  }

  void OnDisable () {
    OnDestroy();
  }

  void OnDrawGizmos () {
    Gizmos.DrawIcon(transform.position, "BoltEntity Gizmo", true);
  }

}
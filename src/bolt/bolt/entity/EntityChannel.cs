using Bolt;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

enum BoltEntityDestroyMode {
  OutOfScope,
  LocalDestroy
}

partial class EntityChannel : BoltChannel {
  Dictionary<Bolt.NetworkId, EntityProxy> _outgoing;
  Dictionary<Bolt.NetworkId, EntityProxy> _incomming;
  List<EntityProxy> _prioritized;

  public EntityChannel() {
    _outgoing = new Dictionary<NetworkId, EntityProxy>(2048, Bolt.NetworkId.EqualityComparer.Instance);
    _incomming = new Dictionary<NetworkId, EntityProxy>(2048, Bolt.NetworkId.EqualityComparer.Instance);
    _prioritized = new List<EntityProxy>();
  }

  public void ForceSync(Bolt.Entity en) {
    EntityProxy proxy;
    ForceSync(en, out proxy);
  }

  public void ForceSync(Bolt.Entity en, out EntityProxy proxy) {
    if (_outgoing.TryGetValue(en.NetworkId, out proxy)) {
      proxy.Flags |= Bolt.ProxyFlags.FORCE_SYNC;
      proxy.Flags &= ~Bolt.ProxyFlags.IDLE;
    }
  }

  public void SetIdle(Bolt.Entity entity, bool idle) {
    EntityProxy proxy;

    if (_outgoing.TryGetValue(entity.NetworkId, out proxy)) {
      if (idle) {
        proxy.Flags |= Bolt.ProxyFlags.IDLE;
      }
      else {
        proxy.Flags &= ~Bolt.ProxyFlags.IDLE;
      }
    }
  }

  public void SetScope(Entity entity, bool inScope) {
    if (BoltCore._config.scopeMode == Bolt.ScopeMode.Automatic) {
      BoltLog.Warn("SetScope has no effect when Scope Mode is set to Automatic");
      return;
    }

    if (ReferenceEquals(entity.Source, connection)) {
      return;
    }

    if (inScope) {
      if (_incomming.ContainsKey(entity.NetworkId)) {
        return;
      }

      EntityProxy proxy;

      if (_outgoing.TryGetValue(entity.NetworkId, out proxy)) {
        if (proxy.Flags & Bolt.ProxyFlags.DESTROY_REQUESTED) {
          if (proxy.Flags & Bolt.ProxyFlags.DESTROY_PENDING) {
            proxy.Flags |= Bolt.ProxyFlags.DESTROY_IGNORE;
          }
          else {
            proxy.Flags &= ~Bolt.ProxyFlags.DESTROY_IGNORE;
            proxy.Flags &= ~Bolt.ProxyFlags.DESTROY_REQUESTED;
          }
        }
      }
      else {
        CreateOnRemote(entity);
      }
    }
    else {
      if (_outgoing.ContainsKey(entity.NetworkId)) {
        DestroyOnRemote(entity, BoltEntityDestroyMode.OutOfScope);
      }
    }
  }

  public bool ExistsOnRemote(Entity entity) {
    if (entity == null) { return false; }
    if (_incomming.ContainsKey(entity.NetworkId)) { return true; }

    EntityProxy proxy;

    if (_outgoing.TryGetValue(entity.NetworkId, out proxy)) {
      return (proxy.Flags & ProxyFlags.CREATE_DONE) && !(proxy.Flags & ProxyFlags.DESTROY_REQUESTED);
    }

    return false;
  }

  public bool MightExistOnRemote(Bolt.Entity entity) {
    return _incomming.ContainsKey(entity.NetworkId) || _outgoing.ContainsKey(entity.NetworkId);
  }

  public void DestroyOnRemote(Bolt.Entity entity, BoltEntityDestroyMode mode) {
    EntityProxy proxy;

    if (_outgoing.TryGetValue(entity.NetworkId, out proxy)) {
      // if we dont have any pending sends for this and we have not created it;
      if (proxy.Envelopes.Count == 0 && !(proxy.Flags & ProxyFlags.CREATE_DONE)) {
        DestroyOutgoingProxy(proxy);

      }
      else {
        proxy.Flags |= ProxyFlags.DESTROY_REQUESTED;
        proxy.Flags &= ~ProxyFlags.IDLE;
      }
    }
  }

  public void CreateOnRemote(Bolt.Entity entity) {
    EntityProxy proxy;
    CreateOnRemote(entity, out proxy);
  }

  public void CreateOnRemote(Bolt.Entity entity, out EntityProxy proxy) {
    if (_incomming.TryGetValue(entity.NetworkId, out proxy)) { return; }
    if (_outgoing.TryGetValue(entity.NetworkId, out proxy)) { return; }

    proxy = entity.CreateProxy();
    proxy.NetworkId = entity.NetworkId;
    proxy.Flags = ProxyFlags.CREATE_REQUESTED;
    proxy.Filter = new Filter(1);
    proxy.Connection = connection;

    _outgoing.Add(proxy.NetworkId, proxy);

    BoltLog.Debug("Created {0} on {1}", entity, connection);
  }

  public override void StepRemoteFrame() {
    foreach (EntityProxy proxy in _incomming.Values) {
      // skip ones we are in control of and that are client predicted
      if (proxy.Entity.HasPredictedControl) {
        continue;
      }

      // simulate this entity
      proxy.Entity.Simulate();
    }
  }

  public override void Pack(BoltPacket packet) {
    int startPos = packet.stream.Position;

    foreach (EntityProxy proxy in _outgoing.Values) {
      if (proxy.Flags & ProxyFlags.DESTROY_REQUESTED) {
        if (proxy.Flags & ProxyFlags.DESTROY_PENDING) {
          continue;
        }

        proxy.Changed.ClearAll();
        proxy.Priority = 1 << 17;
      }
      else {
        // check update rate of this entity
        if ((packet.number % proxy.Entity.UpdateRate) != 0) {
          continue;
        }

        // meep
        if (proxy.Envelopes.Count >= 256) {
          BoltLog.Error("Envelopes for {0} to {1} full", proxy, connection);
          continue;
        }

        // if this connection is loading a map dont send any creates or state updates
        if (connection.isLoadingMap || BoltSceneLoader.IsLoading || !connection._canReceiveEntities) {
          continue;
        }

        if (proxy.Flags & ProxyFlags.FORCE_SYNC) {
          proxy.Priority = 1 << 19;

        }
        else {
          if ((proxy.Flags & ProxyFlags.CREATE_DONE) || (proxy.Envelopes.Count > 0)) {
            if (proxy.Changed.IsZero) {
              continue;
            }

            if (proxy.Entity.PriorityCalculator.Always) {
              proxy.Priority = 1 << 20;
            }
            else {
              proxy.Priority = proxy.Priority + proxy.Entity.PriorityCalculator.CalculateStatePriority(connection, proxy.Skipped);
              proxy.Priority = Mathf.Clamp(proxy.Priority, 0, 1 << 16);
            }
          }
          else {
            if (proxy.Entity.PriorityCalculator.Always) {
              proxy.Priority = 1 << 20;
            }
            else {
              proxy.Priority = 1 << 18;
            }
          }
        }
      }

      // if this is the controller give it the max priority
      if (proxy.Entity.IsController(connection)) {
        proxy.Priority = 1 << 21;
      }

      _prioritized.Add(proxy);
    }
    if (_prioritized.Count > 0) {
      try {
        _prioritized.Sort(EntityProxy.PriorityComparer.Instance);

        // write as many proxies into the packet as possible

        int failCount = 0;

        for (int i = 0; i < _prioritized.Count; ++i) {
          if (failCount >= 2) {
            _prioritized[i].Skipped += 1;
          }
          else {
            var result = PackUpdate(packet, _prioritized[i]);
            if (result) {
              _prioritized[i].Priority = 0;
            }
            else {
              failCount += 1;
            }
          }
        }
      }
      finally {
        _prioritized.Clear();
      }
    }

    packet.stream.WriteStopMarker();
    packet.stats.StateBits = packet.stream.Position - startPos;
  }

  public override void Read(BoltPacket packet) {
    int startPtr = packet.stream.Position;

    // unpack all of our data
    while (packet.stream.CanRead()) {
      if (ReadUpdate(packet) == false) {
        break;
      }
    }

    packet.stats.StateBits = packet.stream.Position - startPtr;
  }

  public override void Lost(BoltPacket packet) {
    while (packet.ProxyEnvelopes.Count > 0) {
      var env = packet.ProxyEnvelopes.Dequeue();
      var pending = env.Proxy.Envelopes.Dequeue();

      //BoltLog.Error("LOST ENV {0}, IN TRANSIT: {1}", env.Proxy, env.Proxy.Envelopes.Count);
      //Assert.Same(env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId], string.Format("PROXY MISS-MATCH {0} <> {1}", env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId]));
      //Assert.Same(env, pending, string.Format("ENVELOPE MISS-MATCH {0} <> {1}", env.PacketNumber, pending.PacketNumber));

      // copy back all priorities
      ApplyPropertyPriorities(env);

      // push skipped count up one
      env.Proxy.Skipped += 1;

      // if this was a forced sync, set flag on proxy again
      if (env.Flags & ProxyFlags.FORCE_SYNC) {
        env.Proxy.Flags |= ProxyFlags.FORCE_SYNC;
      }

      // if we failed to destroy this clear destroying flag
      if (env.Flags & ProxyFlags.DESTROY_PENDING) {
        Assert.True(env.Proxy.Flags & ProxyFlags.DESTROY_PENDING);
        env.Proxy.Flags &= ~ProxyFlags.DESTROY_PENDING;
      }

      env.Dispose();
    }
  }

  public override void Delivered(BoltPacket packet) {
    while (packet.ProxyEnvelopes.Count > 0) {
      var env = packet.ProxyEnvelopes.Dequeue();
      var pending = env.Proxy.Envelopes.Dequeue();

      //BoltLog.Info("DELIVERED ENV {0}, IN TRANSIT: {1}", env.Proxy, env.Proxy.Envelopes.Count);
      //Assert.Same(env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId], string.Format("PROXY MISS-MATCH {0} <> {1}", env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId]));
      //Assert.Same(env, pending, string.Format("ENVELOPE MISS-MATCH {0} <> {1}", env.PacketNumber, pending.PacketNumber));

      if (env.Flags & ProxyFlags.DESTROY_PENDING) {
        Assert.True(env.Proxy.Flags & ProxyFlags.DESTROY_PENDING);

        // delete proxy
        DestroyOutgoingProxy(env.Proxy);
      }
      else if (env.Flags & ProxyFlags.CREATE_REQUESTED) {
        // if this token has been sent, clear it
        if (ReferenceEquals(env.ControlTokenGained, env.Proxy.ControlTokenGained)) {
          env.Proxy.ControlTokenGained = null;
        }

        // clear out request / progress for create
        env.Proxy.Flags &= ~ProxyFlags.CREATE_REQUESTED;

        // set create done
        env.Proxy.Flags |= ProxyFlags.CREATE_DONE;
      }

      env.Dispose();
    }
  }

  public override void Disconnected() {
    foreach (EntityProxy proxy in _outgoing.Values.ToArray()) {
      if (proxy) {
        DestroyOutgoingProxy(proxy);
      }
    }

    foreach (EntityProxy proxy in _incomming.Values.ToArray()) {
      if (proxy) {
        DestroyIncommingProxy(proxy, null);
      }
    }
  }

  public int GetSkippedUpdates(Entity en) {
    EntityProxy proxy;

    if (_outgoing.TryGetValue(en.NetworkId, out proxy)) {
      return proxy.Skipped;
    }

    return -1;
  }

  void ApplyPropertyPriorities(EntityProxyEnvelope env) {
    for (int i = 0; i < env.Written.Count; ++i) {
      Priority p = env.Written[i];

      // set flag for sending this property again
      env.Proxy.Changed.Set(p.PropertyIndex);

      // increment priority
      env.Proxy.PropertyPriority[p.PropertyIndex].PriorityValue += p.PriorityValue;
    }
  }

  bool PackUpdate(BoltPacket packet, EntityProxy proxy) {
    int pos = packet.stream.Position;
    int packCount = 0;

    EntityProxyEnvelope env = proxy.CreateEnvelope();

    packet.stream.WriteBool(true);
    packet.stream.WriteNetworkId(proxy.NetworkId);

    if (packet.stream.WriteBool(proxy.Entity.IsController(connection))) {
      packet.stream.WriteToken(proxy.ControlTokenGained);
      proxy.ControlTokenLost = null;
    }
    else {
      packet.stream.WriteToken(proxy.ControlTokenLost);
      proxy.ControlTokenGained = null;
    }

    if (packet.stream.WriteBool(proxy.Flags & ProxyFlags.DESTROY_REQUESTED)) {
      packet.stream.WriteToken(proxy.Entity.DetachToken);
    }
    else {
      // data for first packet
      if (packet.stream.WriteBool(proxy.Flags & ProxyFlags.CREATE_REQUESTED)) {
        packet.stream.WriteToken(proxy.Entity.AttachToken);

        packet.stream.WritePrefabId(proxy.Entity.PrefabId);
        packet.stream.WriteTypeId(proxy.Entity.Serializer.TypeId);

        packet.stream.WriteVector3(proxy.Entity.UnityObject.transform.position);
        packet.stream.WriteQuaternion(proxy.Entity.UnityObject.transform.rotation);

        if (packet.stream.WriteBool(proxy.Entity.IsSceneObject)) {
          Assert.False(proxy.Entity.SceneId.IsNone);
          packet.stream.WriteUniqueId(proxy.Entity.SceneId);
        }
      }

      packCount = proxy.Entity.Serializer.Pack(connection, packet.stream, env);
    }

    if (packet.stream.Overflowing) {
      packet.stream.Position = pos;
      return false;
    }
    if (packCount == -1) {
      packet.stream.Position = pos;
      return true;
    }
    else {
      var isForce = proxy.Flags & ProxyFlags.FORCE_SYNC;
      var isCreate = proxy.Flags & ProxyFlags.CREATE_REQUESTED;
      var isDestroy = proxy.Flags & ProxyFlags.DESTROY_REQUESTED;

      // if we didn't pack anything and we are not creating or destroying this, just goto next
      if ((packCount == 0) && !isCreate && !isDestroy && !isForce) {
        packet.stream.Position = pos;
        return true;
      }

      // set in progress flags
      if (isDestroy) { env.Flags = (proxy.Flags |= ProxyFlags.DESTROY_PENDING); }

      // clear force sync flag
      proxy.Flags &= ~ProxyFlags.FORCE_SYNC;

      // clear skipped count
      proxy.Skipped = 0;

      // set packet number
      env.PacketNumber = packet.number;

      // put on packets list
      packet.ProxyEnvelopes.Enqueue(env);

      // put on proxies pending queue
      // BoltLog.Info("adding envelope to {0}, count: {1}", proxy, proxy.Envelopes.Count + 1);
      proxy.Envelopes.Enqueue(env);

      // keep going!
      return true;
    }
  }

  bool ReadUpdate(BoltPacket packet) {
    if (packet.stream.ReadBool() == false) {
      return false;
    }

    // grab networkid
    NetworkId networkId = packet.stream.ReadNetworkId();
    bool isController = packet.stream.ReadBool();
    IProtocolToken controlToken = packet.stream.ReadToken();
    bool destroyRequested = packet.stream.ReadBool();

    // we're destroying this proxy
    if (destroyRequested) {
      EntityProxy proxy;
      IProtocolToken detachToken = packet.stream.ReadToken();

      if (_incomming.TryGetValue(networkId, out proxy)) {
        if (proxy.Entity.HasControl) {
          proxy.Entity.ReleaseControlInternal(controlToken);
        }

        DestroyIncommingProxy(proxy, detachToken);
      }
      else {
        BoltLog.Warn("Received destroy of {0} but no such proxy was found", networkId);
      }
    }
    else {
      IProtocolToken attachToken = null;

      bool isSceneObject = false;
      bool createRequested = packet.stream.ReadBool();

      UniqueId sceneId = UniqueId.None;
      PrefabId prefabId = new PrefabId();
      TypeId serializerId = new TypeId();
      Vector3 spawnPosition = new Vector3();
      Quaternion spawnRotation = new Quaternion();

      if (createRequested) {
        attachToken = packet.stream.ReadToken();
        prefabId = packet.stream.ReadPrefabId();
        serializerId = packet.stream.ReadTypeId();
        spawnPosition = packet.stream.ReadVector3();
        spawnRotation = packet.stream.ReadQuaternion();
        isSceneObject = packet.stream.ReadBool();

        if (isSceneObject) {
          sceneId = packet.stream.ReadUniqueId();
        }
      }

      Entity entity = null;
      EntityProxy proxy = null;

      if (createRequested && (_incomming.ContainsKey(networkId) == false)) {
        // prefab checks (if applicable)
        {
          GameObject go = BoltCore.PrefabPool.LoadPrefab(prefabId);

          if (go) {
            if (BoltCore.isServer && !go.GetComponent<BoltEntity>()._allowInstantiateOnClient) {
              throw new BoltException("Received entity of prefab {0} from client at {1}, but this entity is not allowed to be instantiated from clients", go.name, connection.remoteEndPoint);
            }
          }
        }

        // create entity

        if (isSceneObject) {
          GameObject go = BoltCore.FindSceneObject(sceneId);

          if (!go) {
            go = BoltCore.PrefabPool.Instantiate(prefabId, spawnPosition, spawnRotation);
          }

          entity = Entity.CreateFor(go, prefabId, serializerId, EntityFlags.SCENE_OBJECT);
        }
        else {
          entity = Entity.CreateFor(prefabId, serializerId, spawnPosition, spawnRotation);
        }

        entity.Source = connection;
        entity.SceneId = sceneId;
        entity.NetworkId = networkId;

        // handle case where we are given control (it needs to be true during the initialize, read and attached callbacks)
        if (isController) {
          entity.Flags |= EntityFlags.HAS_CONTROL;
        }

        // initialize entity
        entity.Initialize();

        // create proxy
        proxy = entity.CreateProxy();
        proxy.NetworkId = networkId;
        proxy.Connection = connection;

        // register proxy
        _incomming.Add(proxy.NetworkId, proxy);

        // read packet
        entity.Serializer.Read(connection, packet.stream, packet.frame);

        // attach entity
        proxy.Entity.AttachToken = attachToken;
        proxy.Entity.Attach();

        // assign control properly
        if (isController) {
          proxy.Entity.Flags &= ~EntityFlags.HAS_CONTROL;
          proxy.Entity.TakeControlInternal(controlToken);
        }

        // log debug info
        BoltLog.Debug("Received {0} from {1}", entity, connection);

        // notify user
        BoltInternal.GlobalEventListenerBase.EntityReceivedInvoke(proxy.Entity.UnityObject);
      }
      else {
        // find proxy
        proxy = _incomming[networkId];

        if (proxy == null) {
          throw new BoltException("Couldn't find entity for {0}", networkId);
        }

        // update control state yes/no
        if (proxy.Entity.HasControl ^ isController) {
          if (isController) {
            proxy.Entity.TakeControlInternal(controlToken);
          }
          else {
            proxy.Entity.ReleaseControlInternal(controlToken);
          }
        }

        // read update
        proxy.Entity.Serializer.Read(connection, packet.stream, packet.frame);
      }

    }

    return true;
  }

  void DestroyOutgoingProxy(EntityProxy proxy) {
    // remove outgoing proxy index
    _outgoing.Remove(proxy.NetworkId);

    // remove proxy from entity
    if (proxy.Entity && proxy.Entity.IsAttached) {
      proxy.Entity.Proxies.Remove(proxy);
    }

    // if we're told to ignore this destroy, instantly re-create the proxy
    if (proxy.Flags & ProxyFlags.DESTROY_IGNORE) {
      CreateOnRemote(proxy.Entity);
    }
  }

  void DestroyIncommingProxy(EntityProxy proxy, IProtocolToken token) {
    // remove incomming proxy
    _incomming.Remove(proxy.NetworkId);

    // destroy entity
    proxy.Entity.DetachToken = token;

    // destroy entity
    BoltCore.DestroyForce(proxy.Entity);
  }
}

using Bolt;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

enum BoltEntityDestroyMode {
  OutOfScope,
  LocalDestroy
}

partial class EntityChannel : BoltChannel {
  NetIdPool _outgoingProxiesNetworkIdPool;
  EntityProxy[] _outgoingProxiesByPriority;

  Dictionary<Bolt.NetId, EntityProxy> _outgoingProxiesByNetId;
  Dictionary<Bolt.InstanceId, EntityProxy> _outgoingProxiesByInstanceId;

  Dictionary<Bolt.NetId, EntityProxy> _incommingProxiesByNetId;
  Dictionary<Bolt.InstanceId, EntityProxy> _incommingProxiesByInstanceId;

  public EntityChannel() {
    _outgoingProxiesNetworkIdPool = new NetIdPool(EntityProxy.MAX_COUNT);
    _outgoingProxiesByPriority = new EntityProxy[EntityProxy.MAX_COUNT];

    _outgoingProxiesByNetId = new Dictionary<Bolt.NetId, EntityProxy>(1024, Bolt.NetId.EqualityComparer.Instance);
    _outgoingProxiesByInstanceId = new Dictionary<Bolt.InstanceId, EntityProxy>(1024, Bolt.InstanceId.EqualityComparer.Instance);

    _incommingProxiesByNetId = new Dictionary<Bolt.NetId, EntityProxy>(1024, Bolt.NetId.EqualityComparer.Instance);
    _incommingProxiesByInstanceId = new Dictionary<Bolt.InstanceId, EntityProxy>(1024, Bolt.InstanceId.EqualityComparer.Instance);
  }

  public Bolt.Entity GetIncommingEntity(Bolt.NetId netId) {
    if (_incommingProxiesByNetId.ContainsKey(netId)) {
      return _incommingProxiesByNetId[netId].Entity;
    }

    BoltLog.Warn("Unknown {0}", netId);
    return null;
  }

  public Bolt.Entity GetOutgoingEntity(Bolt.NetId netId) {
    if (_outgoingProxiesByNetId.ContainsKey(netId)) {
      return _outgoingProxiesByNetId[netId].Entity;
    }

    BoltLog.Warn("Unknown {0}", netId);
    return null;
  }

  public void ForceSync(Bolt.Entity en) {
    EntityProxy proxy;

    if (_outgoingProxiesByInstanceId.TryGetValue(en.InstanceId, out proxy)) {
      proxy.Flags |= Bolt.ProxyFlags.FORCE_SYNC;
    }
  }

  public void SetIdle(Bolt.Entity entity, bool idle) {
    EntityProxy proxy;

    if (_outgoingProxiesByInstanceId.TryGetValue(entity.InstanceId, out proxy)) {
      if (idle) {
        proxy.Flags |= Bolt.ProxyFlags.FORCE_SYNC;
      }
      else {
        proxy.Flags &= ~Bolt.ProxyFlags.FORCE_SYNC;
      }
    }
  }

  public void SetScope(Entity entity, bool inScope) {
    if (BoltCore._config.scopeMode == Bolt.ScopeMode.Automatic) {
      BoltLog.Warn("SetScope has no effect when Scope Mode is set to Automatic");
      return;
    }

    if (inScope) {
      if (_incommingProxiesByInstanceId.ContainsKey(entity.InstanceId)) {
        return;
      }

      EntityProxy proxy;

      if (_outgoingProxiesByInstanceId.TryGetValue(entity.InstanceId, out proxy)) {
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
      if (_outgoingProxiesByInstanceId.ContainsKey(entity.InstanceId)) {
        DestroyOnRemote(entity, BoltEntityDestroyMode.OutOfScope);
      }
    }
  }

  public NetId GetNetworkId(Entity entity) {
    EntityProxy proxy;

    if (_outgoingProxiesByInstanceId.TryGetValue(entity.InstanceId, out proxy)) { return proxy.NetId; }
    if (_incommingProxiesByInstanceId.TryGetValue(entity.InstanceId, out proxy)) { return proxy.NetId; }

    return new NetId(int.MaxValue);
  }

  public bool ExistsOnRemote(Entity entity) {
    if (entity == null) { return false; }
    if (_incommingProxiesByInstanceId.ContainsKey(entity.InstanceId)) { return true; }

    EntityProxy proxy;

    if (_outgoingProxiesByInstanceId.TryGetValue(entity.InstanceId, out proxy)) {
      return (proxy.Flags & ProxyFlags.CREATE_DONE) && !(proxy.Flags & ProxyFlags.DESTROY_REQUESTED);
    }

    return false;
  }

  public bool MightExistOnRemote(Bolt.Entity entity) {
    return _incommingProxiesByInstanceId.ContainsKey(entity.InstanceId) || _outgoingProxiesByInstanceId.ContainsKey(entity.InstanceId);
  }

  public void DestroyOnRemote(Bolt.Entity entity, BoltEntityDestroyMode mode) {
    EntityProxy proxy;

    if (_outgoingProxiesByInstanceId.TryGetValue(entity.InstanceId, out proxy)) {
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

  public bool CreateOnRemote(Bolt.Entity entity) {
    if (_incommingProxiesByInstanceId.ContainsKey(entity.InstanceId)) { return true; }
    if (_outgoingProxiesByInstanceId.ContainsKey(entity.InstanceId)) { return true; }

    NetId id;

    if (_outgoingProxiesNetworkIdPool.Acquire(out id) == false) {
      BoltLog.Warn("{0} is already proxying the max amount of objects", connection);
      return false;
    }

    EntityProxy proxy;
    proxy = entity.CreateProxy();
    proxy.NetId = id;
    proxy.Flags = ProxyFlags.CREATE_REQUESTED;
    proxy.Filter = new Filter(1);
    proxy.Connection = connection;

    _outgoingProxiesByNetId[proxy.NetId] = proxy;
    _outgoingProxiesByInstanceId[entity.InstanceId] = proxy;

    BoltLog.Debug("Created {0} on {1}", proxy, connection);
    return true;
  }

  public override void StepRemoteFrame() {
    foreach (EntityProxy proxy in _incommingProxiesByInstanceId.Values) {
      // skip ones we are in control of and that are client predicted
      if (proxy.Entity.HasPredictedControl) {
        continue;
      }

      // simulate this entity
      proxy.Entity.Simulate();
    }
  }

  public float GetPriority(Entity entity) {
    EntityProxy proxy;

    if (_outgoingProxiesByInstanceId.TryGetValue(entity.InstanceId, out proxy)) {
      return proxy.Priority;
    }

    return 0f;
  }

  public override void Pack(BoltPacket packet) {
    int n = 0;
    int startPos = packet.stream.Position;

    foreach (EntityProxy proxy in _outgoingProxiesByInstanceId.Values) {
      if (proxy.Flags & ProxyFlags.DESTROY_REQUESTED) {
        if (proxy.Flags & ProxyFlags.DESTROY_PENDING) {
          continue;
        }

        proxy.Mask.Clear();
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
            var noDataToSend = proxy.Mask.AndCheck(proxy.Entity.Serializer.GetFilter(connection, proxy)) == false;
            if (noDataToSend) {
              continue;
            }

            proxy.Priority = proxy.Priority + proxy.Entity.PriorityCalculator.CalculateStatePriority(connection, proxy.Mask, proxy.Skipped);
            proxy.Priority = Mathf.Clamp(proxy.Priority, 0, 1 << 16);
          }
          else {
            proxy.Priority = 1 << 18;
          }
        }
      }

      // if this is the controller give it the max priority
      if (proxy.Entity.IsController(connection)) {
        proxy.Priority = 1 << 20;
      }

      // push
      _outgoingProxiesByPriority[n++] = proxy;
    }

    if (n > 0) {
      Assert.True(_outgoingProxiesByPriority.Take(n).GroupBy(x => x.NetId.Value).All(x => x.Count() == 1));

      // only if we have any proxies to sort
      // sort proxies by their priority (highest to lowest)
      Array.Sort(_outgoingProxiesByPriority, 0, n, EntityProxy.PriorityComparer.Instance);

      // write as many proxies into the packet as possible

      int failCount = 0;

      for (int i = 0; i < n; ++i) {
        if (failCount >= 2) {
          _outgoingProxiesByPriority[i].Skipped += 1;
        }
        else {
          var result = PackUpdate(packet, _outgoingProxiesByPriority[i]);
          if (result) {
            _outgoingProxiesByPriority[i].Priority = 0f;
          }
          else {
            failCount += 1;
          }
        }
      }

      // clear priority array
      Array.Clear(_outgoingProxiesByPriority, 0, _outgoingProxiesByPriority.Length);
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
      Assert.Same(env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId], string.Format("PROXY MISS-MATCH {0} <> {1}", env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId]));
      Assert.Same(env, pending, string.Format("ENVELOPE MISS-MATCH {0} <> {1}", env.PacketNumber, pending.PacketNumber));

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
      Assert.Same(env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId], string.Format("PROXY MISS-MATCH {0} <> {1}", env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId]));
      Assert.Same(env, pending, string.Format("ENVELOPE MISS-MATCH {0} <> {1}", env.PacketNumber, pending.PacketNumber));

      if (env.Flags & ProxyFlags.DESTROY_PENDING) {
        Assert.True(env.Proxy.Flags & ProxyFlags.DESTROY_PENDING);

        // delete proxy
        DestroyOutgoingProxy(env.Proxy);
      }
      else if (env.Flags & ProxyFlags.CREATE_REQUESTED) {
        // clear out request / progress for create
        env.Proxy.Flags &= ~ProxyFlags.CREATE_REQUESTED;

        // set create done
        env.Proxy.Flags |= ProxyFlags.CREATE_DONE;
      }

      env.Dispose();
    }
  }

  public override void Disconnected() {
    foreach (EntityProxy proxy in _outgoingProxiesByInstanceId.Values.ToArray()) {
      if (proxy) {
        DestroyOutgoingProxy(proxy);
      }
    }

    foreach (EntityProxy proxy in _incommingProxiesByNetId.Values.ToArray()) {
      if (proxy) {
        DestroyIncommingProxy(proxy);
      }
    }
  }

  public int GetSkippedUpdates(Entity en) {
    EntityProxy proxy;

    if (_outgoingProxiesByInstanceId.TryGetValue(en.InstanceId, out proxy)) {
      return proxy.Skipped;
    }

    return -1;
  }

  void ApplyPropertyPriorities(EntityProxyEnvelope env) {
    for (int i = 0; i < env.Written.Count; ++i) {
      Priority p = env.Written[i];

      // set flag for sending this property again
      env.Proxy.Mask.Set(p.PropertyIndex);

      // increment priority
      env.Proxy.PropertyPriority[p.PropertyIndex].PriorityValue += p.PriorityValue;
    }
  }

  bool PackUpdate(BoltPacket packet, EntityProxy proxy) {
    int pos = packet.stream.Position;
    int packCount = 0;

    EntityProxyEnvelope env = proxy.CreateEnvelope();

    packet.stream.WriteBool(true);
    packet.stream.WriteNetworkId(proxy.NetId);

    if (packet.stream.WriteBool(proxy.Flags & ProxyFlags.DESTROY_REQUESTED) == false) {
      // if the remote is the controller or not
      packet.stream.WriteBool(ReferenceEquals(proxy.Entity.Controller, connection));

      // data for first packet
      if (packet.stream.WriteBool(proxy.Flags & ProxyFlags.CREATE_REQUESTED)) {
        proxy.Entity.PrefabId.Pack(packet.stream, 32);
        proxy.Entity.Serializer.TypeId.Pack(packet.stream, 32);

        packet.stream.WriteVector3(proxy.Entity.UnityObject.transform.position);
        packet.stream.WriteQuaternion(proxy.Entity.UnityObject.transform.rotation);

        if (packet.stream.WriteBool(proxy.Entity.IsSceneObject)) {
          Assert.False(proxy.Entity.UniqueId.IsNone);
        }

        if (packet.stream.WriteBool(proxy.Entity.UniqueId.IsNone == false)) {
          proxy.Entity.UniqueId.Pack(packet.stream);
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
      var isCreate = proxy.Flags & ProxyFlags.CREATE_REQUESTED;
      var isDestroy = proxy.Flags & ProxyFlags.DESTROY_REQUESTED;

      // if we didn't pack anything and we are not creating or destroying this, just goto next
      if ((packCount == 0) && !isCreate && !isDestroy) {
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
    if (packet.stream.ReadBool() == false)
      return false;

    // grab networkid
    var netId = packet.stream.ReadNetworkId();
    var destroyRequested = packet.stream.ReadBool();

    // we're destroying this proxy
    if (destroyRequested) {
      EntityProxy proxy;

      if (_incommingProxiesByNetId.TryGetValue(netId, out proxy)) {
        if (proxy.Entity.HasControl) {
          proxy.Entity.ReleaseControlInternal();
        }

        DestroyIncommingProxy(proxy);
      }
      else {
        BoltLog.Warn("Received destroy of {0} but no such proxy was found", netId);
      }
    }
    else {
      bool isController = packet.stream.ReadBool();
      bool isSceneObject = false;
      bool createRequested = packet.stream.ReadBool();

      PrefabId prefabId = new PrefabId();
      TypeId serializerId = new TypeId();
      Vector3 spawnPosition = new Vector3();
      Quaternion spawnRotation = new Quaternion();
      UniqueId uniqueId = UniqueId.None;

      if (createRequested) {
        prefabId = PrefabId.Read(packet.stream, 32);
        serializerId = TypeId.Read(packet.stream, 32);
        spawnPosition = packet.stream.ReadVector3();
        spawnRotation = packet.stream.ReadQuaternion();
        isSceneObject = packet.stream.ReadBool();

        if (packet.stream.ReadBool()) {
          uniqueId = UniqueId.Read(packet.stream);
        }
      }

      Entity entity = null;
      EntityProxy proxy = null;

      if (createRequested && (_incommingProxiesByNetId.ContainsKey(netId) == false)) {
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
          GameObject go = BoltCore.FindSceneObject(uniqueId);

          if (!go) {
            go = BoltCore.PrefabPool.Instantiate(prefabId, spawnPosition, spawnRotation);
          }

          entity = Entity.CreateFor(go, prefabId, serializerId, EntityFlags.SCENE_OBJECT);
        }
        else {
          entity = Entity.CreateFor(prefabId, serializerId, spawnPosition, spawnRotation);
        }

        entity.Source = connection;
        entity.UniqueId = uniqueId;

        // handle case where we are given control (it needs to be true during the initialize, read and attached callbacks)
        if (isController) {
          entity.Flags |= EntityFlags.HAS_CONTROL;
        }

        // initialize entity
        entity.Initialize();

        // create proxy
        proxy = entity.CreateProxy();
        proxy.NetId = netId;
        proxy.Connection = connection;

        // register proxy
        _incommingProxiesByNetId.Add(netId, proxy);
        _incommingProxiesByInstanceId.Add(proxy.Entity.InstanceId, proxy);

        // read packet
        entity.Serializer.Read(connection, packet.stream, packet.frame);

        // attach entity
        proxy.Entity.Attach();

        // again for the given control case, we need to clear out the HAS_CONTROL flag or .TakeControl will complain
        if (isController) {
          proxy.Entity.Flags &= ~EntityFlags.HAS_CONTROL;
          proxy.Entity.TakeControlInternal();
        }

        // log debug info
        BoltLog.Debug("Received {0} from {1}", entity, connection);

        // notify user
        BoltInternal.GlobalEventListenerBase.EntityReceivedInvoke(proxy.Entity.UnityObject);
      }
      else {
        // find proxy
        proxy = _incommingProxiesByNetId[netId];

        if (proxy == null) {
          throw new BoltException("couldn't find proxy with id {0}", netId);
        }

        // update control state yes/no
        if (proxy.Entity.HasControl ^ isController) {
          if (isController) {
            proxy.Entity.TakeControlInternal();
          }
          else {
            proxy.Entity.ReleaseControlInternal();
          }
        }

        // read update
        proxy.Entity.Serializer.Read(connection, packet.stream, packet.frame);
      }

    }

    return true;
  }

  void DestroyOutgoingProxy(EntityProxy proxy) {
    _outgoingProxiesByNetId.Remove(proxy.NetId);
    _outgoingProxiesNetworkIdPool.Release(proxy.NetId);
    _outgoingProxiesByInstanceId.Remove(proxy.Entity.InstanceId);

    if (proxy.Flags & ProxyFlags.DESTROY_IGNORE) {
      CreateOnRemote(proxy.Entity);
    }
  }

  void DestroyIncommingProxy(EntityProxy proxy) {
    _incommingProxiesByInstanceId.Remove(proxy.Entity.InstanceId);
    _incommingProxiesByNetId.Remove(proxy.NetId);

    // destroy entity
    BoltCore.DestroyForce(proxy.Entity);
  }
}

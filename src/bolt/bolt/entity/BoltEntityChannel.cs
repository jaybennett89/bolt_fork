using Bolt;
using System;
using System.Collections.Generic;
using UnityEngine;

enum BoltEntityDestroyMode {
  OutOfScope,
  LocalDestroy
}

partial class BoltEntityChannel : BoltChannel {
  NetIdPool _outgoingProxiesNetworkIdPool;
  EntityProxy[] _outgoingProxiesByPriority;

  Dictionary<Bolt.NetId, EntityProxy> _outgoingProxiesByNetId;
  Dictionary<Bolt.InstanceId, EntityProxy> _outgoingProxiesByInstanceId;

  Dictionary<Bolt.NetId, EntityProxy> _incommingProxiesByNetId;
  Dictionary<Bolt.InstanceId, EntityProxy> _incommingProxiesByInstanceId;

  public BoltEntityChannel() {
    _outgoingProxiesNetworkIdPool = new NetIdPool(EntityProxy.MAX_COUNT);
    _outgoingProxiesByPriority = new EntityProxy[EntityProxy.MAX_COUNT];

    _outgoingProxiesByNetId = new Dictionary<Bolt.NetId, EntityProxy>(1024, Bolt.NetId.EqualityComparer.Instance);
    _outgoingProxiesByInstanceId = new Dictionary<Bolt.InstanceId, EntityProxy>(1024, Bolt.InstanceId.EqualityComparer.Instance);

    _incommingProxiesByNetId = new Dictionary<Bolt.NetId, EntityProxy>(1024, Bolt.NetId.EqualityComparer.Instance);
    _incommingProxiesByInstanceId = new Dictionary<Bolt.InstanceId, EntityProxy>(1024, Bolt.InstanceId.EqualityComparer.Instance);
  }

  public Bolt.EntityObject GetIncommingEntity(Bolt.NetId proxyId) {
    if (_incommingProxiesByNetId[proxyId] != null) {
      return _incommingProxiesByNetId[proxyId].Entity;
    }

    return null;
  }

  public Bolt.EntityObject GetOutgoingEntity(Bolt.NetId proxyId) {
    if (_incommingProxiesByNetId[proxyId] != null) {
      return _incommingProxiesByNetId[proxyId].Entity;
    }

    return null;
  }

  public void ForceSync(Bolt.EntityObject en) {
    EntityProxy proxy;

    if (_outgoingProxiesByInstanceId.TryGetValue(en.InstanceId, out proxy)) {
      proxy.Flags |= Bolt.ProxyFlags.FORCE_SYNC;
    }
  }

  public void SetIdle(Bolt.EntityObject entity, bool idle) {
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

  public NetId GetNetworkId(Bolt.EntityObject entity) {
    EntityProxy proxy;

    if (_outgoingProxiesByInstanceId.TryGetValue(entity.InstanceId, out proxy)) { return proxy.NetId; }
    if (_incommingProxiesByInstanceId.TryGetValue(entity.InstanceId, out proxy)) { return proxy.NetId; }

    return new NetId(int.MaxValue);
  }

  public bool ExistsOnRemote(Bolt.EntityObject entity) {
    if (_incommingProxiesByInstanceId.ContainsKey(entity.InstanceId)) { return true; }

    EntityProxy proxy;

    if (_outgoingProxiesByInstanceId.TryGetValue(entity.InstanceId, out proxy)) {
      return
        (proxy.Flags & ProxyFlags.CREATE_DONE) &&
        !(proxy.Flags & ProxyFlags.DESTROY_REQUESTED) &&
        !(proxy.Flags & ProxyFlags.DESTROY_IN_PROGRESS) &&
        !(proxy.Flags & ProxyFlags.DESTROY_DONE);
    }

    return false;
  }

  public bool MightExistOnRemote(Bolt.EntityObject entity) {
    return _incommingProxiesByInstanceId.ContainsKey(entity.InstanceId) || _outgoingProxiesByInstanceId.ContainsKey(entity.InstanceId);
  }

  public void DestroyOnRemote(Bolt.EntityObject entity, BoltEntityDestroyMode mode) {
    EntityProxy proxy;

    if (_outgoingProxiesByInstanceId.TryGetValue(entity.InstanceId, out proxy)) {
      // remove lookup by instance id
      _outgoingProxiesByInstanceId.Remove(entity.InstanceId);

      // clear entity from proxy
      proxy.Entity = null;

      if ((proxy.Flags & ProxyFlags.CREATE_REQUESTED) && !(proxy.Flags & ProxyFlags.CREATE_IN_PROGRESS) && !(proxy.Flags & ProxyFlags.CREATE_DONE)) {
        DestroyOutgoingProxy(proxy);

      }
      else {
        proxy.Flags |= ProxyFlags.DESTROY_REQUESTED;
        proxy.Flags &= ~ProxyFlags.IDLE;
      }
    }
  }

  public bool CreateOnRemote(Bolt.EntityObject entity) {
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
      if (proxy.Entity.HasControl && proxy.Entity.ControllerLocalPrediction) {
        continue;
      }

      // simulate this entity
      proxy.Entity.Simulate();
    }
  }

  public override void Pack(BoltPacket packet) {
    int n = 0;
    int startPos = packet.stream.Position;

    foreach (EntityProxy proxy in _outgoingProxiesByInstanceId.Values) {
      if (proxy.Flags & ProxyFlags.DESTROY_REQUESTED) {
        if (proxy.Flags & ProxyFlags.DESTROY_IN_PROGRESS) { continue; }
        if (proxy.Flags & ProxyFlags.DESTROY_DONE) { continue; }

        proxy.Mask.Clear();
        proxy.Priority = 1 << 16;
      }
      else if (proxy.Flags & ProxyFlags.CREATE_REQUESTED) {
        if (proxy.Flags & ProxyFlags.CREATE_IN_PROGRESS) { continue; }
        if (connection.isLoadingMap || BoltSceneLoader.isLoading) { continue; }

        proxy.Priority = 1 << 17;
      }
      else if (proxy.Flags & ProxyFlags.CREATE_DONE) {
        // check update rate of this entity
        if ((packet.number % proxy.Entity.UpdateRate) != 0) { continue; }

        // if we are loading
        if (connection.isLoadingMap || BoltSceneLoader.isLoading) { continue; }

        // we are forcing a sync 
        if (proxy.Flags & ProxyFlags.FORCE_SYNC) {
          proxy.Priority = 1 << 18;
        }
        else {
          // if we are idle and this is not a forced sync
          if (proxy.Flags & ProxyFlags.IDLE) { continue; }

          // if we dont have anything to send
          if (proxy.Mask.AndCheck(proxy.Entity.Serializer.GetFilter(connection, proxy)) == false) { continue; }

          // calculate priority
          proxy.Priority = Mathf.Clamp(proxy.Entity.Serializer.CalculatePriority(connection, proxy.Mask, proxy.Skipped), 0, 1 << 15);
        }
      }

      _outgoingProxiesByPriority[n++] = proxy;
    }

    if (n > 0) {
      // only if we have any proxies to sort
      // sort proxies by their priority (highest to lowest)
      Array.Sort(_outgoingProxiesByPriority, 0, n, EntityProxy.PriorityComparer.Instance);

      // write as many proxies into the packet as possible

      int i = 0;
      int failCount = 0;

      for (; i < n; ++i) {
        var result = PackUpdate(packet, _outgoingProxiesByPriority[i]);
        if (result == false) {
          if (++failCount >= 2) {
            break;
          }
        }
      }

      // proxies not written get their skipped count increased by one
      for (; i < n; ++i) { _outgoingProxiesByPriority[i].Skipped += 1; }

      // clear priority array
      Array.Clear(_outgoingProxiesByPriority, 0, _outgoingProxiesByPriority.Length);
    }

    packet.stream.WriteStopMarker();
    packet.info.entityBits = packet.stream.Position - startPos;
  }

  public override void Read(BoltPacket packet) {
    // unpack all of our data
    while (packet.stream.CanRead()) {
      if (ReadUpdate(packet) == false) {
        break;
      }
    }
  }

  public override void Lost(BoltPacket packet) {
    while (packet.envelopes.count > 0) {
      var env = packet.envelopes.RemoveFirst();
      var pending = env.Proxy.Envelopes.Dequeue();

      Assert.Same(env, pending);
      Assert.Same(env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId]);

      // copy back all priorities
      ApplyPropertyPriorities(env);

      // push skipped count up one
      env.Proxy.Skipped += 1;

      // if this was a forced sync, set flag on proxy again
      if (env.Flags & ProxyFlags.FORCE_SYNC) {
        env.Proxy.Flags |= ProxyFlags.FORCE_SYNC;
      }

      // if we failed to destroy this clear destroying flag
      if (env.Flags & ProxyFlags.DESTROY_IN_PROGRESS) {
        Assert.True(env.Proxy.Flags & ProxyFlags.DESTROY_IN_PROGRESS);
        Assert.True(env.Proxy.Envelopes.count == 0);
        env.Proxy.Flags &= ~ProxyFlags.DESTROY_IN_PROGRESS;
      }

      // if we failed to create this clear creating flag
      else if (env.Flags & ProxyFlags.CREATE_IN_PROGRESS) {
        Assert.True(env.Proxy.Flags & ProxyFlags.CREATE_IN_PROGRESS);
        Assert.True(env.Proxy.Envelopes.count == 0);
        env.Proxy.Flags &= ~ProxyFlags.CREATE_IN_PROGRESS;
      }

      env.Dispose();
    }
  }

  public override void Delivered(BoltPacket packet) {
    while (packet.envelopes.count > 0) {
      var env = packet.envelopes.RemoveFirst();
      var pending = env.Proxy.Envelopes.Dequeue();

      Assert.Same(env, pending);
      Assert.Same(env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId]);

      if (env.Flags & ProxyFlags.DESTROY_IN_PROGRESS) {
        Assert.True(env.Proxy.Flags & ProxyFlags.DESTROY_IN_PROGRESS);
        Assert.True(env.Proxy.Envelopes.count == 0);

        // clear out request / progress for create
        env.Proxy.Flags &= ~ProxyFlags.DESTROY_REQUESTED;
        env.Proxy.Flags &= ~ProxyFlags.DESTROY_IN_PROGRESS;

        // set destroy done
        env.Proxy.Flags |= ProxyFlags.DESTROY_DONE;

        // delete proxy
        DestroyOutgoingProxy(env.Proxy);

      }
      else if (env.Flags & ProxyFlags.CREATE_IN_PROGRESS) {
        Assert.True(env.Proxy.Flags & ProxyFlags.CREATE_IN_PROGRESS);
        Assert.True(env.Proxy.Envelopes.count == 0);

        // clear out request / progress for create
        env.Proxy.Flags &= ~ProxyFlags.CREATE_REQUESTED;
        env.Proxy.Flags &= ~ProxyFlags.CREATE_IN_PROGRESS;

        // set create done
        env.Proxy.Flags |= ProxyFlags.CREATE_DONE;
      }

      env.Dispose();
    }
  }

  public override void Disconnected() {
    foreach (EntityProxy proxy in _outgoingProxiesByInstanceId.Values) {
      if (proxy) {
        DestroyOutgoingProxy(proxy);
      }
    }

    foreach (EntityProxy proxy in _incommingProxiesByNetId.Values) {
      if (proxy) {
        DestroyIncommingProxy(proxy);
      }
    }
  }

  public int GetSkippedUpdates(EntityObject en) {
    EntityProxy proxy;

    if (_outgoingProxiesByInstanceId.TryGetValue(en.InstanceId, out proxy)) {
      return proxy.Skipped;
    }

    return 0;
  }

  void ApplyPropertyPriorities(EntityProxyEnvelope env) {
    for (int i = 0; i < env.Written.Count; ++i) {
      Priority p = env.Written[i];

      // set flag for sending this property again
      env.Proxy.Mask.Set(p.PropertyIndex);

      // increment priority
      env.Proxy.PropertyPriority[p.PriorityValue].PriorityValue += p.PriorityValue;
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

        //packet.stream.WriteVector3Half(proxy.entity.transform.position);
        //if (BoltCore._config.globalUniqueIds) {
        //  packet.stream.WriteUniqueId(proxy.entity._uniqueId);
        //}
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
      if (isCreate) { env.Flags = (proxy.Flags |= ProxyFlags.CREATE_IN_PROGRESS); }
      if (isDestroy) { env.Flags = (proxy.Flags |= ProxyFlags.DESTROY_IN_PROGRESS); }

      // clear force sync flag
      proxy.Flags &= ~ProxyFlags.FORCE_SYNC;

      // clear skipped count
      proxy.Skipped = 0;

      // put on packets list
      packet.envelopes.AddLast(env);

      // put on proxies pending queue
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
      Assert.NotNull(_incommingProxiesByNetId[netId]);
      DestroyIncommingProxy(_incommingProxiesByNetId[netId]);
    }
    else {
      bool isController = packet.stream.ReadBool();
      bool createRequested = packet.stream.ReadBool();

      EntityObject entity = null;
      EntityProxy proxy = null;

      if (createRequested) {
        GameObject prefab = null;
        PrefabId prefabId = PrefabId.Read(packet.stream, 32);
        TypeId serializerId = TypeId.Read(packet.stream, 32);

        if (BoltRuntimeSettings.prefabs.TryGetIndex(prefabId.Value, out prefab)) {
          if (BoltCore.isServer && !prefab.GetComponent<BoltEntity>()._allowInstantiateOnClient) {
            throw new BoltException("Received entity of prefab {0} from client at {1}, but this entity is not allowed to be instantiated from clients", prefab.name, connection.remoteEndPoint);
          }
        }
        else {
          throw new BoltException("Received entity with unknown {0}", prefabId);
        }

        // create entity
        entity = BoltCore.CreateEntity(prefab, serializerId);
        entity.Source = connection;

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
          proxy.Entity.TakeControl();
        }

        // log debug info
        BoltLog.Debug("Received proxy for {0} from {1}", entity.UnityObject, connection);
      }
      else {
        // find proxy
        proxy = _incommingProxiesByNetId[netId];

        if (proxy == null) {
          throw new BoltException("couldn't find proxy with id {0}", netId);
        }

        // read update
        proxy.Entity.Serializer.Read(connection, packet.stream, packet.frame);

        // update control state yes/no
        if (proxy.Entity.HasControl ^ isController) {
          if (isController) {
            proxy.Entity.TakeControl();
          }
          else {
            proxy.Entity.ReleaseControl();
          }
        }
      }

    }

    return true;
  }

  void DestroyOutgoingProxy(EntityProxy proxy) {
    _outgoingProxiesByNetId.Remove(proxy.NetId);
    _outgoingProxiesNetworkIdPool.Release(proxy.NetId);
    _outgoingProxiesByInstanceId.Remove(proxy.Entity.InstanceId);
  }

  void DestroyIncommingProxy(EntityProxy proxy) {
    _incommingProxiesByInstanceId.Remove(proxy.Entity.InstanceId);
    _incommingProxiesByNetId.Remove(proxy.NetId);

    // destroy entity
    BoltCore.DestroyForce(proxy.Entity);
  }
}

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

    proxy = EntityProxy.Alloc();
    proxy.NetId = id;
    proxy.Entity = entity;
    proxy.Connection = connection;
    proxy.Flags = ProxyFlags.CREATE_REQUESTED;
    proxy.Mask = entity.Serializer.GetFullMask().Clone();
    proxy.Filter = entity.Serializer.GetDefaultFilter();

    _outgoingProxiesByNetId[proxy.NetId] = proxy;
    _outgoingProxiesByInstanceId[entity.InstanceId] = proxy;

    BoltLog.Debug("created {0} on {1}", proxy, connection);

    return true;
  }

  public override void StepRemoteFrame() {
    foreach (EntityProxy proxy in _incommingProxiesByInstanceId.Values) {
      // skip ones we are in control of
      if (proxy.Entity.Flags & EntityFlags.HAS_CONTROL) {
        continue;
      }

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
      else if (proxy.Flags & ProxyFlags.CREATE_DONE) {
        // if we are loading
        if (connection.isLoadingMap || BoltSceneLoader.isLoading) { continue; }

        // check update rate of this entity
        if ((packet.number % proxy.Entity.UpdateRate) != 0) { continue; }

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
      else if (proxy.Flags & ProxyFlags.CREATE_REQUESTED) {
        if (proxy.Flags & ProxyFlags.CREATE_IN_PROGRESS) { continue; }
        if (connection.isLoadingMap || BoltSceneLoader.isLoading) { continue; }

        proxy.Priority = 1 << 17;
      }

      _outgoingProxiesByPriority[n++] = proxy;
    }

    if (n > 0) {
      // only if we have any proxies to sort
      // sort proxies by their priority (highest to lowest)
      Array.Sort(_outgoingProxiesByPriority, 0, n, EntityProxy.PriorityComparer.Instance);

      // write as many proxies into the packet as possible

      int i = 0;

      for (; i < n; ++i) {
        bool success = PackUpdate(packet, _outgoingProxiesByPriority[i]);

        // we failed to write this into the packet
        if (success == false) { break; }
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

      if (!(env.Proxy.Flags & ProxyFlags.DESTROY_DONE)) {
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
      }

      env.Dispose();
    }
  }

  public override void Delivered(BoltPacket packet) {
    while (packet.envelopes.count > 0) {
      var env = packet.envelopes.RemoveFirst();
      var pending = env.Proxy.Envelopes.Dequeue();

      if (!(env.Proxy.Flags & ProxyFlags.DESTROY_DONE)) {
        Assert.Same(env, pending);
        Assert.Same(env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId]);

        if (env.Flags & ProxyFlags.DESTROY_IN_PROGRESS) {
          Assert.True(env.Proxy.Flags & ProxyFlags.DESTROY_IN_PROGRESS);
          Assert.True(env.Proxy.Envelopes.count == 0);

          env.Proxy.Flags |= ProxyFlags.DESTROY_DONE;

          DestroyOutgoingProxy(env.Proxy);

        }
        else if (env.Flags & ProxyFlags.CREATE_IN_PROGRESS) {
          Assert.True(env.Proxy.Flags & ProxyFlags.CREATE_IN_PROGRESS);
          Assert.True(env.Proxy.Envelopes.count == 0);

          env.Proxy.Flags &= ~ProxyFlags.CREATE_REQUESTED;
          env.Proxy.Flags |= ProxyFlags.CREATE_DONE;
        }
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
      env.Proxy.Mask.Set(p.Property);

      // increment priority
      env.Proxy.PropertyPriority[p.Value].Value += p.Value;
    }
  }

  bool PackUpdate(BoltPacket packet, EntityProxy proxy) {
    int pos = packet.stream.Position;
    EntityProxyEnvelope env = proxy.CreateEnvelope();

    packet.stream.WriteBool(true);
    packet.stream.WriteNetworkId(proxy.NetId);

    if (packet.stream.WriteBool(proxy.Flags & ProxyFlags.DESTROY_REQUESTED) == false) {
      //packet.stream.WriteBool(proxy.entity._teleportFlip);

      //// info struct
      //BoltEntityUpdateInfo info = new BoltEntityUpdateInfo();
      //info.connection = connection;
      //info.frame = packet.frame;
      //info.first = packet.stream.WriteBool(proxy.flags & BoltEntityProxy.FLAG_CREATE);

      // data for first packet
      if (packet.stream.WriteBool(proxy.Flags & ProxyFlags.CREATE_REQUESTED)) {
        proxy.Entity.PrefabId.Pack(packet.stream, 32);
        proxy.Entity.Serializer.TypeId.Pack(packet.stream, 32);

        //packet.stream.WriteVector3Half(proxy.entity.transform.position);
        //if (BoltCore._config.globalUniqueIds) {
        //  packet.stream.WriteUniqueId(proxy.entity._uniqueId);
        //}
      }

      // if the remote is the controller or not
      packet.stream.WriteBool(ReferenceEquals(proxy.Entity.Controller, connection));

      // let serializer pack data
      proxy.Entity.Serializer.Pack(connection, packet.stream, env);
    }

    if (packet.stream.Overflowing) {
      packet.stream.Position = pos;
      return false;

    }
    else {
      // clear force sync flag and skipped count
      proxy.Flags &= ~ProxyFlags.FORCE_SYNC;
      proxy.Skipped = 0;

      // clear out property priorities and dirty bits
      for (int i = 0; i < env.Written.Count; ++i) {
        Priority p = env.Written[i];
        proxy.PropertyPriority[p.Property].Value = 0;
        proxy.Mask.Clear(p.Property);
      }

      // set in progress flag
      if ((proxy.Flags & ProxyFlags.DESTROY_REQUESTED)) {
        env.Flags = (proxy.Flags |= ProxyFlags.DESTROY_IN_PROGRESS);
      }
      else if ((proxy.Flags & ProxyFlags.CREATE_REQUESTED)) {
        env.Flags = (proxy.Flags |= ProxyFlags.CREATE_IN_PROGRESS);
      }

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
    var destroy = packet.stream.ReadBool();

    // we're destroying this proxy
    if (destroy) {
      Assert.NotNull(_incommingProxiesByNetId[netId]);
      DestroyIncommingProxy(_incommingProxiesByNetId[netId]);
    }
    else {
      bool create = packet.stream.ReadBool();
      bool controlling = packet.stream.ReadBool();

      EntityObject entity = null;
      EntityProxy proxy = null;

      if (create) {
        GameObject prefab = null;
        GameObject instance = null;

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
        entity.Serializer.Read(connection, packet.stream, packet.frame);

        // create proxy
        proxy = EntityProxy.Alloc();
        proxy.NetId = netId;
        proxy.Entity = entity;
        proxy.Connection = connection;

        // register proxy
        _incommingProxiesByNetId[netId] = proxy;
        _incommingProxiesByInstanceId[proxy.Entity.InstanceId] = proxy;

        // handle case where we are given control (it needs to be true during the attached callbacks)
        if (controlling) {
          entity.Flags |= EntityFlags.HAS_CONTROL;
        }

        // attach entity
        proxy.Entity.Attach();

        // again for the given control case, we need to clear out the HAS_CONTROL flag or .TakeControl will complain
        if (controlling) {
          proxy.Entity.Flags &= ~EntityFlags.HAS_CONTROL;
          proxy.Entity.TakeControl();
        }

        // log debug info
        BoltLog.Debug("Received proxy for {0} from {1}", entity.UserToken, connection);
      }
      else {
        proxy = _incommingProxiesByNetId[netId];

        if (proxy == null) {
          throw new BoltException("couldn't find proxy with id {0}", netId);
        }

        if (proxy.Entity.HasControl ^ controlling) {
          if (controlling) {
            proxy.Entity.TakeControl();
          }
          else {
            proxy.Entity.ReleaseControl();
          }
        }
      }

      proxy.Entity.Serializer.Read(connection, packet.stream, packet.frame);
    }

    return true;
  }

  void DestroyOutgoingProxy(EntityProxy proxy) {
    if (proxy.Entity != null) {
      _outgoingProxiesByInstanceId.Remove(proxy.Entity.InstanceId);
    }

    _outgoingProxiesByNetId.Remove(proxy.NetId);
    _outgoingProxiesNetworkIdPool.Release(proxy.NetId);

    EntityProxy.Free(proxy);
  }

  void DestroyIncommingProxy(EntityProxy proxy) {
    if (proxy.Entity != null) {
      _incommingProxiesByInstanceId.Remove(proxy.Entity.InstanceId);
    }

    _incommingProxiesByNetId.Remove(proxy.NetId);

    // debugggggggg!
    //BoltLog.Debug("{0} is no longer proxied from {1}", proxy.entity, connection);

    // destroy entity
    BoltCore.DestroyForce(proxy.Entity);

    // free proxy object
    EntityProxy.Free(proxy);
  }
}

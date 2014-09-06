using System;
using System.Collections.Generic;
using UnityEngine;

enum BoltEntityDestroyMode {
  OutOfScope,
  LocalDestroy
}

partial class BoltEntityChannel : BoltChannel {
  BoltIdPool _outgoingProxiesNetworkIdPool;
  BoltEntityProxy[] _outgoingProxiesByNetworkId;
  BoltEntityProxy[] _outgoingProxiesByPriority;
  Dictionary<uint, BoltEntityProxy> _outgoingProxiesByEntityId;

  BoltEntityProxy[] _incommingProxiesByNetworkId;
  Dictionary<uint, BoltEntityProxy> _incommingProxiesByEntityId;

  public BoltEntityChannel () {
    _outgoingProxiesNetworkIdPool = new BoltIdPool(BoltEntityProxy.MAX_COUNT);
    _outgoingProxiesByNetworkId = new BoltEntityProxy[BoltEntityProxy.MAX_COUNT];
    _outgoingProxiesByPriority = new BoltEntityProxy[BoltEntityProxy.MAX_COUNT];
    _outgoingProxiesByEntityId = new Dictionary<uint, BoltEntityProxy>(1024);

    _incommingProxiesByNetworkId = new BoltEntityProxy[BoltEntityProxy.MAX_COUNT];
    _incommingProxiesByEntityId = new Dictionary<uint, BoltEntityProxy>(1024);
  }

  public BoltEntity GetIncommingEntity (uint proxyId) {
    if (_incommingProxiesByNetworkId[proxyId] != null) {
      return _incommingProxiesByNetworkId[proxyId].entity;
    }

    return null;
  }

  public BoltEntity GetOutgoingEntity (uint proxyId) {
    if (_outgoingProxiesByNetworkId[proxyId] != null) {
      return _outgoingProxiesByNetworkId[proxyId].entity;
    }

    return null;
  }

  public void ForceSync (BoltEntity en) {
    BoltEntityProxy proxy;

    if (_outgoingProxiesByEntityId.TryGetValue(en._id, out proxy)) {
      if (en.IsControlledBy(connection)) {
        proxy.mask = en.boltSerializer.controllerMask;
      } else {
        proxy.mask = en.boltSerializer.proxyMask;
      }

      proxy.flags |= BoltEntityProxy.FLAG_FORCE_SYNC;
    }
  }

  public void SetIdle (BoltEntity entity, bool idle) {
    BoltEntityProxy proxy;

    if (_outgoingProxiesByEntityId.TryGetValue(entity._id, out proxy)) {
      if (idle) {
        proxy.flags |= BoltEntityProxy.FLAG_IDLE;
      } else {
        proxy.flags &= ~BoltEntityProxy.FLAG_IDLE;
      }
    }
  }

  public uint GetNetworkId (BoltEntity entity) {
    BoltEntityProxy proxy;

    if (_outgoingProxiesByEntityId.TryGetValue(entity._id, out proxy)) { return proxy.networkId; }
    if (_incommingProxiesByEntityId.TryGetValue(entity._id, out proxy)) { return proxy.networkId; }

    return uint.MaxValue;
  }

  public bool ExistsOnRemote (BoltEntity entity) {
    if (_incommingProxiesByEntityId.ContainsKey(entity._id)) { return true; }

    BoltEntityProxy proxy;

    if (_outgoingProxiesByEntityId.TryGetValue(entity._id, out proxy)) {
      const uint FLAGS =
        BoltEntityProxy.FLAG_CREATE |
        BoltEntityProxy.FLAG_CREATE_IN_PROGRESS |
        BoltEntityProxy.FLAG_DESTROY |
        BoltEntityProxy.FLAG_DESTROY_IN_PROGRESS;

      return (proxy.flags & FLAGS) == 0;
    }

    return false;
  }

  public bool MightExistOnRemote (BoltEntity entity) {
    return _incommingProxiesByEntityId.ContainsKey(entity._id) || _outgoingProxiesByEntityId.ContainsKey(entity._id);
  }

  public void DestroyOnRemote (BoltEntity entity, BoltEntityDestroyMode mode) {
    BoltEntityProxy proxy;

    if (_outgoingProxiesByEntityId.TryGetValue(entity._id, out proxy)) {
      // if this entity is being destroyed locally
      if (mode == BoltEntityDestroyMode.LocalDestroy) {
        // we should clear it from our entity id lookup table
        _outgoingProxiesByEntityId.Remove(entity._id);

        // and also null the entity on the proxy
        proxy.entity = null;
      }

      if (proxy.flags.IsSet(BoltEntityProxy.FLAG_CREATE) && proxy.flags.IsClear(BoltEntityProxy.FLAG_CREATE_IN_PROGRESS)) {
        // if the create flag is set but we are not trying to create this proxy currently we can destroy it right away
        DestroyOutgoingProxy(proxy, true);

      } else {
        // if not set, we should set the destroy flag to mark this object for destruction
        // and also clear the idle flag, as destroy operations have the highest priority
        proxy.flags |= BoltEntityProxy.FLAG_DESTROY;
        proxy.flags &= ~BoltEntityProxy.FLAG_IDLE;
      }
    }
  }

  public bool CreateOnRemote (BoltEntity entity) {
    BoltEntityProxy proxy;

    if (_incommingProxiesByEntityId.TryGetValue(entity._id, out proxy)) { return true; }
    if (_outgoingProxiesByEntityId.TryGetValue(entity._id, out proxy)) { return true; }

    uint id;

    if (_outgoingProxiesNetworkIdPool.Acquire(out id) == false) {
      BoltLog.Warn("{0} is already proxying the max amount of objects", connection);
      return false;
    }

    proxy = BoltEntityProxy.Alloc();
    proxy.connection = connection;
    proxy.networkId = id;
    proxy.entity = entity;
    proxy.flags = BoltEntityProxy.FLAG_CREATE;
    proxy.mask = uint.MaxValue;

    _outgoingProxiesByNetworkId[proxy.networkId] = proxy;
    _outgoingProxiesByEntityId[entity._id] = proxy;

    //BoltLog.Debug("created {0} on {1}", proxy, connection);
    return true;
  }

  public override void StepRemoteFrame () {
    for (int i = 0; i < _incommingProxiesByNetworkId.Length; ++i) {
      BoltEntityProxy proxy = _incommingProxiesByNetworkId[i];

      if (proxy && proxy.entity.boltIsProxy == true && proxy.entity.boltIsControlling == false) {
        proxy.entity.SimulateStep();
      }
    }
  }

  public override void Pack (BoltPacket packet) {
    int startPos = packet.stream.Position;
    int n = 0;

    for (int i = 0; i < _outgoingProxiesByNetworkId.Length; ++i) {
      BoltEntityProxy proxy = _outgoingProxiesByNetworkId[i];

      // if we dont have a proxy, skip this
      if (!proxy) { continue; }

      if (proxy.flags & BoltEntityProxy.FLAG_DESTROY) {
        if (proxy.flags & BoltEntityProxy.FLAG_DESTROY_IN_PROGRESS) { continue; }

        proxy.mask = 0;
        proxy.priority = 1 << 16;

      } else if (proxy.flags & BoltEntityProxy.FLAG_CREATE) {
        if (proxy.flags & BoltEntityProxy.FLAG_CREATE_IN_PROGRESS) { continue; }
        if (connection.isLoadingMap || BoltSceneLoader.isLoading) { continue; }

        if (proxy.entity.IsControlledBy(connection)) {
          proxy.mask = proxy.entity.boltSerializer.controllerMask;
        } else {
          proxy.mask = proxy.entity.boltSerializer.proxyMask;
        }

        proxy.priority = 1 << 17;

      } else {
        // skip if the connection is disabled
        if (connection.isLoadingMap || BoltSceneLoader.isLoading) {
          continue;
        }

        // check update rate of this entity
        else if ((packet.number % proxy.entity.boltPackFrequency) != 0) {
          continue;
        }

        // we are forcing a sync
        else if (proxy.flags & BoltEntityProxy.FLAG_FORCE_SYNC) {
          proxy.priority = 1 << 18;
        }

        // skip if no dirty mask or idle
        else if ((proxy.mask == 0) || (proxy.flags & BoltEntityProxy.FLAG_IDLE)) {
          continue;
        }

        // normal case, calculate priority
        else {
          proxy.priority = proxy.entity.boltSerializer.CalculatePriority(connection, proxy.mask, proxy.skipped);
        }
      }

      _outgoingProxiesByPriority[n++] = proxy;
    }

    if (n > 0) {
      // only if we have any proxies to sort
      // sort proxies by their priority (highest to lowest)
      Array.Sort(_outgoingProxiesByPriority, 0, n, BoltEntityProxy.PriorityComparer.Instance);

      // write as many proxies into the packet as possible

      int i = 0;

      for (; i < n; ++i) {
        bool success = PackUpdate(packet, _outgoingProxiesByPriority[i]);

        // we failed to write this into the packet
        if (success == false) { break; }
      }

      // proxies not written get their skipped count increased by one
      for (; i < n; ++i) { _outgoingProxiesByPriority[i].skipped += 1; }

      // clear priority array
      Array.Clear(_outgoingProxiesByPriority, 0, _outgoingProxiesByPriority.Length);
    }

    packet.stream.WriteStopMarker();
    packet.info.entityBits = packet.stream.Position - startPos;
  }

  public override void Read (BoltPacket packet) {
    // unpack all of our data
    while (packet.stream.CanRead()) {
      if (ReadUpdate(packet) == false) {
        break;
      }
    }
  }

  public override void Lost (BoltPacket packet) {
    while (packet.envelopes.count > 0) {
      var env = packet.envelopes.RemoveFirst();
      var pending = env.proxy.envelopes.Dequeue();

      if (env.proxy.destroyed == false) {
        Assert.Same(env, pending);
        Assert.Same(env.proxy, _outgoingProxiesByNetworkId[env.proxy.networkId]);

        Bits mask = env.mask;

        for (int i = 0; i < env.proxy.envelopes.count; ++i) {
          mask &= ~env.proxy.envelopes[i].mask;
        }

        env.proxy.mask |= mask;
        env.proxy.skipped += 1;

        // if this was a forced sync, set flag on proxy again
        if (env.flags & BoltEntityProxy.FLAG_FORCE_SYNC) {
          env.proxy.flags |= BoltEntityProxy.FLAG_FORCE_SYNC;
        }

        // if we failed to destroy this clear destroying flag
        if (env.flags & BoltEntityProxy.FLAG_DESTROY_IN_PROGRESS) {
          Assert.True(env.proxy.flags & BoltEntityProxy.FLAG_DESTROY_IN_PROGRESS);
          Assert.True(env.proxy.envelopes.count == 0);
          env.proxy.flags &= ~BoltEntityProxy.FLAG_DESTROY_IN_PROGRESS;
        }

        // if we failed to create this clear creating flag
        else if (env.flags & BoltEntityProxy.FLAG_CREATE_IN_PROGRESS) {
          Assert.True(env.proxy.flags & BoltEntityProxy.FLAG_CREATE_IN_PROGRESS);
          Assert.True(env.proxy.envelopes.count == 0);
          env.proxy.flags &= ~BoltEntityProxy.FLAG_CREATE_IN_PROGRESS;
        }
      }

      env.Dispose();
    }
  }

  public override void Delivered (BoltPacket packet) {
    while (packet.envelopes.count > 0) {
      var env = packet.envelopes.RemoveFirst();
      var pending = env.proxy.envelopes.Dequeue();

      if (env.proxy.destroyed == false) {
        Assert.Same(env, pending);
        Assert.Same(env.proxy, _outgoingProxiesByNetworkId[env.proxy.networkId]);

        if (env.flags & BoltEntityProxy.FLAG_DESTROY_IN_PROGRESS) {
          Assert.True(env.proxy.flags & BoltEntityProxy.FLAG_DESTROY_IN_PROGRESS);
          Assert.True(env.proxy.envelopes.count == 0);

          DestroyOutgoingProxy(env.proxy, false);

        } else if (env.flags & BoltEntityProxy.FLAG_CREATE_IN_PROGRESS) {
          Assert.True(env.proxy.flags & BoltEntityProxy.FLAG_CREATE_IN_PROGRESS);
          Assert.True(env.proxy.envelopes.count == 0);

          env.proxy.flags &= ~(BoltEntityProxy.FLAG_CREATE | BoltEntityProxy.FLAG_CREATE_IN_PROGRESS);
        }
      }

      env.Dispose();
    }
  }

  public override void Disconnected () {
    for (int i = 0; i < _outgoingProxiesByNetworkId.Length; ++i) {
      BoltEntityProxy proxy = _outgoingProxiesByNetworkId[i];

      if (proxy) {
        proxy.entity = null;
        proxy.flags |= BoltEntityProxy.FLAG_DESTROY;

        DestroyOutgoingProxy(proxy, true);
      }
    }

    for (int i = 0; i < _incommingProxiesByNetworkId.Length; ++i) {
      BoltEntityProxy proxy = _incommingProxiesByNetworkId[i];

      if (proxy) {
        DestroyIncommingProxy(proxy);
      }
    }
  }

  public uint GetSkippedUpdates (BoltEntity en) {
    BoltEntityProxy proxy;

    if (_outgoingProxiesByEntityId.TryGetValue(en._id, out proxy)) {
      return proxy.skipped;
    }

    return 0;
  }

  bool PackUpdate (BoltPacket packet, BoltEntityProxy proxy) {
    int pos = packet.stream.Position;
    BoltEntityProxyEnvelope env = proxy.CreateEnvelope();

    packet.stream.WriteBool(true);
    packet.stream.WriteNetworkId(proxy.networkId);

    if (packet.stream.WriteBool(proxy.flags & BoltEntityProxy.FLAG_DESTROY) == false) {
      packet.stream.WriteBool(proxy.entity._teleportFlip);

      // info struct
      BoltEntityUpdateInfo info = new BoltEntityUpdateInfo();
      info.connection = connection;
      info.frame = packet.frame;
      info.first = packet.stream.WriteBool(proxy.flags & BoltEntityProxy.FLAG_CREATE);

      packet.stream.WriteBool(ReferenceEquals(proxy.entity._remoteController, connection));

      // data for first packet
      if (info.first) {
        packet.stream.WriteInt(proxy.entity.boltPrefabId);
        packet.stream.WriteVector3Half(proxy.entity.transform.position);

        if (BoltCore._config.globalUniqueIds) {
          packet.stream.WriteUniqueId(proxy.entity._uniqueId);
        }
      }

      // copy proxy mask
      Bits mask = proxy.mask;

      // write into stream
      proxy.entity.boltSerializer.Pack(info, packet.stream, ref mask);
      proxy.mask = mask;
    }

    if (packet.stream.Overflowing) {
      proxy.mask = env.mask;
      packet.stream.Position = pos;
      return false;

    } else {
      // clear force sync flag and skipped count
      proxy.flags &= ~BoltEntityProxy.FLAG_FORCE_SYNC;
      proxy.skipped = 0;

      // mask out bits "not used"
      env.mask &= ~proxy.mask;

      // set in progress flag
      if (proxy.flags & BoltEntityProxy.FLAG_CREATE) {
        env.flags = (proxy.flags |= BoltEntityProxy.FLAG_CREATE_IN_PROGRESS);
      } else if (proxy.flags & BoltEntityProxy.FLAG_DESTROY) {
        env.flags = (proxy.flags |= BoltEntityProxy.FLAG_DESTROY_IN_PROGRESS);
      }

      // put on packets list
      packet.envelopes.AddLast(env);

      // put on proxies pending queue
      proxy.envelopes.Enqueue(env);

      // keep going!
      return true;
    }
  }

  bool ReadUpdate (BoltPacket packet) {
    if (packet.stream.ReadBool() == false)
      return false;

    // grab networkid
    uint networkId = packet.stream.ReadNetworkId();
    bool destroy = packet.stream.ReadBool();

    // we're destroying this proxy
    if (destroy) {
      Assert.NotNull(_incommingProxiesByNetworkId[networkId]);
      DestroyIncommingProxy(_incommingProxiesByNetworkId[networkId]);

    } else {
      bool teleportFlip = packet.stream.ReadBool();
      bool first = packet.stream.ReadBool();
      bool controlling = packet.stream.ReadBool();
      BoltEntityProxy proxy = null;

      // we're creating this proxy
      if (first) {

        int prefabId = packet.stream.ReadInt();
        Vector3 spawnAt = packet.stream.ReadVector3Half();
        BoltUniqueId uid = new BoltUniqueId();

        if (BoltCore._config.globalUniqueIds) {
          uid = packet.stream.ReadUniqueId();
        }

        GameObject prefab = null;

        if (BoltRuntimeSettings.prefabs.TryGetIndex(prefabId, out prefab)) {
          Assert.Null(_incommingProxiesByNetworkId[networkId]);

          var go = BoltCore._instantiate(prefab, spawnAt, Quaternion.identity);
          var en = go.GetComponent<BoltEntity>();

          if (BoltCore.isServer) {
            if (!en._allowInstantiateOnClient) {
              GameObject.Destroy(go);

              throw new BoltException("Received entity of prefab {1} from client at {0}, but this entity is not allowed to be instantiated from clients", connection.remoteEndPoint, prefab.name);
            }
          }

          proxy = BoltEntityProxy.Alloc();
          proxy.connection = connection;
          proxy.networkId = networkId;
          proxy.entity = en;

          _incommingProxiesByNetworkId[networkId] = proxy;
          _incommingProxiesByEntityId[proxy.entity._id] = proxy;

          BoltCore.Attach(proxy.entity, connection, BoltEntity.FLAG_IS_PROXY, uid);
          BoltLog.Debug("Received proxy {0} from {1}", proxy.entity, connection);

          if (controlling) {
            proxy.entity.TakeControlInternal();
          }
        } else {
          throw new BoltException("couldn't find prefab with id {0}", prefabId);
        }
      } else {
        proxy = _incommingProxiesByNetworkId[networkId];

        if (proxy == null) {
          throw new BoltException("couldn't find proxy with id {0}", networkId);
        }

        if (proxy.entity.boltIsControlling == true && controlling == false) {
          proxy.entity.ReleaseControlInternal();
        }

        if (proxy.entity.boltIsControlling == false && controlling == true) {
          proxy.entity.TakeControlInternal();
        }
      }

      // unpack update
      BoltEntityUpdateInfo info = new BoltEntityUpdateInfo();
      info.connection = connection;
      info.first = first;
      info.frame = packet.frame;

      if (teleportFlip != proxy.entity._teleportFlip) {
        proxy.entity._teleportFlip = teleportFlip;
        proxy.entity.boltSerializer.Teleported();
      }

      proxy.entity.boltSerializer.Read(info, packet.stream);
    }

    return true;
  }

  void DestroyOutgoingProxy (BoltEntityProxy proxy, bool allowWithoutDestroy) {
    Assert.True(proxy.flags & BoltEntityProxy.FLAG_DESTROY, "not marked with FLAG_DESTROY");

    _outgoingProxiesByNetworkId[proxy.networkId] = null;
    _outgoingProxiesNetworkIdPool.Release(proxy.networkId);

    BoltEntityProxy.Free(proxy);
  }

  void DestroyIncommingProxy (BoltEntityProxy proxy) {
    Assert.NotNull(proxy.entity);

    _incommingProxiesByEntityId.Remove(proxy.entity._id);
    _incommingProxiesByNetworkId[proxy.networkId] = null;

    // debugggggggg!
    //BoltLog.Debug("{0} is no longer proxied from {1}", proxy.entity, connection);

    // destroy entity
    BoltCore.Destroy(proxy.entity);

    // free proxy object
    BoltEntityProxy.Free(proxy);
  }
}

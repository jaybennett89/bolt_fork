using System;
using System.Text.RegularExpressions;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  partial class NetworkState : IEntitySerializer {
    TypeId IEntitySerializer.TypeId {
      get { return Meta.TypeId; }
    }

    void IEntitySerializer.OnRender() {
      for (int i = 0; i < Meta.OnRender.Count; ++i) {
        var p = Meta.OnRender[i];
        p.Property.OnRender(Objects[p.OffsetObjects]);
      }
    }

    void IEntitySerializer.OnInitialized() {
      NetworkStorage storage;

      storage = AllocateStorage();
      storage.Frame = Entity.IsOwner ? BoltCore.frame : -1;

      Frames.AddLast(storage);

      for (int i = 0; i < Meta.Properties.Length; ++i) {
        var p = Meta.Properties[i];
        p.Property.OnInit(Objects[p.OffsetObjects]);
      }
    }

    void IEntitySerializer.OnCreated(Entity entity) {
      Entity = entity;
    }

    void IEntitySerializer.OnParentChanging(Entity newParent, Entity oldParent) {
      for (int i = 0; i < Meta.Properties.Length; ++i) {
        var p = Meta.Properties[i];
        p.Property.OnParentChanged(Objects[p.OffsetObjects], newParent, oldParent);
      }
    }

    void IEntitySerializer.OnSimulateBefore() {
      if (Entity.IsOwner || Entity.HasPredictedControl) {
        Frames.first.Frame = BoltCore.frame;
      }
      else {
        while ((Frames.count > 1) && (Entity.Frame >= Frames.Next(Frames.first).Frame)) {
          // combine changed properties
          Frames.Next(Frames.first).Combine(Frames.first);

          // free it
          FreeStorage(Frames.RemoveFirst());
        }
      }

      int count = Meta.OnSimulateBefore.Count;
      if (count > 0) {
        for (int i = 0; i < count; ++i) {
          var p = Meta.OnSimulateBefore[i];
          p.Property.OnSimulateBefore(Objects[p.OffsetObjects]);
        }
      }

      InvokeCallbacks();
    }

    void IEntitySerializer.OnSimulateAfter() {
      int count = Meta.OnSimulateAfter.Count;
      if (count > 0) {
        for (int i = 0; i < count; ++i) {
          var p = Meta.OnSimulateAfter[i];
          p.Property.OnSimulateAfter(Objects[p.OffsetObjects]);
        }
      }

      InvokeCallbacks();
    }

    void IEntitySerializer.OnControlGained() {
      while (Frames.count > 1) {
        // compact all changes into last frame
        Frames.last.Combine(Frames.first);

        // remove first frame
        FreeStorage(Frames.RemoveFirst());
      }
    }

    void IEntitySerializer.OnControlLost() {
      Frames.first.Frame = Entity.Frame;
    }

    BitSet IEntitySerializer.GetDefaultMask() {
      return PropertyDefaultMask;
    }

    BitSet IEntitySerializer.GetFilter(BoltConnection connection, EntityProxy proxy) {
      if (Entity.IsController(connection)) {
        return Meta.Filters[31];
      }

      return Meta.Filters[30];
    }

    void IEntitySerializer.DebugInfo() {
      if (BoltNetworkInternal.DebugDrawer != null) {
        BoltNetworkInternal.DebugDrawer.LabelBold("");
        BoltNetworkInternal.DebugDrawer.LabelBold("State Info");
        BoltNetworkInternal.DebugDrawer.LabelField("Type", Factory.GetFactory(Meta.TypeId).TypeObject);
        BoltNetworkInternal.DebugDrawer.LabelField("Type Id", Meta.TypeId);

        BoltNetworkInternal.DebugDrawer.LabelBold("");
        BoltNetworkInternal.DebugDrawer.LabelBold("State Properties");

        for (int i = 0; i < Meta.Properties.Length; ++i) {
          var pi = Meta.Properties[i];

          string label = pi.Paths.NullOrEmpty() ? pi.Property.PropertyName : FixArrayIndices(pi.Paths[pi.Paths.Length - 1], pi.Indices);
          object value = pi.Property.DebugValue(Objects[pi.OffsetObjects], Storage);

          if (!Entity.IsOwner) {
            EntityProxy proxy;

            if (Entity.Source._entityChannel.TryFindProxy(Entity, out proxy)) {
              label = "(" + proxy.PropertyPriority[i].PropertyUpdated + ") " + label;
            }
          }

          if (value != null) {
            BoltNetworkInternal.DebugDrawer.LabelField(label, value.ToString());
          }
          else {
            BoltNetworkInternal.DebugDrawer.LabelField(label, "N/A");
          }
        }
      }
    }

    string FixArrayIndices(string path, int[] indices) {
      Regex r = new Regex(@"\[\]");

      for (int i = 0; i < indices.Length; ++i) {
        path = r.Replace(path, "[" + indices[i] + "]", 1);
      }

      return path;
    }

    void IEntitySerializer.InitProxy(EntityProxy p) {
      p.PropertyPriority = new Priority[Meta.CountProperties];

      for (int i = 0; i < p.PropertyPriority.Length; ++i) {
        p.PropertyPriority[i].PropertyIndex = i;
      }
    }

    int IEntitySerializer.Pack(BoltConnection connection, UdpPacket stream, EntityProxyEnvelope env) {
      int propertyCount = 0;

      BitSet filter = ((IEntitySerializer)this).GetFilter(connection, env.Proxy);

      Priority[] tempPriority = Meta.PropertiesTempPriority;
      Priority[] proxyPriority = env.Proxy.PropertyPriority;

      for (int i = 0; i < proxyPriority.Length; ++i) {
        Assert.True(proxyPriority[i].PropertyIndex == i);

        // if this property is set both in our filter and the proxy mask we can consider it for sending
        if (filter.IsSet(i) && env.Proxy.IsSet(i)) {
          // increment priority for this property
          proxyPriority[i].PropertyPriority += Meta.Properties[i].Property.PropertyPriority;
          proxyPriority[i].PropertyPriority = UE.Mathf.Clamp(proxyPriority[i].PropertyPriority, 0, BoltCore._config.maxPropertyPriority);

          // copy to our temp array
          tempPriority[propertyCount] = proxyPriority[i];

          // increment temp count
          propertyCount += 1;
        }
      }

      // sort temp array based on priority
      Array.Sort<Priority>(tempPriority, 0, propertyCount, Priority.Comparer.Instance);

      // write into stream
      PackProperties(connection, stream, env, tempPriority, propertyCount);

      for (int i = 0; i < env.Written.Count; ++i) {
        Priority p = env.Written[i];

        // clear priority for written property
        env.Proxy.PropertyPriority[p.PropertyIndex].PropertyPriority = 0;

        // clear mask for it
        env.Proxy.Clear(p.PropertyIndex);
      }

      return env.Written.Count;
    }

    void PackProperties(BoltConnection connection, UdpPacket packet, EntityProxyEnvelope env, Priority[] priority, int count) {
      int propertyCountPtr = packet.Ptr;
      packet.WriteByte(0, Meta.PacketMaxPropertiesBits);

      // how many bits can we write at the most
      int bits = System.Math.Min(Meta.PacketMaxBits, packet.Size - packet.Position);

      for (int i = 0; i < count; ++i) {
        // this means we can even fit another property id
        if (bits <= Meta.PropertyIdBits) {
          break;
        }

        // we have written enough properties
        if (env.Written.Count == Meta.PacketMaxProperties) {
          break;
        }

        Priority p = priority[i];
        NetworkPropertyInfo pi = Meta.Properties[p.PropertyIndex];

        if (p.PropertyPriority == 0) {
          break;
        }

        int b = Meta.PropertyIdBits + pi.Property.BitCount(Objects[pi.OffsetObjects]);
        int ptr = packet.Ptr;

        if (bits >= b) {
          // write property id
          packet.WriteInt(p.PropertyIndex, Meta.PropertyIdBits);

          if (pi.Property.Write(connection, Objects[pi.OffsetObjects], Storage, packet)) {

#if DEBUG
            int totalBits = packet.Position - ptr;
            if (totalBits != b) {
              //BoltLog.Warn("Property of type {0} did not write the correct amount of bits, written: {1}, expected: {2}", pi.Property, totalBits, b);
            }
#endif

            if (packet.Overflowing) {
              packet.Ptr = ptr;
              break;
            }

            // use up bits
            bits -= b;

            // add to written list
            env.Written.Add(p);
          }
          else {
            // reset position
            packet.Ptr = ptr;
          }
        }
      }

      // gotta be less then 256
      Assert.True(env.Written.Count <= Meta.PacketMaxProperties);

      // write the amount of properties
      UdpPacket.WriteByteAt(packet.Data, propertyCountPtr, Meta.PacketMaxPropertiesBits, (byte)env.Written.Count);
    }

    void IEntitySerializer.Read(BoltConnection connection, UdpPacket packet, int frame) {
      int count = packet.ReadByte(Meta.PacketMaxPropertiesBits);
      var storage = default(NetworkStorage);

      if (Entity.HasPredictedControl) {
        Assert.True(Frames.count == 1);

        storage = Frames.first;
        storage.Frame = BoltCore.frame;
      }
      else {
        if (Frames.first.Frame == -1) {
          Assert.True(Frames.count == 1);

          storage = Frames.first;
          storage.Frame = frame;
        }
        else {
          storage = DuplicateStorage(Frames.last);
          storage.Frame = frame;
          storage.ClearAll();

          // tell the properties that need to know about this
          for (int i = 0; i < Meta.OnFrameCloned.Count; ++i) {
            // grab property info
            var pi = Meta.OnFrameCloned[i];

            // invoke callback
            pi.Property.OnFrameCloned(Objects[pi.OffsetObjects], storage);
          }

          Frames.AddLast(storage);
        }
      }

      while (--count >= 0) {
        var propertyIndex = packet.ReadInt(Meta.PropertyIdBits);
        var propertyInfo = Meta.Properties[propertyIndex];

        if (!Entity.IsOwner) {
          EntityProxy proxy;

          if (Entity.Source._entityChannel.TryFindProxy(Entity, out proxy)) {
            proxy.PropertyPriority[propertyIndex].PropertyUpdated = frame;
          }
        }

        // make sure this is the correct one
        Assert.True(propertyIndex == Objects[propertyInfo.OffsetObjects].OffsetProperties + propertyInfo.Property.OffsetProperties);

        // read data into frame
        propertyInfo.Property.Read(connection, Objects[propertyInfo.OffsetObjects], storage, packet);

        // set changed flag
        storage.Set(propertyIndex);
      }
    }
  }
}

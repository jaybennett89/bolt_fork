using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  partial class NetworkState : IEntitySerializer {
    TypeId IEntitySerializer.TypeId {
      get { return Meta.TypeId; }
    }

    void IEntitySerializer.OnRender() {
      for (int i = 0; i < Meta.Properties.Length; ++i) {
        var p = Meta.Properties[i];
        p.Property.OnRender(Objects[p.OffsetObjects]);
      }
    }

    void IEntitySerializer.OnInitialized() {
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
          Frames.Next(Frames.first).Changed.Combine(Frames.first.Changed);

          // free it
          FreeStorage(Frames.RemoveFirst());
        }
      }

      InvokeCallbacks();

      for (int i = 0; i < Meta.Properties.Length; ++i) {
        var p = Meta.Properties[i];
        p.Property.OnSimulateBefore(Objects[p.OffsetObjects]);
      }
    }

    void IEntitySerializer.OnSimulateAfter() {
      for (int i = 0; i < Meta.Properties.Length; ++i) {
        var p = Meta.Properties[i];
        p.Property.OnSimulateAfter(Objects[p.OffsetObjects]);
      }

      InvokeCallbacks();
    }

    void IEntitySerializer.OnControlGained() {
      while (Frames.count > 1) {
        // compact all changes into last frame
        Frames.last.Changed.Combine(Frames.first.Changed);

        // remove first frame
        FreeStorage(Frames.RemoveFirst());
      }
    }

    void IEntitySerializer.OnControlLost() {
      Assert.True(Frames.count == 1);
      Frames.first.Frame = Entity.Frame;
    }

    BitSet IEntitySerializer.GetDefaultMask() {
      return PropertyDefaultMask;
    }

    BitSet IEntitySerializer.GetFilter(BoltConnection connection, EntityProxy proxy) {
      if (Entity.IsController(connection)) {
        return Meta.Filters[32];
      }

      return Meta.Filters[31];
    }

    void IEntitySerializer.DebugInfo() {

    }

    void IEntitySerializer.InitProxy(EntityProxy p) {
      p.PropertyPriority = new Priority[Meta.CountProperties];

      for (int i = 0; i < p.PropertyPriority.Length; ++i) {
        p.PropertyPriority[i].PropertyIndex = i;
      }
    }

    int IEntitySerializer.Pack(BoltConnection connection, UdpPacket stream, EntityProxyEnvelope env) {
      BitSet filter = ((IEntitySerializer)this).GetFilter(connection, env.Proxy);

      if (Meta.PropertiesTempPriority == null) {
        Meta.PropertiesTempPriority = new Priority[Meta.CountProperties];
      }

      int tempCount = 0;

      Priority[] tempPriority = Meta.PropertiesTempPriority;
      Priority[] proxyPriority = env.Proxy.PropertyPriority;

      for (int i = 0; i < proxyPriority.Length; ++i) {
        Assert.True(proxyPriority[i].PropertyIndex == i);

        // if this property is set both in our filter and the proxy mask we can consider it for sending
        if (filter.IsSet(i) && env.Proxy.Changed.IsSet(i)) {
          // increment priority for this property
          proxyPriority[i].PriorityValue += Meta.Properties[i].Property.PropertyPriority;

          // copy to our temp array
          tempPriority[tempCount] = proxyPriority[i];

          // increment temp count
          tempCount += 1;
        }
      }

      // sort temp array based on priority
      Array.Sort<Priority>(tempPriority, 0, tempCount, Priority.Comparer.Instance);

      // write into stream
      PackProperties(connection, stream, env, tempPriority, tempCount);

      for (int i = 0; i < env.Written.Count; ++i) {
        Priority p = env.Written[i];

        // clear priority for written property
        env.Proxy.PropertyPriority[p.PropertyIndex].PriorityValue = 0;

        // clear mask for it
        env.Proxy.Changed.Clear(p.PropertyIndex);
      }

      return env.Written.Count;
    }

    void PackProperties(BoltConnection connection, UdpPacket packet, EntityProxyEnvelope env, Priority[] priority, int count) {
      int propertyCountPtr = packet.Ptr;
      packet.WriteByte(0, PacketMaxPropertiesBits);

      // how many bits can we write at the most
      int bits = System.Math.Min(Meta.PacketMaxBits, packet.Size - packet.Position);

      for (int i = 0; i < count; ++i) {
        // this means we can even fit another property id
        if (bits <= PacketPropertyIdBits) {
          break;
        }

        // we have written enough properties
        if (env.Written.Count == Meta.PacketMaxProperties) {
          break;
        }

        Priority p = priority[i];
        NetworkPropertyInfo pi = Meta.Properties[p.PropertyIndex];

        if (p.PriorityValue == 0) {
          break;
        }

        int b = PacketPropertyIdBits + pi.Property.BitCount(Objects[pi.OffsetObjects]);
        int ptr = packet.Ptr;

        if (bits >= b) {
          // write property id
          packet.WriteInt(p.PropertyIndex, PacketPropertyIdBits);

          if (pi.Property.Write(connection, Objects[pi.OffsetObjects], Storage, packet)) {
#if DEBUG
            int totalBits = packet.Position - ptr;
            if (totalBits != b) {
              BoltLog.Warn("Property of type {0} did not write the correct amount of bits, written: {1}, expected: {2}", pi.Property, totalBits, b);
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
      UdpPacket.WriteByteAt(packet.Data, propertyCountPtr, PacketMaxPropertiesBits, (byte)env.Written.Count);
    }

    void IEntitySerializer.Read(BoltConnection connection, UdpPacket packet, int frame) {
      int count = packet.ReadByte(PacketMaxPropertiesBits);
      var storage = default(NetworkStorage);

      if (Frames.count == 0) {
        storage = AllocateStorage();
        storage.Frame = frame;

        Frames.AddLast(storage);

        if (Entity.HasPredictedControl) {
          storage.Frame = BoltCore.frame;
        }
      }
      else {
        if (Entity.HasPredictedControl) {
          Assert.True(Frames.count == 1);

          storage = Frames.first;
          storage.Frame = BoltCore.frame;
        }
        else {
          storage = DuplicateStorage(Frames.last);
          storage.Frame = frame;
          storage.Changed.ClearAll();
          Frames.AddLast(storage);
        }
      }

      while (--count >= 0) {
        var propertyIndex = packet.ReadInt(PacketPropertyIdBits);
        var propertyInfo = Meta.Properties[propertyIndex];

		// make sure this is the correct one
        Assert.True(propertyIndex == Objects[propertyInfo.OffsetObjects].OffsetProperties + propertyInfo.Property.OffsetProperties);

        // read data into frame
        propertyInfo.Property.Read(connection, Objects[propertyInfo.OffsetObjects], storage, packet);

        // set changed flag
        storage.Changed.Set(propertyIndex);
      }
    }
  }
}

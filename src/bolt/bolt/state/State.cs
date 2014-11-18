using System;
using System.Linq;
using System.Collections.Generic;
using UdpKit;
using UE = UnityEngine;
using System.Text;

namespace Bolt {

  /// <summary>
  /// Base interface for all states
  /// </summary>
  [Documentation]
  public interface IState {
    UE.Animator Animator {
      get;
    }

    /// <summary>
    /// Set the animator object this state should use for reading/writing mecanim parameters
    /// </summary>
    /// <param name="animator">The animator object to use</param>
    void SetAnimator(UE.Animator animator);

    /// <summary>
    /// Allows you to hook up a callback to a specific property
    /// </summary>
    /// <param name="path">The path of the property</param>
    /// <param name="callback">The callback delegate</param>
    void AddCallback(string path, PropertyCallback callback);

    /// <summary>
    /// Allows you to hook up a callback to a specific property
    /// </summary>
    /// <param name="path">The path of the property</param>
    /// <param name="callback">The callback delegate</param>
    void AddCallback(string path, PropertyCallbackSimple callback);

    /// <summary>
    /// Removes a callback from a property
    /// </summary>
    /// <param name="path">The path of the property</param>
    /// <param name="callback">The callback delegate to remove</param>
    void RemoveCallback(string path, PropertyCallback callback);

    /// <summary>
    /// Removes a callback from a property
    /// </summary>
    /// <param name="path">The path of the property</param>
    /// <param name="callback">The callback delegate to remove</param>
    void RemoveCallback(string path, PropertyCallbackSimple callback);

    /// <summary>
    /// Set a property dynamically by string name
    /// </summary>
    /// <param name="property"></param>
    /// <param name="value"></param>
    void SetDynamic(string property, object value);
  }

  internal abstract class State : IState, IEntitySerializer {
    internal Entity Entity;
    internal UE.Animator Animator;

    internal readonly int PropertyIdBits;
    internal readonly int PacketMaxPropertiesBits;
    internal readonly object[] Objects;

    internal readonly BoltDoubleList<NetworkFrame> Frames = new BoltDoubleList<NetworkFrame>();
    internal readonly Dictionary<string, List<PropertyCallback>> Callbacks = new Dictionary<string, List<PropertyCallback>>();
    internal readonly Dictionary<string, List<PropertyCallbackSimple>> CallbacksSimple = new Dictionary<string, List<PropertyCallbackSimple>>();

    internal readonly Priority[] TempPriority;
    internal readonly StateMetaData MetaData;

    internal BitSet DefaultMask;

    public TypeId TypeId {
      get { return MetaData.TypeId; }
    }

    public NetworkFrame CurrentFrame {
      get { return Frames.first; }
    }

    protected State(StateMetaData meta) {
      MetaData = meta;
      MetaData.PacketMaxProperties = System.Math.Max(System.Math.Min(MetaData.PacketMaxProperties, 255), 1);

      Objects = new object[MetaData.SerializerGroup.ObjectsCount];
      TempPriority = new Priority[MetaData.SerializerGroup.Serializers.Count];

      PropertyIdBits = Bolt.Math.BitsRequired(MetaData.SerializerGroup.Serializers.Count);
      PacketMaxPropertiesBits = Bolt.Math.BitsRequired(MetaData.PacketMaxProperties);

      SetupObjects();

#if DEBUG
      for (int i = 0; i < Objects.Length; ++i) {
        Assert.NotNull(Objects[i]);
      }
#endif
    }

    UE.Animator IState.Animator {
      get { return Animator; }
    }

    protected abstract void SetupObjects();

    public void DebugInfo() {
      if (BoltNetworkInternal.DebugDrawer != null) {
        BoltNetworkInternal.DebugDrawer.LabelBold("State Info");
        BoltNetworkInternal.DebugDrawer.LabelField("Animator", Animator ? Animator.gameObject.name : "NOT ASSIGNED");
        BoltNetworkInternal.DebugDrawer.LabelField("State Type", Factory.GetFactory(TypeId).TypeObject);
        BoltNetworkInternal.DebugDrawer.LabelField("Frame Buffer Size", Frames.count.ToString());
        BoltNetworkInternal.DebugDrawer.LabelBold("State Properties");

        for (int i = 0; i < MetaData.SerializerGroup.Serializers.Count; ++i) {
          string label = MetaData.SerializerGroup.Serializers[i].Settings.PropertyFullPath;
          object value = MetaData.SerializerGroup.Serializers[i].GetDebugValue(this);

          BoltNetworkInternal.DebugDrawer.Indent(label.Count(c => c == '.' || c == '['));

          if (value != null) {
            BoltNetworkInternal.DebugDrawer.LabelField(label, value.ToString());
          }
          else {
            BoltNetworkInternal.DebugDrawer.LabelField(label, "N/A");
          }

          BoltNetworkInternal.DebugDrawer.Indent(0);
        }
      }
    }

    public void OnControlGained() {
      if (!Entity.IsOwner) {
        while (Frames.count > 1) {
          MetaData.FramePool.Free(Frames.RemoveFirst());
        }
      }
    }

    public void OnControlLost() {
      if (!Entity.IsOwner) {
        Assert.True(Frames.count == 1);
        Frames.first.Number = Entity.Frame;
      }
    }

    public void SetAnimator(UE.Animator animator) {
      Animator = animator;

      if (Animator) {
        if (Animator.updateMode != UE.AnimatorUpdateMode.AnimatePhysics) {
          BoltLog.Warn("Animator for '{0}' is not set to 'AnimatePhysics', this might cause Bolt to miss values and triggers being updated.", animator.gameObject);
        }

        if (!Entity.IsOwner && Animator.applyRootMotion) {
          BoltLog.Warn("Animator for '{0}' has root motion enabled on a remote entity, auto-disabling it");
          Animator.applyRootMotion = false;
        }
      }
    }

    bool VerifyCallbackPath(string path) {
      if (MetaData.SerializerGroup.SerializerPaths.Contains(path)) {
        return true;
      }

      BoltLog.Error("No callback path '{0}' available on {1}", path, this);
      return false;
    }

    public void AddCallback(string path, PropertyCallbackSimple callback) {
      if (VerifyCallbackPath(path)) {
        List<PropertyCallbackSimple> callbacksList;

        if (CallbacksSimple.TryGetValue(path, out callbacksList) == false) {
          CallbacksSimple[path] = callbacksList = new List<PropertyCallbackSimple>(32);
        }

        callbacksList.Add(callback);
      }
    }

    public void AddCallback(string path, PropertyCallback callback) {
      if (VerifyCallbackPath(path)) {
        List<PropertyCallback> callbacksList;

        if (Callbacks.TryGetValue(path, out callbacksList) == false) {
          Callbacks[path] = callbacksList = new List<PropertyCallback>(32);
        }

        callbacksList.Add(callback);
      }
    }

    public void RemoveCallback(string path, PropertyCallback callback) {
      if (VerifyCallbackPath(path)) {
        List<PropertyCallback> callbacksList;

        if (Callbacks.TryGetValue(path, out callbacksList) == false) {
          callbacksList.Remove(callback);
        }
      }
    }

    public void RemoveCallback(string path, PropertyCallbackSimple callback) {
      if (VerifyCallbackPath(path)) {
        List<PropertyCallbackSimple> callbacksList;

        if (CallbacksSimple.TryGetValue(path, out callbacksList) == false) {
          callbacksList.Remove(callback);
        }
      }
    }

    public BitSet GetDefaultMask() {
      return DefaultMask;
    }

    public void InitProxy(EntityProxy p) {
      p.PropertyPriority = new Priority[MetaData.SerializerGroup.Serializers.Count];

      for (int i = 0; i < p.PropertyPriority.Length; ++i) {
        p.PropertyPriority[i].PropertyIndex = i;
      }
    }

    public void OnRender() {
      for (int i = 0; i < MetaData.SerializerGroup.Serializers.Count; ++i) {
        MetaData.SerializerGroup.Serializers[i].OnRender(this, this.Frames.first);
      }
    }

    public void OnInitialized() {
      if (Entity.IsOwner) {
        Frames.AddLast(MetaData.FramePool.Allocate(this, BoltCore.frame));
      }

      for (int i = 0; i < MetaData.SerializerGroup.Serializers.Count; ++i) {
        MetaData.SerializerGroup.Serializers[i].OnInit(this);
      }
    }

    public void OnCreated(Entity entity) {
      Entity = entity;
    }

    public void OnParentChanging(Entity newParent, Entity oldParent) {
      for (int i = 0; i < MetaData.SerializerGroup.Serializers.Count; ++i) {
        MetaData.SerializerGroup.Serializers[i].OnParentChanged(this, newParent, oldParent);
      }
    }

    public void OnSimulateBefore() {

      if (Entity.IsOwner || Entity.HasPredictedControl) {
        Frames.first.Number = BoltCore.frame;
      }
      else {
        while ((Frames.count > 1) && (Entity.Frame >= Frames.Next(Frames.first).Number)) {
          // trigger callbacks for disposed frame
          InvokeCallbacks(Frames.first);

          // remove
          MetaData.FramePool.Free(Frames.RemoveFirst());
        }
      }

      InvokeCallbacks(Frames.first);

      for (int i = 0; i < MetaData.SerializerGroup.Serializers.Count; ++i) {
        MetaData.SerializerGroup.Serializers[i].OnSimulateBefore(this);
      }
    }

    public void OnSimulateAfter() {
      for (int i = 0; i < MetaData.SerializerGroup.Serializers.Count; ++i) {
        MetaData.SerializerGroup.Serializers[i].OnSimulateAfter(this);
      }

      InvokeCallbacks(Frames.first);
    }

    void InvokeCallbacks(NetworkFrame frame) {
      if (frame.Changed.IsZero) {
        return;
      }

      var bits = frame.Changed.GetIterator();
      var serializer = -1;

      while (bits.Next(out serializer)) {
        InvokeCallbacksForProperty(MetaData.SerializerGroup.Serializers[serializer]);
      }

      if (Entity.Proxies.count > 0) {
        var proxies = Entity.Proxies.GetIterator();

        while (proxies.Next()) {
          proxies.val.Changed.Combine(frame.Changed);
        }
      }

      frame.Changed.ClearAll();
    }

    void InvokeCallbacksForProperty(PropertySerializer p) {
      for (int i = 0; i < p.Settings.PropertyPaths.Count; ++i) {
        {
          List<PropertyCallback> callbacksList;

          if (Callbacks.TryGetValue(p.Settings.PropertyPaths[i], out callbacksList)) {
            for (int n = 0; n < callbacksList.Count; ++n) {
              callbacksList[n](this, p.Settings.PropertyFullPath, p.Settings.ArrayIndices);
            }
          }
        }
        {
          List<PropertyCallbackSimple> callbacksList;

          if (CallbacksSimple.TryGetValue(p.Settings.PropertyPaths[i], out callbacksList)) {
            for (int n = 0; n < callbacksList.Count; ++n) {
              callbacksList[n]();
            }
          }
        }
      }
    }

    public int Pack(BoltConnection connection, UdpPacket stream, EntityProxyEnvelope env) {
      BitSet filter = GetFilter(connection, env.Proxy);

      int tempCount = 0;

      Priority[] tempPriority = TempPriority;
      Priority[] proxyPriority = env.Proxy.PropertyPriority;

      for (int i = 0; i < proxyPriority.Length; ++i) {
        Assert.True(proxyPriority[i].PropertyIndex == i);

        // if this property is set both in our filter and the proxy mask we can consider it for sending
        if (filter.IsSet(i) && env.Proxy.Changed.IsSet(i)) {
          // increment priority for this property
          proxyPriority[i].PriorityValue += MetaData.SerializerGroup.Serializers[i].Settings.PropertyPriority;

          // copy to our temp array
          tempPriority[tempCount] = proxyPriority[i];

          // increment temp count
          tempCount += 1;
        }
      }

      // sort temp array based on priority
      if (tempCount > 0) {
        Array.Sort<Priority>(tempPriority, 0, tempCount, Priority.Comparer.Instance);
      }

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

    void PackProperties(BoltConnection connection, UdpPacket stream, EntityProxyEnvelope env, Priority[] priority, int priorityCount) {
      int propertyCountPtr = stream.Ptr;
      stream.WriteByte(0, PacketMaxPropertiesBits);

      // how many bits can we write at the most
      int bits = System.Math.Min(MetaData.PacketMaxBits, stream.Size - stream.Position);

      for (int i = 0; i < priorityCount; ++i) {

        // this means we can even fit another property id
        if (bits <= PropertyIdBits) {
          break;
        }

        // we have written enough properties
        if (env.Written.Count == MetaData.PacketMaxProperties) {
          break;
        }

        Priority p = priority[i];
        PropertySerializer s = MetaData.SerializerGroup.Serializers[p.PropertyIndex];

        if (p.PriorityValue == 0) {
          break;
        }

        int b = PropertyIdBits + s.StateBits(this, Frames.first);
        int ptr = stream.Ptr;

        if (bits >= b) {
          // write property id
          stream.WriteInt(p.PropertyIndex, PropertyIdBits);

          // write data into stream
          if (s.StatePack(this, Frames.first, connection, stream)) {
#if DEBUG
            int totalBits = stream.Position - ptr;
            if (totalBits != b) {
              BoltLog.Warn("Property of type {0} did not write the correct amount of bits, written: {1}, expected: {2}", s, totalBits, b);
            }
#endif
            // use up bits
            bits -= b;

            // add to written list
            env.Written.Add(p);
          }
          else {
            // reset position
            stream.Ptr = ptr;
          }
        }
      }

      // gotta be less then 256
      Assert.True(env.Written.Count <= MetaData.PacketMaxProperties);

      // write the amount of properties
      UdpPacket.WriteByteAt(stream.Data, propertyCountPtr, PacketMaxPropertiesBits, (byte)env.Written.Count);
    }

    public void Read(BoltConnection connection, UdpPacket stream, int frameNumber) {
      int count = stream.ReadByte(PacketMaxPropertiesBits);
      var frame = default(NetworkFrame);

      if (Frames.count == 0) {
        frame = MetaData.FramePool.Allocate(this, frameNumber);
        Frames.AddLast(frame);
      }
      else {
        if (Entity.HasPredictedControl) {
          Assert.True(Frames.count == 1);

          frame = Frames.first;
          frame.Number = BoltCore.frame;
        }
        else {
          frame = MetaData.FramePool.Duplicate(Frames.last, frameNumber);
          Frames.AddLast(frame);
        }
      }

      while (--count >= 0) {
        int property = stream.ReadInt(PropertyIdBits);
        var serializer = MetaData.SerializerGroup.Serializers[property];

        // read data into frame
        serializer.StateRead(this, frame, connection, stream);
      }
    }

    public BitSet GetFilter(BoltConnection connection, EntityProxy proxy) {
      if (Entity.IsController(connection)) {
        return MetaData.ControllerFilter;
      }

      return BitSet.Full;
    }

    public void SetDynamic(string property, object value) {
      for (int i = 0; i < MetaData.SerializerGroup.Serializers.Count; ++i) {
        if (MetaData.SerializerGroup.Serializers[i].Settings.PropertyFullPath == property) {
          MetaData.SerializerGroup.Serializers[i].SetDynamic(Frames.first, value);
          break;
        }
      }
    }
  }
}

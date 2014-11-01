using System;
using System.Linq;
using System.Collections.Generic;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  public delegate void PropertyCallback(IState state, string propertyPath, ArrayIndices arrayIndices);
  public delegate void PropertyCallbackSimple();

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
  }

  public interface IStateModifier : IDisposable {

  }

  internal abstract class State : IState, IEntitySerializer {
    internal struct StateMetaData {
      public TypeId TypeId;

      public int FrameSize;
      public int ObjectCount;
      public int PropertyCount;
      public int PacketMaxBits;
      public int PacketMaxProperties;

      public BitArray[] PropertyFilters;
      public BitArray PropertyControllerFilter;
      public PropertySerializer[] PropertySerializers;
      public Dictionary<Filter, BitArray> PropertyFilterCache;
      public HashSet<string> PropertyCallbackPaths;
    }

    public class Frame : IBoltListNode {
      public int Number;
      public State State;
      public object[] Objects;
      public readonly byte[] Data;

      public Frame(int number, int size) {
        Number = number;
        Data = new byte[size];
      }

      public Frame Duplicate(int frameNumber) {
        Frame clone;

        clone = new Frame(frameNumber, Data.Length);
        clone.Objects = Objects;
        clone.State = State;

        Array.Copy(Data, 0, clone.Data, 0, Data.Length);

        return clone;
      }

      object IBoltListNode.prev {
        get;
        set;
      }

      object IBoltListNode.next {
        get;
        set;
      }

      object IBoltListNode.list {
        get;
        set;
      }
    }

    internal Entity Entity;
    internal UE.Animator Animator;

    internal readonly int PropertyIdBits;
    internal readonly int PacketMaxPropertiesBits;
    internal readonly object[] PropertyObjects;

    internal readonly Frame NullFrame;
    internal readonly Frame DiffFrame;
    internal readonly BoltDoubleList<Frame> Frames = new BoltDoubleList<Frame>();
    internal readonly Dictionary<string, List<PropertyCallback>> Callbacks = new Dictionary<string, List<PropertyCallback>>();
    internal readonly Dictionary<string, List<PropertyCallbackSimple>> CallbacksSimple = new Dictionary<string, List<PropertyCallbackSimple>>();

    internal readonly BitArray FullMask;
    internal readonly BitArray DiffMask;
    internal readonly Priority[] TempPriority;
    internal readonly StateMetaData MetaData;

    public TypeId TypeId {
      get { return MetaData.TypeId; }
    }

    protected State(StateMetaData meta) {
      meta.PacketMaxProperties = System.Math.Max(System.Math.Min(meta.PacketMaxProperties, 255), 1);

      MetaData = meta;
      DiffFrame = AllocFrame(-1);
      NullFrame = AllocFrame(-1);

      FullMask = BitArray.CreateSet(MetaData.PropertyCount);
      DiffMask = BitArray.CreateClear(MetaData.PropertyCount);
      TempPriority = new Priority[MetaData.PropertyCount];

      PropertyIdBits = Bolt.Math.BitsRequired(MetaData.PropertyCount);
      PropertyObjects = new object[MetaData.ObjectCount];
      PacketMaxPropertiesBits = Bolt.Math.BitsRequired(MetaData.PacketMaxProperties);
    }

    UE.Animator IState.Animator {
      get { return Animator; }
    }

    public void DebugInfo() {
      if (BoltNetworkInternal.DebugDrawer != null) {
        BoltNetworkInternal.DebugDrawer.LabelBold("State Info");
        BoltNetworkInternal.DebugDrawer.LabelField("State Type", Factory.GetFactory(TypeId).TypeObject);
        BoltNetworkInternal.DebugDrawer.LabelField("Frame Buffer Size", Frames.count.ToString());
        BoltNetworkInternal.DebugDrawer.LabelBold("State Properties");

        for (int i = 0; i < MetaData.PropertySerializers.Length; ++i) {
          string label = MetaData.PropertySerializers[i].StateSettings.PropertyPath;
          object value = MetaData.PropertySerializers[i].GetDebugValue(this);

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
          FreeFrame(Frames.RemoveFirst());
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
      if (MetaData.PropertyCallbackPaths.Contains(path)) {
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
      BoltLog.Debug("Added callbacks for '{0}', total callbacks: {1}", path, CallbacksSimple.Select(x => x.Value.Count).Sum());
    }

    public void AddCallback(string path, PropertyCallback callback) {
      BoltLog.Debug("Adding callback for {0}", path);

      if (VerifyCallbackPath(path)) {
        List<PropertyCallback> callbacksList;

        if (Callbacks.TryGetValue(path, out callbacksList) == false) {
          Callbacks[path] = callbacksList = new List<PropertyCallback>(32);
        }

        callbacksList.Add(callback);
      }

      BoltLog.Debug("Added callbacks for '{0}', total callbacks: {1}", path, Callbacks.Select(x => x.Value.Count).Sum());
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

    public BitArray GetDefaultMask() {
      if (Frames.count == 0) {
        return BitArray.CreateClear(MetaData.PropertyCount);
      }

      return Diff(Frames.first, NullFrame).Clone();
    }

    public void InitProxy(EntityProxy p) {
      p.PropertyPriority = new Priority[MetaData.PropertyCount];

      for (int i = 0; i < p.PropertyPriority.Length; ++i) {
        p.PropertyPriority[i].PropertyIndex = i;
      }
    }

    public void OnRender() {
      for (int i = 0; i < MetaData.PropertySerializers.Length; ++i) {
        MetaData.PropertySerializers[i].OnRender(this, this.Frames.first);
      }
    }

    public void OnInitialized() {
      if (Entity.IsOwner) {
        Frames.AddLast(AllocFrame(BoltCore.frame));
      }

      for (int i = 0; i < MetaData.PropertySerializers.Length; ++i) {
        MetaData.PropertySerializers[i].OnInit(this);
      }
    }

    public void OnCreated(Entity entity) {
      Entity = entity;
    }

    public void OnParentChanging(Entity newParent, Entity oldParent) {
      for (int i = 0; i < MetaData.PropertySerializers.Length; ++i) {
        MetaData.PropertySerializers[i].OnParentChanged(this, newParent, oldParent);
      }
    }

    public void OnSimulateBefore() {
      InvokeCallbacks();

      if (Entity.IsOwner || Entity.HasPredictedControl) {
        Frames.first.Number = BoltCore.frame;
      }
      else {
        while ((Frames.count > 1) && (Entity.Frame >= Frames.Next(Frames.first).Number)) {
          FreeFrame(Frames.RemoveFirst());
        }
      }

      for (int i = 0; i < MetaData.PropertySerializers.Length; ++i) {
        MetaData.PropertySerializers[i].OnSimulateBefore(this);
      }
    }

    public void OnSimulateAfter() {
      for (int i = 0; i < MetaData.PropertySerializers.Length; ++i) {
        MetaData.PropertySerializers[i].OnSimulateAfter(this);
      }

      InvokeCallbacks();
    }

    void InvokeCallbacks() {
      // calculate diff mask
      var diff = Diff(Frames.first, DiffFrame);

      // copy data from latest frame to diff buffer
      Array.Copy(Frames.first.Data, 0, DiffFrame.Data, 0, Frames.first.Data.Length);

      // combine with existing masks for proxies
      var it = Entity.Proxies.GetIterator();

      while (it.Next()) {
        it.val.Mask.OrAssign(diff);
      }

      // raise local changed events
      for (int i = 0; i < MetaData.PropertySerializers.Length; ++i) {
        if (diff.IsSet(i)) {
          InvokeCallbacksForProperty(MetaData.PropertySerializers[i]);
        }
      }
    }

    void InvokeCallbacksForProperty(PropertySerializer p) {
      for (int i = 0; i < p.StateSettings.CallbackPaths.Length; ++i) {
        {
          List<PropertyCallback> callbacksList;

          if (Callbacks.TryGetValue(p.StateSettings.CallbackPaths[i], out callbacksList)) {
            for (int n = 0; n < callbacksList.Count; ++n) {
              callbacksList[n](this, p.StateSettings.PropertyPath, p.StateSettings.CallbackIndices);
            }
          }
        }
        {
          List<PropertyCallbackSimple> callbacksList;

          if (CallbacksSimple.TryGetValue(p.StateSettings.CallbackPaths[i], out callbacksList)) {
            for (int n = 0; n < callbacksList.Count; ++n) {
              callbacksList[n]();
            }
          }
        }
      }
    }

    public int Pack(BoltConnection connection, UdpStream stream, EntityProxyEnvelope env) {
      BitArray filter = GetFilter(connection, env.Proxy);

      int tempCount = 0;

      Priority[] tempPriority = TempPriority;
      Priority[] proxyPriority = env.Proxy.PropertyPriority;

      for (int i = 0; i < proxyPriority.Length; ++i) {
        Assert.True(proxyPriority[i].PropertyIndex == i);

        // if this property is set both in our filter and the proxy mask we can consider it for sending
        if (env.Proxy.Mask.IsSet(proxyPriority[i].PropertyIndex)) {

          // increment priority for this property
          proxyPriority[i].PriorityValue += MetaData.PropertySerializers[i].StateSettings.Priority;

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
        env.Proxy.Mask.Clear(p.PropertyIndex);
      }

      return env.Written.Count;
    }

    void PackProperties(BoltConnection connection, UdpStream stream, EntityProxyEnvelope env, Priority[] priority, int priorityCount) {
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
        PropertySerializer s = MetaData.PropertySerializers[p.PropertyIndex];

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
      UdpStream.WriteByteAt(stream.Data, propertyCountPtr, PacketMaxPropertiesBits, (byte)env.Written.Count);
    }

    public void Read(BoltConnection connection, UdpStream stream, int frameNumber) {
      int count = stream.ReadByte(PacketMaxPropertiesBits);
      var frame = default(Frame);

      if (Frames.count == 0) {
        frame = AllocFrame(frameNumber);
        Frames.AddLast(frame);
      }
      else {
        if (Entity.HasPredictedControl) {
          Assert.True(Frames.count == 1);

          frame = Frames.first;
          frame.Number = BoltCore.frame;
        }
        else {
          frame = Frames.last.Duplicate(frameNumber);
          Frames.AddLast(frame);
        }
      }

      while (--count >= 0) {
        int property = stream.ReadInt(PropertyIdBits);
        var serializer = MetaData.PropertySerializers[property];

        // read data into frame
        serializer.StateRead(this, frame, connection, stream);
      }
    }

    public BitArray GetFilter(BoltConnection connection, EntityProxy proxy) {
      if (Entity.IsController(connection)) {
        return MetaData.PropertyControllerFilter;
      }

      return CalculateFilter(proxy.Filter);
    }

    BitArray Diff(Frame a, Frame b) {
      DiffMask.Clear();

      int L = MetaData.PropertySerializers.Length;

      for (int i = 0; i < L; ++i) {
        PropertySerializer s = MetaData.PropertySerializers[i];

        if (Blit.Diff(a.Data, b.Data, s.Settings.ByteOffset, s.StateSettings.ByteLength)) {
          DiffMask.Set(i);
        }
      }

      return DiffMask;
    }

    Frame AllocFrame(int number) {
      Frame f;

      f = new Frame(number, MetaData.FrameSize);
      f.Objects = PropertyObjects;
      f.State = this;

      return f;
    }

    void FreeFrame(Frame frame) {

    }

    BitArray CalculateFilter(Filter filter) {
      return FullMask;
    }




  }
}

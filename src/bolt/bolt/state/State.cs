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


    void SetDynamic(string property, object value);
  }

  public interface IStateModifier : IDisposable {

  }

  internal class DataFrame {
    object _object;

    public int Number;
    public readonly NetworkValue[] Values;

    public State State { get { return (State)_object; } set { _object = value; } }
    public Event Event { get { return (Event)_object; } set { _object = value; } }
    public Command Command { get { return (Command)_object; } set { _object = value; } }

    public DataFrame(int size) {
      Values = new NetworkValue[size];
    }
  }

  internal class DataFramePool {

    public DataFramePool(int frameSize) {

    }

  }

  internal abstract class State : IState, IEntitySerializer {
    internal struct StateMetaData {
      public TypeId TypeId;

      public int FrameSize;
      public int ObjectCount;
      public int PropertyCount;
      public int PacketMaxBits;
      public int PacketMaxProperties;

      public Block[] PropertyBlocks;
      public int[] PropertyBlocksResult;

      public FramePool FramePool;
      public BitArray[] PropertyFilters;
      public BitArray PropertyControllerFilter;
      public PropertySerializer[] PropertySerializers;
      public PropertySerializer[] PropertySerializersOnRender;
      public PropertySerializer[] PropertySerializersOnSimulateAfter;
      public PropertySerializer[] PropertySerializersOnSimulateBefore;
      public Dictionary<Filter, BitArray> PropertyFilterCache;
      public HashSet<string> PropertyCallbackPaths;
    }

    public class FramePool {
      public readonly int Size;
      public readonly Stack<Frame> Pool = new Stack<Frame>();

      public FramePool(int frameSize) {
        Size = frameSize;
      }

      public Frame Allocate(State state, int number) {
        Frame f;

        if (Pool.Count > 0) {
          f = Pool.Pop();
          Assert.True(f.Pooled);
        }
        else {
          f = new Frame(0, Size);
        }

        f.Pooled = false;
        f.Number = number;

        f.State = state;
        f.Objects = f.State.PropertyObjects;

        return f;
      }

      public void Free(Frame f) {
        Assert.False(f.Pooled);
        Assert.True(f.Data.Length == Size);

        Array.Clear(f.Data, 0, Size);

        Pool.Push(f);

        f.Pooled = true;
      }

      public Frame Duplicate(Frame f, int number) {
        Frame c = Allocate(f.State, number);

        Assert.True(f.Data.Length == c.Data.Length);
        Buffer.BlockCopy(f.Data, 0, c.Data, 0, f.Data.Length);

        return c;
      }
    }

    public class Frame : IBoltListNode {
      public int Number;
      public bool Pooled;
      public bool Changed;

      public State State;

      public object[] Objects;
      public readonly byte[] Data;

      public Frame(int number, int size) {
        Number = number;
        Data = new byte[size];
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
      MetaData = meta;
      MetaData.PacketMaxProperties = System.Math.Max(System.Math.Min(MetaData.PacketMaxProperties, 255), 1);

      FullMask = BitArray.CreateSet(MetaData.PropertyCount);
      DiffMask = BitArray.CreateClear(MetaData.PropertyCount);
      TempPriority = new Priority[MetaData.PropertyCount];

      PropertyIdBits = Bolt.Math.BitsRequired(MetaData.PropertyCount);
      PropertyObjects = new object[MetaData.ObjectCount];
      PacketMaxPropertiesBits = Bolt.Math.BitsRequired(MetaData.PacketMaxProperties);

      DiffFrame = MetaData.FramePool.Allocate(this, -1);
      NullFrame = MetaData.FramePool.Allocate(this, -1);
    }

    UE.Animator IState.Animator {
      get { return Animator; }
    }

    public void DebugInfo() {
      if (BoltNetworkInternal.DebugDrawer != null) {
        BoltNetworkInternal.DebugDrawer.LabelBold("State Info");
        BoltNetworkInternal.DebugDrawer.LabelField("Animator", Animator ? Animator.gameObject.name : "NOT ASSIGNED");
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
      //BoltLog.Debug("Added callbacks for '{0}', total callbacks: {1}", path, CallbacksSimple.Select(x => x.Value.Count).Sum());
    }

    public void AddCallback(string path, PropertyCallback callback) {
      //BoltLog.Debug("Adding callback for {0}", path);

      if (VerifyCallbackPath(path)) {
        List<PropertyCallback> callbacksList;

        if (Callbacks.TryGetValue(path, out callbacksList) == false) {
          Callbacks[path] = callbacksList = new List<PropertyCallback>(32);
        }

        callbacksList.Add(callback);
      }

      //BoltLog.Debug("Added callbacks for '{0}', total callbacks: {1}", path, Callbacks.Select(x => x.Value.Count).Sum());
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

      int diffCount;
      return Diff(Frames.first, NullFrame, out diffCount).Clone();
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
        Frames.AddLast(MetaData.FramePool.Allocate(this, BoltCore.frame));
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
          MetaData.FramePool.Free(Frames.RemoveFirst());
        }
      }

      for (int i = 0; i < MetaData.PropertySerializersOnSimulateBefore.Length; ++i) {
        MetaData.PropertySerializersOnSimulateBefore[i].OnSimulateBefore(this);
      }
    }

    public void OnSimulateAfter() {
      for (int i = 0; i < MetaData.PropertySerializersOnSimulateAfter.Length; ++i) {
        MetaData.PropertySerializersOnSimulateAfter[i].OnSimulateAfter(this);
      }

      InvokeCallbacks();
    }

    void InvokeCallbacks() {
      //if (Frames.first.Changed == false) {
      //  return;
      //}

      // calculate diff mask
      var diffCount = 0;
      var diff = DiffFast(Frames.first, DiffFrame, out diffCount);

      // copy data from latest frame to diff buffer
      Buffer.BlockCopy(Frames.first.Data, 0, DiffFrame.Data, 0, Frames.first.Data.Length);

      // combine with existing masks for proxies
      for (int i = 0; i < diffCount; ++i) {
        // set on proxies
        var it = Entity.Proxies.GetIterator();

        while (it.Next()) {
          it.val.Mask.Set(diff[i]);
        }

        InvokeCallbacksForProperty(MetaData.PropertySerializers[diff[i]]);
      }

      Frames.first.Changed = false;
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

    public int Pack(BoltConnection connection, UdpPacket stream, EntityProxyEnvelope env) {
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
      UdpPacket.WriteByteAt(stream.Data, propertyCountPtr, PacketMaxPropertiesBits, (byte)env.Written.Count);
    }

    public void Read(BoltConnection connection, UdpPacket stream, int frameNumber) {
      int count = stream.ReadByte(PacketMaxPropertiesBits);
      var frame = default(Frame);

      if (Frames.count == 0) {
        frame = MetaData.FramePool.Allocate(this, frameNumber);
        frame.Changed = true;
        Frames.AddLast(frame);
      }
      else {
        if (Entity.HasPredictedControl) {
          Assert.True(Frames.count == 1);

          frame = Frames.first;
          frame.Changed = true;
          frame.Number = BoltCore.frame;
        }
        else {
          frame = MetaData.FramePool.Duplicate(Frames.last, frameNumber);
          frame.Changed = true;
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

    int[] DiffFast(Frame a, Frame b, out int count) {
      count = Blit.Diff(a.Data, b.Data, MetaData.PropertyBlocks, MetaData.PropertyBlocksResult);
      return MetaData.PropertyBlocksResult;
    }

    BitArray Diff(Frame a, Frame b, out int count) {
      DiffMask.Clear();

      count = Blit.Diff(a.Data, b.Data, MetaData.PropertyBlocks, MetaData.PropertyBlocksResult);

      for (int i = 0; i < count; ++i) {
        DiffMask.Set(MetaData.PropertyBlocksResult[i]);
      }

      //int L = MetaData.PropertySerializers.Length;

      //for (int i = 0; i < L; ++i) {
      //  PropertySerializer s = MetaData.PropertySerializers[i];

      //  if (Blit.DiffNative(a.Data, b.Data, s.Settings.ByteOffset, s.StateSettings.ByteLength)) {
      //    DiffMask.Set(i);
      //  }
      //}

      return DiffMask;
    }

    BitArray CalculateFilter(Filter filter) {
      return FullMask;
    }

    public void SetDynamic(string property, object value) {
      for (int i = 0; i < MetaData.PropertySerializers.Length; ++i) {
        if (MetaData.PropertySerializers[i].StateSettings.PropertyPath == property) {
          MetaData.PropertySerializers[i].SetDynamic(Frames.first, value);
          break;
        }
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  public delegate void PropertyCallback(IState state, string path, int[] indices);

  public interface IState {
    void SetAnimator(UE.Animator animator);
    void AddCallback(string path, PropertyCallback callback);
  }

  public interface IStateModifier : IDisposable {

  }

  public interface IStatePredictor : IDisposable {

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
    }

    public class Frame : IBoltListNode {
      public int Number;
      public State State;
      public object[] Objects;
      public readonly byte[] Data;
      public readonly List<int> ReadProperties;

      public Frame(int number, int size) {
        Number = number;
        Data = new byte[size];
        ReadProperties = new List<int>();
      }

      public Frame Duplicate(int frameNumber) {
        Frame f;

        f = new Frame(frameNumber, Data.Length);
        f.Objects = Objects;
        f.State = State;

        Array.Copy(Data, 0, f.Data, 0, Data.Length);

        return f;
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

    internal EntityObject Entity;
    internal UE.Animator Animator;

    internal readonly int PropertyIdBits;
    internal readonly int PacketMaxPropertiesBits;
    internal readonly object[] PropertyObjects;

    internal readonly Frame NullFrame;
    internal readonly Frame DiffFrame;
    internal readonly BoltDoubleList<Frame> Frames = new BoltDoubleList<Frame>();
    internal readonly Dictionary<string, List<PropertyCallback>> Callbacks = new Dictionary<string, List<PropertyCallback>>();

    internal readonly BitArray FullMask;
    internal readonly BitArray DiffMask;
    internal readonly Priority[] TempPriority;
    internal readonly StateMetaData MetaData;

    public TypeId TypeId {
      get { return MetaData.TypeId; }
    }

    protected State(StateMetaData meta) {
      meta.PacketMaxProperties = Math.Max(Math.Min(meta.PacketMaxProperties, 255), 1);

      MetaData = meta;
      DiffFrame = AllocFrame(-1);
      NullFrame = AllocFrame(-1);

      FullMask = BitArray.CreateSet(MetaData.PropertyCount);
      DiffMask = BitArray.CreateClear(MetaData.PropertyCount);
      TempPriority = new Priority[MetaData.PropertyCount];

      PropertyIdBits = BoltMath.BitsRequired(MetaData.PropertyCount - 1);
      PropertyObjects = new object[MetaData.ObjectCount];
      PacketMaxPropertiesBits = BoltMath.BitsRequired(MetaData.PacketMaxProperties);
    }

    public void DebugInfo() {
      BoltGUI.LabelText("Frame Buffer", Frames.count.ToString());
    }

    public void SetAnimator(UE.Animator animator) {
      Animator = animator;
    }

    public void AddCallback(string path, PropertyCallback callback) {
      List<PropertyCallback> callbacksList;

      if (Callbacks.TryGetValue(path, out callbacksList) == false) {
        Callbacks[path] = callbacksList = new List<PropertyCallback>(32);
      }

      callbacksList.Add(callback);
    }

    public virtual float CalculatePriority(BoltConnection connection, BitArray mask, int skipped) {
      return skipped;
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

    public void OnCreated(EntityObject entity) {
      Entity = entity;
    }

    public void OnSimulateBefore() {
      if (Entity.IsOwner) {
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
      // invoke simulate after on properties
      for (int i = 0; i < MetaData.PropertySerializers.Length; ++i) {
        MetaData.PropertySerializers[i].OnSimulateAfter(this);
      }

      // calculate diff mask
      var diff = Diff(Frames.first, DiffFrame);

      // combine with existing masks for proxies
      var it = Entity.Proxies.GetIterator();

      while (it.Next()) {
        it.val.Mask.OrAssign(diff);
      }

      // raise local changed events
      for (int i = 0; i < MetaData.PropertySerializers.Length; ++i) {
        if (diff.IsSet(i)) {
          InvokeCallbacks(MetaData.PropertySerializers[i]);
        }
      }

      // copy data from latest frame to diff buffer
      Array.Copy(Frames.first.Data, 0, DiffFrame.Data, 0, Frames.first.Data.Length);
    }

    void InvokeCallbacks(PropertySerializer p) {
      p.OnChanged(this, Frames.first);

      for (int i = 0; i < p.StateData.CallbackPaths.Length; ++i) {
        List<PropertyCallback> callbacksList;

        if (Callbacks.TryGetValue(p.StateData.CallbackPaths[i], out callbacksList)) {
          for (int n = 0; n < callbacksList.Count; ++n) {
            callbacksList[n](this, p.StateData.PropertyPath, p.StateData.CallbackIndices);
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
        if (BitArray.SetInBoth(filter, env.Proxy.Mask, i)) {

          // increment priority for this property
          proxyPriority[i].PriorityValue += MetaData.PropertySerializers[i].StateData.Priority;

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
      PackWrite(connection, stream, env, tempPriority, tempCount);

      for (int i = 0; i < env.Written.Count; ++i) {
        Priority p = env.Written[i];

        // clear priority for written property
        env.Proxy.PropertyPriority[p.PropertyIndex].PriorityValue = 0;

        // clear mask for it
        env.Proxy.Mask.Clear(p.PropertyIndex);
      }

      return env.Written.Count;
    }

    void PackWrite(BoltConnection connection, UdpStream stream, EntityProxyEnvelope env, Priority[] priority, int priorityCount) {
      int propertyCountPtr = stream.Ptr;
      stream.WriteByte(0, PacketMaxPropertiesBits);

      // how many bits can we write at the most
      int bits = Math.Min(MetaData.PacketMaxBits, stream.Size - stream.Position);

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
          BoltLog.Info("Serializing {0}", s.StateData.PropertyName);

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
      }
      else {
        if (Entity.HasControl && Entity.ControllerLocalPrediction) {
          frame = Frames.first;
          frame.Number = BoltCore.frame;
        }
        else {
          frame = Frames.last.Duplicate(frameNumber);
        }
      }

      while (--count >= 0) {
        int property = stream.ReadInt(PropertyIdBits);
        var serializer = MetaData.PropertySerializers[property];

        // read data into frame
        serializer.StateRead(this, frame, connection, stream);

        // put property index into updated list
        frame.ReadProperties.Add(property);
      }

      Frames.AddLast(frame);
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

        if (Blit.Diff(a.Data, b.Data, s.StateData.ByteOffset, s.StateData.ByteLength)) {
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

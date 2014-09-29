using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  public delegate void StateCallback(IState state, string path, int[] indices);

  public interface IState {
    void AddCallback(string path, StateCallback callback);
  }

  public interface IStateModifier : IDisposable {

  }

  public interface IStatePredictor : IDisposable {

  }


  internal abstract class State : IState, IEntitySerializer {
    protected struct StateMetaData {
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

    protected readonly int PropertyIdBits;
    protected readonly int PacketMaxPropertiesBits;
    protected readonly object[] PropertyObjects;

    protected readonly Frame NullFrame;
    protected readonly Frame DiffFrame;
    protected readonly Frame SendFrame;
    protected readonly BoltDoubleList<Frame> Frames = new BoltDoubleList<Frame>();
    protected readonly Dictionary<string, List<StateCallback>> Callbacks = new Dictionary<string, List<StateCallback>>();

    protected readonly BitArray FullMask;
    protected readonly BitArray DiffMask;
    protected readonly Priority[] TempPriority;
    protected readonly StateMetaData MetaData;

    public TypeId TypeId {
      get { return MetaData.TypeId; }
    }

    protected State(StateMetaData meta) {
      meta.PacketMaxProperties = Math.Max(Math.Min(meta.PacketMaxProperties, 255), 1);

      MetaData = meta;
      DiffFrame = AllocFrame(-1);
      NullFrame = AllocFrame(-1);
      SendFrame = AllocFrame(-1);

      FullMask = BitArray.CreateSet(meta.PropertyCount);
      DiffMask = BitArray.CreateClear(meta.PropertyCount);
      TempPriority = new Priority[meta.PropertyCount];

      PropertyIdBits = BoltMath.BitsRequired(meta.PropertyCount - 1);
      PropertyObjects = new object[meta.ObjectCount];
      PacketMaxPropertiesBits = BoltMath.BitsRequired(meta.PacketMaxProperties);
    }

    public void AddCallback(string path, StateCallback callback) {
      List<StateCallback> callbacksList;

      if (Callbacks.TryGetValue(path, out callbacksList) == false) {
        Callbacks[path] = callbacksList = new List<StateCallback>(32);
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

      // store indexes
      for (int i = 0; i < p.PropertyPriority.Length; ++i) {
        p.PropertyPriority[i].PropertyIndex = i;
      }
    }

    public void OnPrepareSend() {

    }

    public void OnRender() {

    }

    public void OnInitialized() {
      if (Entity.IsOwner) {
        Frames.AddLast(AllocFrame(BoltCore.frame));
      }
    }

    public void OnCreated(EntityObject entity) {
      Entity = entity;
    }

    public void OnSimulateBefore() {
      if (Entity.IsOwner) {
        return;
      }

      //BoltLog.Info("frame {0} - {1}", Frames.first.Number, Frames.count);

      while ((Entity.Frame > Frames.first.Number) && (Frames.count > 1)) {
        FreeFrame(Frames.RemoveFirst());
      }
    }

    public void OnSimulateAfter() {
      //if (Entity.IsOwner) {
      //Stopwatch sw = Stopwatch.StartNew();

      // calculate diff mask
      BitArray diff = Diff(Frames.first, DiffFrame);

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

      //sw.Stop();
      //BoltLog.Info("Elapsed {0}", sw.Elapsed);
      //}
      //else {
      //  // if we have any properties to call events for
      //  if (Frames.first.ReadProperties.Count > 0) {
      //    for (int i = 0; i < Frames.first.ReadProperties.Count; ++i) {
      //      //GetPropertySerializersArray()[Frames.first.ReadProperties[i]].Changed(this);
      //    }

      //    Frames.first.ReadProperties.Clear();
      //  }
      //}
    }

    void InvokeCallbacks(PropertySerializer p) {
      for (int i = 0; i < p.MetaData.CallbackPaths.Length; ++i) {
        List<StateCallback> callbacksList;

        if (Callbacks.TryGetValue(p.MetaData.CallbackPaths[i], out callbacksList)) {
          for (int n = 0; n < callbacksList.Count; ++n) {
            callbacksList[n](this, p.MetaData.PropertyPath, p.MetaData.CallbackIndices);
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
        Assert.True(proxyPriority[i].PropertyIndex == i, "{0} == {1}", proxyPriority[i].PropertyIndex, i);

        // if this property is set both in our filter and the proxy mask we can consider it for sending
        if (BitArray.SetInBoth(filter, env.Proxy.Mask, i)) {

          // increment priority for this property
          proxyPriority[i].PriorityValue += MetaData.PropertySerializers[i].MetaData.Priority;

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

        int b = PropertyIdBits + s.CalculateBits(Frames.first.Data);
        int ptr = stream.Ptr;

        if (bits >= b) {
          // write property id
          stream.WriteInt(p.PropertyIndex, PropertyIdBits);

          // write data into stream
          s.Pack(Frames.first, connection, stream);

          // use up bits
          bits -= b;

          // add to written list
          env.Written.Add(p);
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
        frame = Frames.last.Duplicate(frameNumber);
      }

      while (--count >= 0) {
        int property = stream.ReadInt(PropertyIdBits);
        var serializer = MetaData.PropertySerializers[property];

        // read data into frame
        serializer.Read(frame, connection, stream);

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

        if (Blit.Diff(a.Data, b.Data, s.MetaData.ByteOffset, s.MetaData.ByteLength)) {
          DiffMask.Set(i);
        }
      }

      return DiffMask;
    }

    Frame AllocFrame(int number) {
      Frame f;

      f = new Frame(number, MetaData.FrameSize);
      f.Objects = PropertyObjects;

      return f;
    }

    void FreeFrame(Frame frame) {

    }

    BitArray CalculateFilter(Filter filter) {
      if ((filter.Bits & 1) == 1) {
        return FullMask;
      }

      BitArray permutation;

      if (MetaData.PropertyFilterCache.TryGetValue(filter, out permutation) == false) {
        permutation = BitArray.CreateClear(MetaData.PropertyCount);

        for (int i = 0; i < 32; ++i) {
          long b = 1 << i;

          if ((filter.Bits & b) == b) {
            permutation.OrAssign(MetaData.PropertyFilters[i]);
          }
        }

        MetaData.PropertyFilterCache.Add(filter, permutation);
      }

      return permutation;
    }
  }
}

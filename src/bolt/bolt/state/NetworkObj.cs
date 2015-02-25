using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Bolt {
  public abstract class NetworkObj_Root : NetworkObj {
    internal NetworkObj_Root(NetworkObj_Meta meta)
      : base(meta) {
      InitRoot();
    }
  }

  public abstract class NetworkObj : IDisposable {
    internal String Path;
    internal NetworkObj Root;
    internal List<NetworkObj> RootObjects;
    internal readonly NetworkObj_Meta Meta;

    internal int OffsetObjects;
    internal int OffsetStorage;
    internal int OffsetProperties;

    internal NetworkState RootState {
      get { return (NetworkState)Root; }
    }

    internal void Add() {
      Assert.True(OffsetObjects == Objects.Count);
      Objects.Add(this);
    }

    internal List<NetworkObj> Objects {
      get { return Root.RootObjects; }
    }

    internal virtual NetworkStorage Storage {
      get { return Root.Storage; }
    }

    internal NetworkObj(NetworkObj_Meta meta) {
      Meta = meta;
    }

    internal void InitRoot() {
      RootObjects = new List<NetworkObj>(Meta.CountObjects);

      Path = null;
      Meta.InitObject(this, this, new NetworkObj_Meta.Offsets());

      Assert.True(RootObjects.Count == Meta.CountObjects, "RootObjects.Count == Meta.CountObjects");
    }

    internal void Init(string path, NetworkObj parent, NetworkObj_Meta.Offsets offsets) {
      Path = path;
      Meta.InitObject(this, parent, offsets);
    }


    internal NetworkStorage AllocateStorage() {
      return Meta.AllocateStorage();
    }

    internal NetworkStorage DuplicateStorage(NetworkStorage s) {
      NetworkStorage c;

      c = AllocateStorage();
      c.Root = s.Root;
      c.Frame = s.Frame;

      Array.Copy(s.Values, 0, c.Values, 0, s.Values.Length);

      return c;
    }

    internal void FreeStorage(NetworkStorage storage) {
      Meta.FreeStorage(storage);
    }

    internal int this[NetworkProperty property] {
      get {
#if DEBUG
        Assert.NotNull(property);

        Assert.True(OffsetObjects >= 0);
        Assert.True(OffsetObjects < Root.Meta.CountObjects);

        Assert.Same(Root.Objects[OffsetObjects], this);
        Assert.Same(Root.Objects[OffsetObjects].Meta, property.PropertyMeta);
        Assert.Same(Root.Meta.Properties[Root.Objects[OffsetObjects].OffsetProperties + property.OffsetProperties].Property, property);
#endif

        return this.OffsetStorage + property.OffsetStorage;
      }
    }

    void IDisposable.Dispose() {
    }
  }
}

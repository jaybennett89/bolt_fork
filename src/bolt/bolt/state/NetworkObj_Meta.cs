using System.Linq;
using System.Collections.Generic;

namespace Bolt {
  internal abstract class NetworkObj_Meta {
    internal struct Offsets {
      internal int OffsetStorage;
      internal int OffsetObjects;
      internal int OffsetProperties;

      internal Offsets(int properties, int storage, int objects) {
        OffsetStorage = storage;
        OffsetObjects = objects;
        OffsetProperties = properties;
      }
    }

    internal TypeId TypeId;
    internal BitSet[] Filters;
    internal Priority[] PropertiesTempPriority;
    internal NetworkPropertyInfo[] Properties;
    internal List<NetworkPropertyInfo> OnRenderCallback = new List<NetworkPropertyInfo>();
    internal List<NetworkPropertyInfo> OnSimulateAfterCallback = new List<NetworkPropertyInfo>();
    internal List<NetworkPropertyInfo> OnSimulateBeforeCallback = new List<NetworkPropertyInfo>();

    internal int CountObjects;
    internal int CountStorage;
    internal int CountProperties;

    internal NetworkObj_Meta() {
      Filters = new BitSet[32];
    }

    void AddPropertyToArray(int offsetProperties, int offsetObjects, NetworkProperty property) {
      Assert.Null(Properties[offsetProperties].Property);

      if (offsetProperties > 0) {
        Assert.NotNull(Properties[offsetProperties - 1].Property);
      }

      Properties[offsetProperties].Property = property;
      Properties[offsetProperties].OffsetObjects = offsetObjects;

      for (int i = 0; i < 32; ++i) {
        int f = 1 << i;

        // this can't be set
        Assert.False(Filters[i].IsSet(offsetProperties));

        // if property is included in this filter, flag it
        if ((property.PropertyFilters & f) == f) {
          Filters[i].Set(offsetProperties);

          // now it must be set
          Assert.True(Filters[i].IsSet(offsetProperties));
        }
      }
    }

    internal void AddProperty(int offsetProperties, int offsetObjects, NetworkProperty property) {
      AddPropertyToArray(offsetProperties, offsetObjects, property);
      Properties[offsetProperties].Path = new NetworkPropertyPath { Next = null, Name = property.PropertyName };
    }

    void AddCopiedProperty(int offsetProperties, int offsetObjects, NetworkProperty property, NetworkPropertyPath path, string prefix) {
      AddPropertyToArray(offsetProperties, offsetObjects, property);
      Properties[offsetProperties].Path = new NetworkPropertyPath { Next = path, Name = prefix };
    }

    internal void CopyProperties(int offsetProperties, int offsetObjects, NetworkObj_Meta meta, string prefix) {
      for (int i = 0; i < meta.Properties.Length; ++i) {
        AddCopiedProperty(offsetProperties + i, offsetObjects + meta.Properties[i].OffsetObjects, meta.Properties[i].Property, meta.Properties[i].Path, prefix);
      }
    }

    internal abstract void InitObject(NetworkObj obj, Offsets offsets);

    internal virtual void InitMeta() {
      for (int i = 0; i < Properties.Length; ++i) {
        if (Properties[i].Property.WantOnRenderCallback) {
          OnRenderCallback.Add(Properties[i]);
        }

        if (Properties[i].Property.WantOnSimulateAfterCallback) {
          OnSimulateAfterCallback.Add(Properties[i]);
        }

        if (Properties[i].Property.WantOnSimulateBeforeCallback) {
          OnSimulateBeforeCallback.Add(Properties[i]);
        }

        BoltLog.Info("Path for {0}: {1}",
          Properties[i].Property.PropertyName,
          string.Join(".", Properties[i].Path.ToArray())
        );
      }

    }

    internal void InitObject(NetworkObj obj, NetworkObj parent, Offsets offsets) {
      obj.Parent = parent;

      obj.OffsetStorage = offsets.OffsetStorage;
      obj.OffsetObjects = offsets.OffsetObjects;
      obj.OffsetProperties = offsets.OffsetProperties;

      obj.Add();

      InitObject(obj, offsets);
    }
  }
}

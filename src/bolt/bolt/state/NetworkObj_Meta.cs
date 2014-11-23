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

    internal int CountObjects;
    internal int CountStorage;
    internal int CountProperties;

    internal NetworkObj_Meta() {
      Filters = new BitSet[32];
    }

    internal void AddProperty(int offsetProperties, int offsetObjects, NetworkProperty property) {
      Assert.Null(Properties[offsetProperties].Property);

      if (offsetProperties > 0) {
        Assert.NotNull(Properties[offsetProperties - 1].Property);
      }

      Properties[offsetProperties].Property = property;
      Properties[offsetProperties].OffsetObjects = offsetObjects;

      for (int i = 0; i < 32; ++i) {
        int f = 1 << i;

        // this can't be set already or something is wrong
        Assert.False(Filters[i].IsSet(offsetProperties));

        // if property is included in this filter, flag it
        if ((property.PropertyFilters & f) == f) {
          Filters[i].Set(offsetProperties);
        }
      }
    }

    internal void CopyProperties(int offsetProperties, int offsetObjects, NetworkObj_Meta meta) {
      for (int i = 0; i < meta.Properties.Length; ++i) {
        AddProperty(offsetProperties + i, offsetObjects + meta.Properties[i].OffsetObjects, meta.Properties[i].Property);
      }
    }

    internal abstract void InitMeta();
    internal abstract void InitObject(NetworkObj obj, Offsets offsets);

    internal void InitObject(NetworkObj obj, NetworkObj root, Offsets offsets) {
      obj.OffsetStorage = offsets.OffsetStorage;
      obj.OffsetObjects = offsets.OffsetObjects;
      obj.OffsetProperties = offsets.OffsetProperties;

      obj.Root = root;
      obj.Add();

      InitObject(obj, offsets);
    }
  }
}

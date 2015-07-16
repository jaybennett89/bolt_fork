using System.Collections.Generic;
using System.Linq;

public class BoltStateAsset : BoltCompilableAsset {
  public BoltAssetProperty tranform = new BoltAssetProperty() { name = "transform", type = BoltAssetPropertyType.Transform };
  public BoltAssetProperty mecanim = new BoltAssetProperty() { name = "mecanim", type = BoltAssetPropertyType.Mecanim };

  public BoltAssetProperty[] _properties = new BoltAssetProperty[0];
  public BoltAssetPropertyGroup[] _groups = new BoltAssetPropertyGroup[0];

  public string className { get { return name; } }
  public string serializerclassName { get { return className + "Serializer"; } }
  public string factoryName { get { return className + "Factory"; } }
  public string frameclassName { get { return className + "ServerFrame"; } }
  public string interfaceName { get { return "I" + className; } }

  public IEnumerable<BoltAssetProperty> defaultProperties {
    get {
      if (tranform.enabled) {
        yield return tranform;
      }

      if (mecanim.enabled && mecanim.mecanimSettings.mecanimAsset) {
        yield return mecanim;
      }
    }
  }

  public IEnumerable<BoltAssetProperty> smoothedProperties {
    get {
      foreach (BoltAssetProperty p in allProperties) {
        if (p.smoothed) {
          yield return p;
        }
      }
    }
  }

  public IEnumerable<BoltAssetProperty> globalProperties {
    get {
      foreach (BoltAssetProperty p in defaultProperties) {
        yield return p;
      }

      foreach (BoltAssetProperty p in _properties) {
        if (p.enabled) {
          yield return p;
        }
      }
    }
  }

  public IEnumerable<BoltAssetProperty> customProperties {
    get {
      foreach (BoltAssetProperty p in _properties) {
        if (p.enabled) {
          yield return p;
        }
      }

      foreach (BoltAssetPropertyGroup g in allGroups) {
        foreach (BoltAssetProperty p in g.allProperties) {
          yield return p;
        }
      }
    }
  }

  public IEnumerable<BoltAssetProperty> allProperties {
    get {
      foreach (BoltAssetProperty p in defaultProperties) {
        yield return p;
      }

      foreach (BoltAssetProperty p in customProperties) {
        yield return p;
      }
    }
  }

  public IEnumerable<BoltAssetProperty> referenceProperties {
    get {
      foreach (BoltAssetProperty p in allProperties) {
        if (p.isReference) {
          yield return p;
        }
      }
    }
  }

  public IEnumerable<BoltAssetProperty> valueProperties {
    get {
      foreach (BoltAssetProperty p in allProperties) {
        if (p.isReference == false) {
          yield return p;
        }
      }
    }
  }

  public IEnumerable<BoltAssetPropertyGroup> allGroups {
    get {
      foreach (BoltAssetPropertyGroup g in _groups) {
        if (g.enabled && g.allProperties.Count() > 0) {
          yield return g;
        }
      }
    }
  }

  public uint Mask (BoltAssetSyncTarget target) {
    uint mask = 0;

    foreach (BoltAssetProperty p in globalProperties) {
      if (p.syncMode == BoltAssetSyncMode.Changed && ((p.syncTarget & target) == target)) {
        mask |= (1u << p.bit);
      }
    }

    foreach (BoltAssetPropertyGroup g in allGroups) {
      if ((g.syncTarget & target) == target) {
        mask |= (1u << g.bit);
      }
    }

    return mask;
  }

  public void AssignBits () {
    int bit = 0;

    foreach (BoltAssetProperty p in globalProperties) {
      if (p.syncMode == BoltAssetSyncMode.Changed) {
        p.bit = bit++;
      } else {
        p.bit = int.MaxValue;
      }
    }

    foreach (BoltAssetPropertyGroup g in allGroups) {
      g.bit = bit++;

      foreach (BoltAssetProperty p in g.allProperties) {
        p.bit = g.bit;
      }
    }
  }
}

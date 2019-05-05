using System;
using System.Collections.Generic;

[Serializable]
public class BoltAssetPropertyGroup {
  [NonSerialized]
  public int bit;

  public bool enabled = true;
  public string name = "new_group";
  public BoltAssetSyncTarget syncTarget = BoltAssetSyncTarget.Controller | BoltAssetSyncTarget.Proxy;
  public BoltAssetProperty[] _properties = new BoltAssetProperty[0];

  public IEnumerable<BoltAssetProperty> allProperties {
    get {
      foreach (BoltAssetProperty p in _properties) {
        if (p.enabled) {
          yield return p;
        }
      }
    }
  }
}

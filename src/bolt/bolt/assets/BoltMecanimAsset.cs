using System.Collections.Generic;
using UnityEngine;

public class BoltMecanimAsset : BoltCompilableAsset {
  public RuntimeAnimatorController controller = null;
  public BoltAssetProperty[] properties = new BoltAssetProperty[0];
  public string interfaceName { get { return "I" + name; } }
  public string frameclassName { get { return name + "Frame"; } }

  public IEnumerable<BoltAssetProperty> allProperties {
    get {
      foreach (BoltAssetProperty p in properties) {
        if (p.enabled) {
          yield return p;
        }
      }
    }
  }

  public IEnumerable<BoltAssetProperty> nonTriggerProperties {
    get {
      foreach (BoltAssetProperty p in allProperties) {
        if (p.type != BoltAssetPropertyType.Trigger) {
          yield return p;
        }
      }
    }
  }

  public IEnumerable<BoltAssetProperty> triggerProperties {
    get {
      foreach (BoltAssetProperty p in allProperties) {
        if (p.type == BoltAssetPropertyType.Trigger) {
          yield return p;
        }
      }
    }
  }
}

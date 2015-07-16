using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BoltCommandAsset : BoltCompilableAsset {
  [HideInInspector]
  public ushort id;
  public BoltAssetProperty[] inputProperties = new BoltAssetProperty[0];
  public BoltAssetProperty[] stateProperties = new BoltAssetProperty[0];

  public string className {
    get { return name; }
  }

  public string factoryName {
    get { return className + "Handler"; }
  }

  public IEnumerable<BoltAssetProperty> allInputProperties {
    get { return inputProperties.Where(x => x.enabled); }
  }

  public IEnumerable<BoltAssetProperty> allStateProperties {
    get { return stateProperties.Where(x => x.enabled); }
  }
}

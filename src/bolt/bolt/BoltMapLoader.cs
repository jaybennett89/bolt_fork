using System;
using UnityEngine;

class BoltMapLoader : MonoBehaviour {
  class LoadOp : BoltObject {
    public Map map;
    public AsyncOperation async;
  }

  static readonly BoltSingleList<LoadOp> _loadOps = new BoltSingleList<LoadOp>();
  static internal bool isLoading {
    get { return _loadOps.count > 0; }
  }

  void Update () {
    if (_loadOps.count > 0) {
      if (BoltCore.isUnityPro) {
        LoadAsync();
      } else {
        Load();
      }
    }
  }

  void Load () {
    // notify core of loading
    BoltCore.LoadMapBeginInternal(_loadOps.first.map);

    // load level
    Application.LoadLevel(_loadOps.first.map.name);

    // we are done!
    Done();
  }

  void LoadAsync () {
    if (_loadOps.first.async == null) {
      // notify core of loading
      BoltCore.LoadMapBeginInternal(_loadOps.first.map);

      // begin new async load
      _loadOps.first.async = Application.LoadLevelAsync(_loadOps.first.map.name);

    } else {
      if (_loadOps.first.async.isDone) {
        Done();
      }
    }
  }

  void Done () {
    try {
      // collect all old garbage
      GC.Collect();

      // invoke to core
      BoltCore.LoadMapDoneInternal(_loadOps.first.map);
    } finally {
      _loadOps.RemoveFirst();
    }
  }

  internal static void Enqueue (Map map) {
    _loadOps.AddLast(new LoadOp { map = map });
  }
}

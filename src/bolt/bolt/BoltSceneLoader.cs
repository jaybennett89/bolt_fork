using System;
using UnityEngine;

class BoltSceneLoader : MonoBehaviour {
  class LoadOp : BoltObject {
    public Scene scene;
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
    BoltCore.LoadMapBeginInternal(_loadOps.first.scene);

    // load level
    Application.LoadLevel(_loadOps.first.scene.name);

    // we are done!
    Done();
  }

  void LoadAsync () {
    if (_loadOps.first.async == null) {
      // notify core of loading
      BoltCore.LoadMapBeginInternal(_loadOps.first.scene);

      // begin new async load
      _loadOps.first.async = Application.LoadLevelAsync(_loadOps.first.scene.name);

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
      BoltCore.LoadMapDoneInternal(_loadOps.first.scene);
    } finally {
      _loadOps.RemoveFirst();
    }
  }

  internal static void Enqueue (Scene scene) {
    _loadOps.AddLast(new LoadOp { scene = scene });
  }
}

using System;
using UnityEngine;

class BoltSceneLoader : MonoBehaviour {
  class LoadOp : BoltObject {
    public Scene scene;
    public AsyncOperation async;
  }
  static Scene _loaded;
  static int _loadedDelay;

  static readonly BoltSingleList<LoadOp> _loadOps = new BoltSingleList<LoadOp>();
  static internal bool isLoading { get { return _loadOps.count > 0; } }

  void Update() {
    if (_loadOps.count > 0) {
      if (BoltCore.isUnityPro) {
        LoadAsync();
      }
      else {
        Load();
      }
    }
    else {
      if (_loadedDelay > 0) {

        if (--_loadedDelay == 0) {
          if (_loadOps.count == 0) {
            BoltCore.LoadMapDoneInternal(_loaded);
          }
        }
      }
    }
  }

  void Load() {

    // notify core of loading
    BoltCore.LoadMapBeginInternal(_loadOps.first.scene);

    // load level
    Application.LoadLevel(_loadOps.first.scene.name);

    // we are done!
    Done();
  }

  void LoadAsync() {
    if (_loadOps.first.async == null) {
      // notify core of loading
      BoltCore.LoadMapBeginInternal(_loadOps.first.scene);

      // begin new async load
      _loadOps.first.async = Application.LoadLevelAsync(_loadOps.first.scene.name);

    }
    else {
      if (_loadOps.first.async.isDone) {
        Done();
      }
    }
  }

  void Done() {
    try {
      GC.Collect();

      _loaded = _loadOps.RemoveFirst().scene;
    }
    finally {
      if (_loadOps.count == 0) {
        _loadedDelay = 10;
      }
    }
  }

  internal static void Enqueue(Scene scene) {
    _loadedDelay = 0;
    _loadOps.AddLast(new LoadOp { scene = scene });
  }
}

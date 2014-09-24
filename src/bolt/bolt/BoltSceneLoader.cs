using System;
using UnityEngine;

class BoltSceneLoader : MonoBehaviour {
  class LoadOp : BoltObject {
    public Scene scene;
    public AsyncOperation async;
  }

  static int _frameDelay;
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
      if (_frameDelay > 0) {
        if (--_frameDelay == 0) {
          try {
            BoltCore.LoadMapDoneInternal(_loadOps.first.scene);
          }
          finally {
            _loadOps.RemoveFirst();
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
    GC.Collect();
    _frameDelay = 10;
  }

  internal static void Enqueue(Scene scene) {
    _frameDelay = 0;
    _loadOps.AddLast(new LoadOp { scene = scene });
  }
}

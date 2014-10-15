using System;
using UnityEngine;

class BoltSceneLoader : MonoBehaviour {
  class LoadOp : BoltObject {
    public Bolt.Scene scene;
    public AsyncOperation async;
  }

  static int _delay;
  static Bolt.Scene _loaded;

  static readonly BoltSingleList<LoadOp> _loadOps = new BoltSingleList<LoadOp>();
  static internal bool IsLoading { get { return _loadOps.count > 0; } }

  void Update() {
    if (_loadOps.count > 0) {
      if (BoltNetworkInternal.UsingUnityPro) {
        LoadAsync();
      }
      else {
        Load();
      }
    }
    else {
      if (_delay > 0) {
        if (--_delay == 0) {
          if (_loadOps.count == 0) {
            BoltCore.SceneLoadDone(_loaded);
          }
        }
      }
    }
  }

  void Load() {
    // notify core of loading
    BoltCore.SceneLoadBegin(_loadOps.first.scene);

    // load level
    Application.LoadLevel(_loadOps.first.scene.Index);

    // we are done!
    Done();
  }

  void LoadAsync() {
    if (_loadOps.first.async == null) {
      // notify core of loading
      BoltCore.SceneLoadBegin(_loadOps.first.scene);

      // begin new async load
      _loadOps.first.async = Application.LoadLevelAsync(_loadOps.first.scene.Index);

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
        _delay = 60;
      }
    }
  }

  internal static void Enqueue(Bolt.Scene scene) {
    BoltLog.Debug("Loading {0} ({1})", scene, BoltNetworkInternal.GetSceneName(scene.Index));

    _delay = 0;
    _loadOps.AddLast(new LoadOp { scene = scene });
  }
}
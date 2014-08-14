using System;
using UnityEngine;

/// <summary>
/// Performs loading and unloading of maps
/// </summary>
public abstract class BoltMapLoader : MonoBehaviour {
  internal protected class LoadOp : BoltObject {
    public BoltMapLoadOp _op;
    public AsyncOperation _load;
  }

  static internal protected readonly BoltSingleList<LoadOp> _loadOps = new BoltSingleList<LoadOp>();

  /// <summary>
  /// If we are loading a map currently or not
  /// </summary>
  public static bool isLoading {
    get { return _loadOps.count > 0; }
  }

  void Awake () {
    DontDestroyOnLoad(gameObject);
  }

  internal protected void LoadDone () {

    try {
      // reset frame, and set loaded map/token
      BoltCore._loadedMap = _loadOps.first._op;

      // if this is our final load op
      if (_loadOps.count == 1) {
        BoltCore.UpdateActiveGlobalBehaviours(_loadOps.first._op.map);

        if (BoltCore.isServer) {
          foreach (BoltConnection cn in BoltCore.clients) {
            // make sure all clients are loading this map
            cn.LoadMapOnClient(_loadOps.first._op);

            // also trigger client done loading callback on all clients which are done
            cn.TriggerClientLoadedMapCallback();

            // send load done event to clients
            cn.Raise<ILoadMapDone>(evt => evt.op = _loadOps.first._op);
          }
        }
        else {
          if (BoltCore.isClient) {
            // send load done event to server
            BoltCore.server.Raise<ILoadMapDone>(evt => evt.op = _loadOps.first._op);
          }
        }

        // call out to user code
        BoltCallbacksBase.MapLoadDoneInvoke(_loadOps.first._op.map);
      }

      GC.Collect();

    } finally {
      _loadOps.RemoveFirst();
    }
  }

  internal static void LoadMap (BoltMapLoadOp op) {
    _loadOps.AddLast(new LoadOp { _op = op });
  }
}

public class BoltMapLoaderFree : BoltMapLoader {
  void Update () {
    if (_loadOps.count == 0) {
      return;
    }

    // load new map scene
    BoltCallbacksBase.MapLoadBeginInvoke(_loadOps.first._op.map);
    Application.LoadLevel(_loadOps.first._op.map);

    // done!
    LoadDone();
  }
}

public class BoltMapLoaderPro : BoltMapLoader {
  void Update () {
    if (_loadOps.count == 0)
      return;

    if (_loadOps.first._load == null) {
      UnloadDone();

      _loadOps.first._load = Application.LoadLevelAsync(_loadOps.first._op.map);
      BoltCallbacksBase.MapLoadBeginInvoke(_loadOps.first._op.map);
      Progress();
    }
    else if (_loadOps.first._load.isDone == false) {
      Progress();

    }
    else {
      LoadDone();
    }
  }

  void Progress () {
    foreach (BoltConnection cn in BoltCore.clients) {
      cn.LoadMapOnClient(_loadOps.first._op);
    }
  }

  void UnloadDone () {
    GC.Collect();
  }
}
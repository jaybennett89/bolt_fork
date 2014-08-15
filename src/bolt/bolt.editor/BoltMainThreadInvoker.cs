using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Threading;

[InitializeOnLoad]
static class BoltMainThreadInvoker {
  static DateTime lastPlatformCheck = DateTime.MinValue;
  static readonly Queue<Action> _queue = new Queue<Action>();

  static BoltMainThreadInvoker () {
    EditorApplication.update += Update;
  }

  static void Update () {
    lock (_queue) {
      while (_queue.Count > 0) {
        _queue.Dequeue()();
      }
    }

    if (lastPlatformCheck.AddSeconds(5) < DateTime.Now) {
      switch (EditorUserBuildSettings.activeBuildTarget) {
        case BuildTarget.Android:
          BoltEditorUtils.InstallAsset("Assets/Plugins/Android/libudpkit_android.so", () => BoltEditorUtils.GetResourceBytes("bolt.editor.Resources.libudpkit_android.so"));
          break;

        case BuildTarget.iPhone:
          BoltEditorUtils.InstallAsset("Assets/Plugins/iOS/libudpkit_ios.a", () => BoltEditorUtils.GetResourceBytes("bolt.editor.Resources.libudpkit_ios.a"));
          break;
      }

      lastPlatformCheck = DateTime.Now;
    }
  }

  public static void Invoke (Action action) {
    lock (_queue) {
      _queue.Enqueue(action);
    }
  }

}

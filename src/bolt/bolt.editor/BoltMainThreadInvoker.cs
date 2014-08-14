using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Threading;

[InitializeOnLoad]
static class BoltMainThreadInvoker {
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
  }
  
  public static void Invoke (Action action) {
    lock (_queue) {
      _queue.Enqueue(action);
    }
  }

}

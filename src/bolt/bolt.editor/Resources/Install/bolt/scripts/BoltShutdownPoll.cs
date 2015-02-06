using System;
using System.Threading;
using UnityEngine;

public class BoltShutdownPoll : MonoBehaviour {
  public Action Callback;
  public ManualResetEvent ShutdownEvent;

  void Update() {
    if (ShutdownEvent == null) {
      GameObject.Destroy(gameObject);
    }
    else {
      if (ShutdownEvent.WaitOne(0)) {
        try {
          if (Callback != null) {
            Callback();
          }
        }
        finally {
          Callback = null;
          ShutdownEvent = null;
        }
      }
    }
  }
}

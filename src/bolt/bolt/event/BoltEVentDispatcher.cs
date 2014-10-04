using System;
using System.Collections.Generic;
using UnityEngine;

class BoltEventDispatcher {
  internal List<MonoBehaviour> _targets = new List<MonoBehaviour>();

  public void Dispatch (BoltEventBase evnt, IBoltEventFactory handler) {
    for (int i = 0; i < _targets.Count; ++i) {
      MonoBehaviour mb = _targets[i];

      if (mb) {
        // dont call on disabled behaviours
        if (mb.enabled == false) {
          continue;
        }

        // dont call on behaviours attached to inactive game objects
        if (mb.gameObject.activeInHierarchy == false) {
          continue;
        }

        // invoke event
        try {
          handler.Dispatch(evnt, mb);
        } catch (Exception exn) {
          BoltLog.Error("exception thrown while dispatching {0}", evnt);
          BoltLog.Exception(exn);
        }

      } else {
        // remove event if this behaviour is destroyed
        _targets.RemoveAt(i);

        // 
        --i;

        continue;
      }
    }
  }

  public void Add (MonoBehaviour behaviour) {
#if DEBUG
    if (_targets.Contains(behaviour)) {
      BoltLog.Warn("behaviour already registered for this dispatcher");
      return;
    }
#endif

    _targets.Add(behaviour);
  }

  public void Remove (MonoBehaviour behaviour) {
    _targets.Remove(behaviour);
  }
}
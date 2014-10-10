using System;
using System.Collections.Generic;
using UE = UnityEngine;

namespace Bolt {
  partial class EventDispatcher {
    List<UE.MonoBehaviour> _targets = new List<UE.MonoBehaviour>();

    void Raise(Event ev) {
      IEventFactory factory = Factory.GetEventFactory(ev.Meta.TypeId);

      for (int i = 0; i < _targets.Count; ++i) {
        UE.MonoBehaviour mb = _targets[i];

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
            factory.Dispatch(ev, mb);
          }
          catch (Exception exn) {
            BoltLog.Error("User code throw exception when invoking {0}", ev);
            BoltLog.Exception(exn);
          }

        }
        else {
          // remove callback if this behaviour is destroyed
          _targets.RemoveAt(i);

          // 
          --i;

          continue;
        }
      }
    }

    public void Add(UE.MonoBehaviour behaviour) {
#if DEBUG
      if (_targets.Contains(behaviour)) {
        BoltLog.Warn("Behaviour is already registered in this dispatcher, ignoring call to Add.");
        return;
      }
#endif

      _targets.Add(behaviour);
    }

    public void Remove(UE.MonoBehaviour behaviour) {
      var success = _targets.Remove(behaviour);
      if (success == false) {
        BoltLog.Warn("Behaviour not available in this dispatcher, ignoring call to Remove.");
      }
    }

    public void Clear() {
      for (int i = 0; i < _targets.Count; ++i) {
        var mb = _targets[i] as BoltInternal.GlobalEventListenerBase;
        if (mb != null) {
          if (mb.PersistBetweenStartupAndShutdown()) {
            continue;
          }
        }

        // remove at this indexx
        _targets.RemoveAt(i);

        // reset index
        --i;
      }
    }
  }
}
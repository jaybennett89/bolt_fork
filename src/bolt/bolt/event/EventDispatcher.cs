using System;
using System.Collections.Generic;
using UE = UnityEngine;

namespace Bolt {
  partial class EventDispatcher {
    struct EventListener {
      public IEventListener Listener;
      public UE.GameObject GameObject;
      public UE.MonoBehaviour Behaviour;
    }

    List<EventListener> _targets = new List<EventListener>();

    void Raise(Event ev) {
      IEventFactory factory = Factory.GetEventFactory(ev.Meta.TypeId);

      for (int i = 0; i < _targets.Count; ++i) {
        EventListener mb = _targets[i];

        if (mb.Behaviour) {
          // dont call on disabled behaviours
          if (mb.Behaviour.enabled == false) {
            if ((mb.Listener == null) || (mb.Listener.InvokeIfDisabled == false)) {
              continue;
            }
          }

          // dont call on behaviours attached to inactive game objects
          if (mb.GameObject.activeInHierarchy == false) {
            if ((mb.Listener == null) || (mb.Listener.InvokeIfGameObjectIsInactive == false)) {
              continue;
            }
          }

          // invoke event
          try {
            factory.Dispatch(ev, mb.Behaviour);
          }
          catch (Exception exn) {
            BoltLog.Error("User code threw exception when invoking {0}", ev);
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

      for (int i = 0; i < _targets.Count; ++i) {
        if (ReferenceEquals(_targets[i].Behaviour, behaviour)) {
          BoltLog.Warn("Behaviour is already registered in this dispatcher, ignoring call to Add.");
          return;
        }
      }

      _targets.Add(new EventListener { Behaviour = behaviour, GameObject = behaviour.gameObject, Listener = behaviour as IEventListener });
    }

    public void Remove(UE.MonoBehaviour behaviour) {

      for (int i = 0; i < _targets.Count; ++i) {
        if (ReferenceEquals(_targets[i].Behaviour, behaviour)) {
          _targets.RemoveAt(i);
          return;
        }
      }

      BoltLog.Warn("Behaviour not available in this dispatcher, ignoring call to Remove.");
    }

    public void Clear() {
      for (int i = 0; i < _targets.Count; ++i) {
        var mb = _targets[i].Behaviour as BoltInternal.GlobalEventListenerBase;
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
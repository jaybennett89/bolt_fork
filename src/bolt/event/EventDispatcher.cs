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

    struct CallbackWrapper {
      public Object Original;
      public Action<Event> Wrapper;
    }

    List<EventListener> _targets = new List<EventListener>();
    Dictionary<Type, List<CallbackWrapper>> _callbacks = new Dictionary<Type, List<CallbackWrapper>>();

    void Raise(Event ev) {
      IEventFactory factory = Factory.GetEventFactory(ev.Meta.TypeId);

      List<CallbackWrapper> callbacks;

      if (_callbacks.TryGetValue(ev.GetType(), out callbacks)) {
        for (int i = 0; i < callbacks.Count; ++i) {
          callbacks[i].Wrapper(ev);
        }
      }

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

    public void Add<T>(Action<T> callback) where T : Event {
      List<CallbackWrapper> callbacks;

      if (_callbacks.TryGetValue(typeof(T), out callbacks) == false) {
        _callbacks.Add(typeof(T), callbacks = new List<CallbackWrapper>());
      }

      CallbackWrapper wrapper;
      wrapper.Original = callback;
      wrapper.Wrapper = ev => callback((T)ev);

      callbacks.Add(wrapper);
    }

    public void Remove<T>(Action<T> callback) where T : Event {
      List<CallbackWrapper> callbacks;

      if (_callbacks.TryGetValue(typeof(T), out callbacks) == false) {
        for (int i = 0; i < callbacks.Count; ++i) {
          var org = (Action<T>)callbacks[i].Original;
          if (org == callback) {
            callbacks.RemoveAt(i);
            return;
          }
        }
      }

      BoltLog.Warn("Could not find delegate registered as callback");
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
      _callbacks = new Dictionary<Type, List<CallbackWrapper>>();

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
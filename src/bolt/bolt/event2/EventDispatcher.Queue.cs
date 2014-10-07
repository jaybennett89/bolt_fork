using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  partial class EventDispatcher {
    static Queue<Event> _dispatchQueue = new Queue<Event>();

    public static void Enqueue(Event ev) {
      _dispatchQueue.Enqueue(ev);
    }

    public static void DispatchAllEvents() {
      while (_dispatchQueue.Count > 0) {
        Dispatch(_dispatchQueue.Dequeue());
      }
    }

    static void Dispatch(Event ev) {
      var it = BoltCore._connections.GetIterator();

      switch (ev.Targets) {
        case Event.ENTITY_EVERYONE:
          if (ev.TargetEntity != null) {
            while (it.Next()) {
              it.val._eventChannel.Queue(ev);
            }

            RaiseLocal(ev);
          }
          break;

        case Event.ENTITY_EVERYONE_EXCEPT_CONTROLLER:
          if (ev.TargetEntity != null) {
            while (it.Next()) {
              if (ev.TargetEntity.IsController(it.val) == false) {
                it.val._eventChannel.Queue(ev);
              }
            }

            if (ev.TargetEntity.HasControl == false) {
              RaiseLocal(ev);
            }
          }
          break;

        case Event.GLOBAL_EVERYONE:
          while (it.Next()) {
            it.val._eventChannel.Queue(ev);
          }

          RaiseLocal(ev);
          break;

        case Event.GLOBAL_OTHERS:
          while (it.Next()) {
            it.val._eventChannel.Queue(ev);
          }

          if (ev.IsFromLocalComputer == false) {
            RaiseLocal(ev);
          }
          break;

        case Event.GLOBAL_ALL_CLIENTS:
          while (it.Next()) {
            it.val._eventChannel.Queue(ev);
          }

          if (BoltCore.isClient) {
            RaiseLocal(ev);
          }
          break;

        case Event.GLOBAL_SERVER:
          if (BoltCore.isServer) {
            RaiseLocal(ev);
          }
          else {
            BoltCore.server._eventChannel.Queue(ev);
          }
          break;

        case Event.GLOBAL_SPECIFIC_CONNECTION:
          if (ev.IsFromLocalComputer) {
            ev.TargetConnection._eventChannel.Queue(ev);
          }
          else {
            RaiseLocal(ev);
          }
          break;
      }
    }

    static void RaiseLocal(Event ev) {
      try {
        BoltLog.Info("Raising {0}", ev);

        if (ev.IsEntityEvent) {
          ev.TargetEntity.EventDispatcher.Raise(ev);
        }
        else {
          BoltCore._globalEventDispatcher.Raise(ev);
        }
      }
      finally {
        ev.DecrementRefs();
      }
    }
  }
}

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
          Entity_Everyone(ev);
          break;

        case Event.ENTITY_EVERYONE_EXCEPT_CONTROLLER:
          Entity_Everyone_Except_Controller(ev);
          break;

        case Event.GLOBAL_EVERYONE:
          Global_Everyone(ev);
          break;

        case Event.GLOBAL_OTHERS:
          Global_Others(ev);
          break;

        case Event.GLOBAL_ALL_CLIENTS:
          Global_All_Clients(ev);
          break;

        case Event.GLOBAL_SERVER:
          Global_Server(ev);
          break;

        case Event.GLOBAL_SPECIFIC_CONNECTION:
          Global_Specific_Connection(ev);
          break;
      }
    }

    static void Global_Specific_Connection(Event ev) {
      if (ev.IsFromLocalComputer) {
        ev.TargetConnection._eventChannel.Queue(ev);
      }
      else {
        RaiseLocal(ev);
      }
    }

    static void Global_Server(Event ev) {
      if (BoltCore.isServer) {
        RaiseLocal(ev);
      }
      else {
        BoltCore.server._eventChannel.Queue(ev);
      }
    }

    static void Global_All_Clients(Event ev) {
      var it = BoltCore._connections.GetIterator();

      while (it.Next()) {
        if (ReferenceEquals(it.val, ev.SourceConnection)) {
          continue;
        }

        it.val._eventChannel.Queue(ev);
      }

      if (BoltCore.isClient) {
        RaiseLocal(ev);
      }
    }

    static void Global_Others(Event ev) {
      var it = BoltCore._connections.GetIterator();

      while (it.Next()) {
        if (ReferenceEquals(it.val, ev.SourceConnection)) {
          continue;
        }

        it.val._eventChannel.Queue(ev);
      }

      if (ev.IsFromLocalComputer == false) {
        RaiseLocal(ev);
      }
    }

    static void Global_Everyone(Event ev) {
      var it = BoltCore._connections.GetIterator();

      while (it.Next()) {
        if (ReferenceEquals(it.val, ev.SourceConnection)) {
          continue;
        }

        it.val._eventChannel.Queue(ev);
      }

      RaiseLocal(ev);
    }

    static void Entity_Everyone_Except_Controller(Event ev) {
      var it = BoltCore._connections.GetIterator();

      if (ev.TargetEntity != null) {
        while (it.Next()) {
          if (ev.TargetEntity.IsController(it.val)) {
            continue;
          }

          if (ReferenceEquals(it.val, ev.SourceConnection)) {
            continue;
          }

          it.val._eventChannel.Queue(ev);
        }

        if (ev.TargetEntity.HasControl == false) {
          RaiseLocal(ev);
        }
      }
    }

    static void Entity_Everyone(Event ev) {
      var it = BoltCore._connections.GetIterator();

      if (ev.TargetEntity != null) {
        while (it.Next()) {
          if (ReferenceEquals(it.val, ev.SourceConnection)) {
            continue;
          }

          it.val._eventChannel.Queue(ev);
        }

        RaiseLocal(ev);
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

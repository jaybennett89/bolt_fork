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

    public static void Received(Event ev) {
      if (BoltCore.EventFilter.EventReceived(ev)) {
        _dispatchQueue.Enqueue(ev);
      }
    }

    public static void DispatchAllEvents() {
      while (_dispatchQueue.Count > 0) {
        Dispatch(_dispatchQueue.Dequeue());
      }
    }

    static void Dispatch(Event ev) {
      switch (ev.Targets) {
        case Event.ENTITY_EVERYONE:
          Entity_Everyone(ev);
          break;

        case Event.ENTITY_EVERYONE_EXCEPT_CONTROLLER:
          Entity_Everyone_Except_Controller(ev);
          break;

        case Event.ENTITY_EVERYONE_EXCEPT_OWNER:
          Entity_Everyone_Except_Owner(ev);
          break;

        case Event.ENTITY_EVERYONE_EXCEPT_OWNER_AND_CONTROLLER:
          Entity_Everyone_Except_Owner_And_Controller(ev);
          break;

        case Event.ENTITY_ONLY_CONTROLLER:
          Entity_Only_Controller(ev);
          break;

        case Event.ENTITY_ONLY_OWNER:
          Entity_Only_Owner(ev);
          break;

        case Event.ENTITY_ONLY_SELF:
          Entity_Only_Self(ev);
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

        case Event.GLOBAL_ONLY_SERVER:
          Global_Server(ev);
          break;

        case Event.GLOBAL_SPECIFIC_CONNECTION:
          Global_Specific_Connection(ev);
          break;

        case Event.GLOBAL_ONLY_SELF:
          Global_Only_Self(ev);
          break;
      }
    }

    static void Global_Only_Self(Event ev) {
      RaiseLocal(ev);

      // we can free this (never used after this)
      ev.FreeStorage();
    }

    static void Entity_Only_Self(Event ev) {
      if (ev.TargetEntity) {
        RaiseLocal(ev);

        // we can free this (never used after this)
        ev.FreeStorage();
      }
    }

    static void Entity_Only_Owner(Event ev) {
      if (ev.TargetEntity) {
        if (ev.TargetEntity.IsOwner) {
          RaiseLocal(ev);

          // we can free this (never used after this)
          ev.FreeStorage();
        }
        else {
          // forward to owner
          ev.TargetEntity.Source._eventChannel.Queue(ev);
        }
      }
    }

    static void Entity_Only_Controller(Event ev) {
      if (ev.TargetEntity) {
        if (ev.TargetEntity.HasControl) {
          RaiseLocal(ev);

          // we can free this (never used after this)
          ev.FreeStorage();
        }
        else {
          if (ev.TargetEntity.IsOwner) {
            if (ev.TargetEntity.Controller != null) {
              ev.TargetEntity.Controller._eventChannel.Queue(ev);
            }
            else {
              BoltLog.Warn("NetworkEvent sent to controller but no controller exists, event will NOT be raised");
            }
          }
          else {
            ev.TargetEntity.Source._eventChannel.Queue(ev);
          }
        }
      }
      else {
        BoltLog.Warn("NetworkEvent with NULL target, event will NOT be forwarded or raised");
      }
    }

    static void Entity_Everyone_Except_Owner_And_Controller(Event ev) {
      if (ev.TargetEntity != null) {
        var it = BoltCore._connections.GetIterator();

        while (it.Next()) {
          if (ReferenceEquals(it.val, ev.SourceConnection)) {
            continue;
          }

          it.val._eventChannel.Queue(ev);
        }

        if (ev.TargetEntity.IsOwner == false && ev.TargetEntity.HasControl == false) {
          RaiseLocal(ev);
        }
      }
    }

    static void Entity_Everyone_Except_Owner(Event ev) {
      if (ev.TargetEntity != null) {
        var it = BoltCore._connections.GetIterator();

        while (it.Next()) {
          if (ReferenceEquals(it.val, ev.SourceConnection)) {
            continue;
          }

          it.val._eventChannel.Queue(ev);
        }

        if (ev.TargetEntity.IsOwner == false) {
          RaiseLocal(ev);
        }
      }
    }

    static void Entity_Everyone_Except_Controller(Event ev) {
      if (ev.TargetEntity != null) {
        var it = BoltCore._connections.GetIterator();

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

    static void Global_Specific_Connection(Event ev) {
      if (ev.FromSelf) {
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

      if (ev.FromSelf == false) {
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

    static void RaiseLocal(Event ev) {
      try {
        BoltLog.Debug("Raising {0}", ev);

        if (ev.IsEntityEvent) {
          ev.TargetEntity.EventDispatcher.Raise(ev);
        }
        else {
          BoltCore._globalEventDispatcher.Raise(ev);
        }

        if (BoltCore.isClient && ev.FromSelf == false) {
          ev.FreeStorage();
        }
      }
      finally {
        //ev.DecrementRefs();
      }
    }
  }
}

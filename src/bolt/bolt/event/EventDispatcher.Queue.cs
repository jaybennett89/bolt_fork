using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  partial class EventDispatcher {
    static Queue<NetworkEvent> _dispatchQueue = new Queue<NetworkEvent>();

    public static void Enqueue(NetworkEvent ev) {
      _dispatchQueue.Enqueue(ev);
    }

    public static void Received(NetworkEvent ev) {
      if (BoltCore.EventFilter.EventReceived(ev)) {
        _dispatchQueue.Enqueue(ev);
      }
    }

    public static void DispatchAllEvents() {
      while (_dispatchQueue.Count > 0) {
        Dispatch(_dispatchQueue.Dequeue());
      }
    }

    static void Dispatch(NetworkEvent ev) {
      switch (ev.Targets) {
        case NetworkEvent.ENTITY_EVERYONE:
          Entity_Everyone(ev);
          break;

        case NetworkEvent.ENTITY_EVERYONE_EXCEPT_CONTROLLER:
          Entity_Everyone_Except_Controller(ev);
          break;

        case NetworkEvent.ENTITY_EVERYONE_EXCEPT_OWNER:
          Entity_Everyone_Except_Owner(ev);
          break;

        case NetworkEvent.ENTITY_ONLY_CONTROLLER:
          Entity_Only_Controller(ev);
          break;

        case NetworkEvent.ENTITY_ONLY_OWNER:
          Entity_Only_Owner(ev);
          break;

        case NetworkEvent.ENTITY_ONLY_SELF:
          Entity_Only_Self(ev);
          break;

        case NetworkEvent.GLOBAL_EVERYONE:
          Global_Everyone(ev);
          break;

        case NetworkEvent.GLOBAL_OTHERS:
          Global_Others(ev);
          break;

        case NetworkEvent.GLOBAL_ALL_CLIENTS:
          Global_All_Clients(ev);
          break;

        case NetworkEvent.GLOBAL_ONLY_SERVER:
          Global_Server(ev);
          break;

        case NetworkEvent.GLOBAL_SPECIFIC_CONNECTION:
          Global_Specific_Connection(ev);
          break;

        case NetworkEvent.GLOBAL_ONLY_SELF:
          Global_Only_Self(ev);
          break;
      }
    }

    static void Global_Only_Self(NetworkEvent ev) {
      RaiseLocal(ev);
    }

    static void Entity_Only_Self(NetworkEvent ev) {
      if (ev.TargetEntity) {
        RaiseLocal(ev);
      }
    }

    static void Entity_Only_Owner(NetworkEvent ev) {
      if (ev.TargetEntity) {
        if (ev.TargetEntity.IsOwner) {
          RaiseLocal(ev);
        }
        else {
          // forward to owner
          ev.TargetEntity.Source._eventChannel.Queue(ev);
        }
      }
    }

    static void Entity_Only_Controller(NetworkEvent ev) {
      if (ev.TargetEntity) {
        if (ev.TargetEntity.HasControl) {
          RaiseLocal(ev);
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

    static void Entity_Everyone_Except_Owner(NetworkEvent ev) {
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

    static void Entity_Everyone_Except_Controller(NetworkEvent ev) {
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

    static void Entity_Everyone(NetworkEvent ev) {
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

    static void Global_Specific_Connection(NetworkEvent ev) {
      if (ev.IsFromLocalComputer) {
        ev.TargetConnection._eventChannel.Queue(ev);
      }
      else {
        RaiseLocal(ev);
      }
    }

    static void Global_Server(NetworkEvent ev) {
      if (BoltCore.isServer) {
        RaiseLocal(ev);
      }
      else {
        BoltCore.server._eventChannel.Queue(ev);
      }
    }

    static void Global_All_Clients(NetworkEvent ev) {
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

    static void Global_Others(NetworkEvent ev) {
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

    static void Global_Everyone(NetworkEvent ev) {
      var it = BoltCore._connections.GetIterator();

      while (it.Next()) {
        if (ReferenceEquals(it.val, ev.SourceConnection)) {
          continue;
        }

        it.val._eventChannel.Queue(ev);
      }

      RaiseLocal(ev);
    }


    static void RaiseLocal(NetworkEvent ev) {
      try {
        BoltLog.Debug("Raising {0}", ev);

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

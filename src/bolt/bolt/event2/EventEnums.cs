using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  public enum EntityTargets : byte {
    Everyone = Event.ENTITY_EVERYONE,
    EveryoneExceptController = Event.ENTITY_EVERYONE_EXCEPT_CONTROLLER
  }

  public enum GlobalTargets : byte {
    Everyone = Event.GLOBAL_EVERYONE,
    Others = Event.GLOBAL_OTHERS,
    Server = Event.GLOBAL_SERVER,
    AllClients = Event.GLOBAL_ALL_CLIENTS
  }
}

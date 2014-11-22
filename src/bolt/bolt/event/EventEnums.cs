using System;
namespace Bolt {
  [Documentation]
  public enum ReliabilityModes : byte {
    Unreliable = 0,
    //ReliableUnordered = 1,
    ReliableOrdered = 2,
  }

  [Documentation]
  public enum EntityTargets : byte {
    Everyone = NetworkEvent.ENTITY_EVERYONE,
    EveryoneExceptController = NetworkEvent.ENTITY_EVERYONE_EXCEPT_CONTROLLER,
    EveryoneExceptOwner = NetworkEvent.ENTITY_EVERYONE_EXCEPT_OWNER,
    OnlyController = NetworkEvent.ENTITY_ONLY_CONTROLLER,
    OnlyOwner = NetworkEvent.ENTITY_ONLY_OWNER,
    OnlySelf = NetworkEvent.ENTITY_ONLY_SELF,
  }

  [Documentation]
  public enum GlobalTargets : byte {
    Everyone = NetworkEvent.GLOBAL_EVERYONE,
    Others = NetworkEvent.GLOBAL_OTHERS,
    AllClients = NetworkEvent.GLOBAL_ALL_CLIENTS,
    OnlyServer = NetworkEvent.GLOBAL_ONLY_SERVER,
    OnlySelf = NetworkEvent.GLOBAL_ONLY_SELF,

    [Obsolete("Use GlobalTargets.OnlyServer")]
    Server = NetworkEvent.GLOBAL_ONLY_SERVER,
  }
}
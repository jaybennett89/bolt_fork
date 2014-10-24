namespace Bolt {
  [Documentation]
  public enum ReliabilityModes : byte {
    Unreliable = 0,
    //ReliableUnordered = 1,
    ReliableOrdered = 2,
  }

  [Documentation]
  public enum EntityTargets : byte {
    Everyone = Event.ENTITY_EVERYONE,
    EveryoneExceptController = Event.ENTITY_EVERYONE_EXCEPT_CONTROLLER,
    EveryoneExceptOwner = Event.ENTITY_EVERYONE_EXCEPT_OWNER,
    OnlyController = Event.ENTITY_ONLY_CONTROLLER,
    OnlyOwner = Event.ENTITY_ONLY_OWNER,
  }

  [Documentation]
  public enum GlobalTargets : byte {
    Everyone = Event.GLOBAL_EVERYONE,
    Others = Event.GLOBAL_OTHERS,
    Server = Event.GLOBAL_SERVER,
    AllClients = Event.GLOBAL_ALL_CLIENTS
  }
}
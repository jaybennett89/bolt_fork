using System;
namespace Bolt {

  /// <summary>
  /// The reliability mode of an event
  /// </summary>
  [Documentation]
  public enum ReliabilityModes : byte {
    Unreliable = 0,
    //ReliableUnordered = 1,
    ReliableOrdered = 2,
  }

  /// <summary>
  /// The target of an entity event
  /// </summary>
  [Documentation]
  public enum EntityTargets : byte {
    Everyone = Event.ENTITY_EVERYONE,
    EveryoneExceptController = Event.ENTITY_EVERYONE_EXCEPT_CONTROLLER,
    EveryoneExceptOwner = Event.ENTITY_EVERYONE_EXCEPT_OWNER,
    OnlyController = Event.ENTITY_ONLY_CONTROLLER,
    OnlyOwner = Event.ENTITY_ONLY_OWNER,
    OnlySelf = Event.ENTITY_ONLY_SELF,
  }

  /// <summary>
  /// The target of a global event
  /// </summary>
  [Documentation]
  public enum GlobalTargets : byte {
    Everyone = Event.GLOBAL_EVERYONE,
    Others = Event.GLOBAL_OTHERS,
    AllClients = Event.GLOBAL_ALL_CLIENTS,
    OnlyServer = Event.GLOBAL_ONLY_SERVER,
    OnlySelf = Event.GLOBAL_ONLY_SELF,

    [Obsolete("Use GlobalTargets.OnlyServer")]
    Server = Event.GLOBAL_ONLY_SERVER,
  }
}
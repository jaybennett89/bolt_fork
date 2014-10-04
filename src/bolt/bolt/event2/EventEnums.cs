using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  public enum EntityTargets : byte {
    Everyone = 0,
    EveryoneExceptController = 1,
    OnlyController = 2,
  }

  public enum GlobalTargets : byte {
    Everyone = 0,
    EveryoneExceptSender = 1,
    OnlyServer = 2,
    OnlyClients = 3
  }
}

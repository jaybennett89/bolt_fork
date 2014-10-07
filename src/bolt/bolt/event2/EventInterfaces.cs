using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  public interface IGlobalEventListener {
    bool PeristBetweenStartupAndShutdown();
  }
}

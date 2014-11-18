using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  public abstract class NetworkObject : IDisposable {
    internal int OffsetObjects;
    internal int OffsetStorage;
    internal State State;

    internal State.NetworkFrame CurrentFrame {
      get { return State.Frames.first; }
    }

    void IDisposable.Dispose() {

    }
  }
}

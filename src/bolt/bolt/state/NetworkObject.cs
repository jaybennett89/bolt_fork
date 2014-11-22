using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  public abstract class NetworkObject : IDisposable {
    internal State State;

    internal int OffsetObjects;
    internal int OffsetStorage;
    internal int OffsetSerializers;

    internal NetworkFrame CurrentFrame {
      get { return State.Frames.first; }
    }

    internal NetworkObject() {

    }

    internal void VerifySerializer(Type type, string name, int serializer, int storage, int objects) {
      State.VerifySerializer(type, name, serializer, storage, objects);
    }

    void IDisposable.Dispose() {

    }
  }

}

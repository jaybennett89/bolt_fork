using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal struct EventMetaData {
    internal TypeId TypeId;
    internal int ByteSize;
  }

  public abstract class Event : IDisposable {
    internal const int RELIABLE_WINDOW_BITS = 10;
    internal const int RELIABLE_SEQUENCE_BITS = 12;

    internal byte[] Data;
    internal int ServerFrame;

    internal EventMetaData Meta;
    internal EntityTargets EntityTargets;
    internal GlobalTargets GlobalTargets;

    internal EntityObject Entity;
    internal BoltConnection Connection;

    //internal abstract Event Clone();

    internal Event(EventMetaData meta) {
      Meta = meta;
    }

    void IDisposable.Dispose() {

    }

    //{
    //    EventData clone;

    //    clone = (EventData)MemberwiseClone();
    //    clone.Data = new byte[Data.Length];

    //    Array.Copy(Data, 0, clone.Data, 0, Data.Length);

    //    return clone;
    //  }




  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  public abstract class UdpEventBase {
    public abstract int Type { get; }

    public static implicit operator UdpEvent(UdpEventBase ev) {
      return new UdpEvent { Type = ev.Type, Object0 = ev };
    }
  }
}

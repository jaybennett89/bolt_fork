using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace UdpKit {
  class UdpEventStart : UdpEventBase {
    public UdpSocketMode Mode;
    public UdpEndPoint EndPoint;
    public ManualResetEvent ResetEvent;

    public override int Type {
      get { return UdpEvent.INTERNAL_START; }
    }
  }

  public class UdpEventStartDone : UdpEventBase {
    public UdpEndPoint EndPoint;
    public ManualResetEvent ResetEvent;

    public override int Type {
      get { return UdpEvent.PUBLIC_START_DONE; }
    }
  }

  public class UdpEventStartFailed : UdpEventBase {
    public ManualResetEvent ResetEvent;

    public override int Type {
      get { return UdpEvent.PUBLIC_START_DONE; }
    }
  }

  class UdpEventClose : UdpEventBase {
    public ManualResetEvent ResetEvent;

    public override int Type {
      get { return UdpEvent.INTERNAL_CLOSE; }
    }
  }
}

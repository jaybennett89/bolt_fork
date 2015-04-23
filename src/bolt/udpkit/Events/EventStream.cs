using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  class UdpEventStreamCreateChannel : UdpEventBase {
    public UdpChannelConfig ChannelConfig;

    public override int Type {
      get { return UdpEvent.INTERNAL_STREAM_CREATECHANNEL; }
    }
  }

  class UdpEventStreamSetBandwidth : UdpEventBase {
    public UdpConnection Connection;
    public int BytesPerSecond;

    public override int Type {
      get { return UdpEvent.INTERNAL_STREAM_SETBANDWIDTH; }
    }
  }

  class UdpEventStreamQueue : UdpEventBase {
    public UdpConnection Connection;
    public UdpStreamOp StreamOp;

    public override int Type {
      get { return UdpEvent.INTERNAL_STREAM_QUEUE; }
    }
  }

  //class UdpEventStreamDataReceived : UdpEventBase {
  //  public UdpConnection Connection;
  //  public UdpStreamData StreamData;

  //  public override int Type {
  //    get { return UdpEvent.PUBLIC_STREAM_DATARECEIVED; }
  //  }
  //}
}

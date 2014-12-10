using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class Socket_Ping : Query<Ack> {
    public Guid Local;
    public Guid Remote;
  }
}

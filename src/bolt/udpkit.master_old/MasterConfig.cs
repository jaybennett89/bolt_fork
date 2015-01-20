using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  public class MasterConfig {
    public UdpEndPoint Master;

    public UdpEndPoint Probe0;
    public UdpEndPoint Probe1;
    public UdpEndPoint Probe2;

    public UdpEndPoint Punch;
  }
}

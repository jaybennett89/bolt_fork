using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  partial class UdpSocket {
    NAT.UPnP.Client NAT_UPnP_Client;

    NAT.UPnP.Result NAT_UPnP_Result {
      get {
        if (NAT_UPnP_Client != null) {
          return NAT_UPnP_Client.Result;
        }

        return NAT.UPnP.Result.None;
      }
    }

    void NAT_UPnP_Start() {
      NAT_UPnP_Client = new NAT.UPnP.Client(platform, LocalLanEndPoint.Port);
      NAT_UPnP_Client.Start(null);
    }

    void NAT_UPnP_Stop() {
      if (NAT_UPnP_Client != null) {
        NAT_UPnP_Client.Stop();
      }
    }
  }
}

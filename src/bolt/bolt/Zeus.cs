﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  public static class Zeus {
    public static void Connect(UdpEndPoint endpoint) {
      if (BoltNetwork.UdpSocket.GameId == Guid.Empty) {
        BoltLog.Error("Invalid game id, can't connect to Zeus server.");
        return;
      }

      BoltNetwork.MasterServerConnect(endpoint);
    }

    public static void Disconnect() {
      BoltNetwork.MasterServerDisconnect();
    }

    public static void RequestSessionList() {
      BoltNetwork.MasterServerRequestSessionList();
    }
  }

  public static class UdpSessionExtensions {
    public static IProtocolToken GetProtocolToken(this UdpSession session) {
      if (session._hostData == null || session._hostData.Length == 0) {
        return null;
      }

      if (session._hostObject == null) {
        session._hostObject = session._hostData.ToToken();
      }

      return (IProtocolToken)session._hostObject;
    }
  }
}
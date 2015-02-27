using Bolt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  public static class Zeus {
    public static bool IsConnected {
      get { return BoltNetwork.isRunning && BoltNetwork.UdpSocket.ConnectedToMaster; }
    }

    public static int Hosts {
      get { return BoltNetwork.isRunning ? BoltNetwork.UdpSocket.ZeusInfoHosts : 0; }
    }

    public static int ClientsInZeus {
      get { return BoltNetwork.isRunning ? BoltNetwork.UdpSocket.ZeusInfoClientsInZeus : 0; }
    }

    public static int ClientsInGames {
      get { return BoltNetwork.isRunning ? BoltNetwork.UdpSocket.ZeusInfoClientsInGames : 0; }
    }

    public static void RequestInfo() {
      BoltNetwork.VerifyIsRunning();
      BoltNetwork.UdpSocket.MasterServerRequestInfo();
    }

    public static void Connect(UdpEndPoint endpoint) {
      BoltNetwork.VerifyIsRunning();

      if (BoltNetwork.UdpSocket.GameId == Guid.Empty) {
        BoltLog.Error("Invalid game id, can't connect to Zeus server.");
        return;
      }

      BoltNetwork.MasterServerConnect(endpoint);
    }

    public static void Disconnect() {
      BoltNetwork.VerifyIsRunning();
      BoltNetwork.MasterServerDisconnect();
    }

    public static void RequestSessionList() {
      BoltNetwork.VerifyIsRunning();
      BoltNetwork.MasterServerRequestSessionList();
    }
  }
}
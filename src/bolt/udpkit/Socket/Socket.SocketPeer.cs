using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  partial class UdpSocket {
    class Socket_Info {
      public bool Punch;
      public uint PunchPing;
      public uint PunchPackets;
      public uint PunchTime;
      public UdpEndPoint PunchTarget;
    }

    Socket_Info SocketInfo = new Socket_Info();

    void Socket_Update(uint now) {
      if (SocketInfo.Punch) {
        if (SocketInfo.PunchPackets < 10) {
          if (SocketInfo.PunchTime < now) {
            // send packet
            platformSocketPeer.Message_Send<Protocol.Socket_Punch>(SocketInfo.PunchTarget);

            // update punch time
            SocketInfo.PunchTime = (platform.GetPrecisionTime() + 1000) - SocketInfo.PunchPing;

            // 
            SocketInfo.PunchPackets += 1;
          }
        }
        else {
          SocketInfo = new Socket_Info();
        }
      }
    }

    void Socket_Ping(Protocol.Socket_Ping ping) {
      platformSocketPeer.Message_Ack(ping);
    }

    void Socket_Punch(Protocol.Socket_Punch punch) {
      platformSocketPeer.Message_Ack(punch);
    }

    void Socket_NatPunch_PeerRegister_Ack(Protocol.NatPunch_PeerRegister register) {

    }

    void Socket_Punch_Ack(Protocol.Socket_Punch punch) {
      if (punch.Result != null && SocketInfo != null && SocketInfo.Punch && mode == UdpSocketMode.Client) {
        SocketInfo = new Socket_Info();
        ConnectToEndPoint(punch.Result.Sender, null);
      }
    }

    void NatPunch_PunchInfo(Protocol.NatPunch_PunchInfo msg) {
      platformSocketPeer.Message_Ack(msg);

      SocketInfo = new Socket_Info();

      SocketInfo.Punch = true;
      SocketInfo.PunchPackets = 0;
      SocketInfo.PunchPing = msg.Ping;
      SocketInfo.PunchTarget = msg.PunchTo;
      SocketInfo.PunchTime = (platform.GetPrecisionTime() + 1000) - SocketInfo.PunchPing;
    }
  }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  partial class UdpSocket {
    class SessionHandler {
      public Guid Id;
      public string Name;
      public byte[] Data;
      public UdpBag<UdpSession> Sessions = new UdpBag<UdpSession>();

      public SessionHandler () {
        Id = Guid.NewGuid();
      }

      public void UpdateSession (UdpSession session) {
        if (Sessions.Update(s => s.WanEndPoint.Address == session.WanEndPoint.Address, s => session) == false) {
          Sessions.Add(session);
        } 
      }

      public void RemoveOldSessions (uint now) {
      }
    }
  }
}

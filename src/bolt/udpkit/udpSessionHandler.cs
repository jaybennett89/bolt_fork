using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  partial class UdpSocket {
    class UdpSessionHandler {
      public Guid Id;
      public string Name;
      public string Data;
      public UdpBag<UdpSession> Sessions = new UdpBag<UdpSession>();

      public UdpSessionHandler () {
        Id = Guid.NewGuid();
      }

      public void UpdateSession (UdpSession session) {
        if (Sessions.Update(s => s.EndPoint.Address == session.EndPoint.Address, s => session) == false) {
          Sessions.Add(session);
        }
      }

      public void RemoveOldSessions (uint now) {
        Sessions.Filter(s => (s.LastUpdate + 10000) > now);
      }
    }
  }
}

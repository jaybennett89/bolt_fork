using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  partial class UdpSocket {
    class SessionManager {
      uint eventTime;
      Map<Guid, UdpSession> sessions;

      readonly UdpSocket socket;
      readonly UdpSession local;

      public bool IsHostWithName {
        get { return socket.mode == UdpSocketMode.Host && local.HostName.HasValue(); }
      }

      public SessionManager(UdpSocket s) {
        socket = s;
        sessions = new Map<Guid, UdpSession>();

        local = new UdpSession();
        local._id = s.PeerId;
      }

      public void Update(uint now) {
        if (eventTime > 0 && eventTime < now) {
          RaiseSessionUpdatedEvent();
        }
      }

      public UdpSession GetLocalSession() {
        return local.Clone();
      }

      public void SetHostInfo(string name, byte[] data) {
        local._hostName = name;
        local._hostData = data;

        if (IsHostWithName && socket.masterClient != null) {
          socket.masterClient.RegisterHost();
        }
      }

      public void UpdateSession(UdpSession session, UdpSessionSource source) {
        // set source
        session._source = source;

        // update session based on id
        sessions = sessions.Update(session.Id, session);

        // if event time is zero, set event to raise in 500ms
        if (eventTime == 0) {
          eventTime = socket.GetCurrentTime() + 500;
        }
      }

      public void ForgetSessionsAll() {
        sessions = new Map<Guid, UdpSession>();

        // tell user
        RaiseSessionUpdatedEvent();
      }

      public void ForgetSessions(UdpSessionSource source) {
        var newSessions = sessions;

        foreach (var s in sessions) {
          if (s.Value.Source == source) {
            newSessions = newSessions.Remove(s.Key);
          }
        }

        // store here
        sessions = newSessions;

        // tell user
        RaiseSessionUpdatedEvent();
      }

      public void SetWanEndPoint(UdpEndPoint endpoint) {
        local._wanEndPoint = endpoint;
      }

      public void SetLanEndPoint(UdpEndPoint endpoint) {
        local._lanEndPoint = endpoint;
      }

      void RaiseSessionUpdatedEvent() {
        try {
          UdpEvent ev = new UdpEvent();

          ev.Type = UdpEvent.PUBLIC_SESSION_LISTUPDATED;
          ev.SessionList = sessions;

          socket.Raise(ev);
        }
        finally {
          eventTime = 0;
        }
      }

    }
  }
}

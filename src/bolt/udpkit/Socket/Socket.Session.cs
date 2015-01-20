using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  partial class UdpSocket {
    const int SESSION_UPDATE_RATE = 100;
    const int SESSION_LISTUPDATED_EVENT_DELAY = 500;

    class SessionInfo {
      public uint LastUpdate;
      public uint ListUpdated_EventTime;

      public UdpSession Local = new UdpSession();
      public Map<Guid, UdpSession> SessionsList = new Map<Guid, UdpSession>();
    }

    SessionInfo Session = new SessionInfo();

    void Session_Host_SetInfo(string hostName, byte[] hostData) {
      Session.Local = new UdpSession();
      Session.Local._hostName = hostName;
      Session.Local._hostData = hostData;

      MasterServer_HostRegister();
    }

    void Session_Connect(UdpSession session, byte[] connectToken) {
      // lan session
      if (session.HasLan) {
        ConnectToEndPoint(session.LanEndPoint, connectToken);
      }

      // wan session
      else if (session.HasWan) {
        MasterServer_Connect(session, connectToken);
      }

      // 
      else {
        UdpLog.Error("Incorrect session passed in, neither LAN or WAN");
      }
    }

    void Session_Update(uint now) {
      if ((Session.LastUpdate + SESSION_UPDATE_RATE) < now) {
        if ((Session.ListUpdated_EventTime > 0) && (Session.ListUpdated_EventTime < now)) {
          Session_ListUpdated_Event_Raise();
        }

        Session.LastUpdate = now;
      }
    }

    void Session_Add(UdpSession session) {
      Session.SessionsList = Session.SessionsList.Update(session.Id, session);

      if (Session.ListUpdated_EventTime == 0) {
        Session.ListUpdated_EventTime = GetCurrentTime() + SESSION_LISTUPDATED_EVENT_DELAY;
      }
    }

    void Session_ListUpdated_Event_Raise() {
      UdpEvent ev = new UdpEvent();

      ev.Type = UdpEvent.PUBLIC_SESSION_LISTUPDATED;
      ev.SessionList = Session.SessionsList;

      Raise(ev);

      // set this to zero
      Session.ListUpdated_EventTime = 0;
    }
  }
}

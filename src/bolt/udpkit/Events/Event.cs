using System.Runtime.InteropServices;
using System.Threading;

namespace UdpKit {
  public enum UdpSendFailReason {
    None,
    NotConnected,
    PacketWindowFull
  }

  public enum UdpEventType {
    SocketStartupDone = UdpEvent.PUBLIC_START_DONE,
    SocketStartupFailed = UdpEvent.PUBLIC_START_FAILED,

    ConnectRequest = UdpEvent.PUBLIC_CONNECT_REQUEST,
    ConnectFailed = UdpEvent.PUBLIC_CONNECT_FAILED,
    ConnectRefused = UdpEvent.PUBLIC_CONNECT_REFUSED,
    ConnectAttempt = UdpEvent.PUBLIC_CONNECT_ATTEMPT,

    Connected = UdpEvent.PUBLIC_CONNECTED,
    Disconnected = UdpEvent.PUBLIC_DISCONNECTED,

    PacketLost = UdpEvent.PUBLIC_PACKET_LOST,
    PacketReceived = UdpEvent.PUBLIC_PACKET_RECEIVED,
    PacketDelivered = UdpEvent.PUBLIC_PACKET_DELIVERED,

    StreamDataReceived = UdpEvent.PUBLIC_STREAM_DATARECEIVED,

    SessionListUpdated = UdpEvent.PUBLIC_SESSION_LISTUPDATED,
    SessionConnectFailed = UdpEvent.PUBLIC_SESSION_CONNECTFAILED,

    MasterServerConnectFailed = UdpEvent.PUBLIC_MASTERSERVER_CONNECTFAILED,
    MasterServerConnected = UdpEvent.PUBLIC_MASTERSERVER_CONNECTED,
    MasterServerDisconnected = UdpEvent.PUBLIC_MASTERSERVER_DISCONNECTED,
    MasterServerNatProbeResult = UdpEvent.PUBLIC_MASTERSERVER_NATPROBE_RESULT,
  }

  public partial struct UdpEvent {
    internal const int INTERNAL_START = 1;
    internal const int INTERNAL_CONNECT = 3;
    internal const int INTERNAL_CONNECT_CANCEL = 5;
    internal const int INTERNAL_ACCEPT = 7;
    internal const int INTERNAL_REFUSE = 9;
    internal const int INTERNAL_DISCONNECT = 11;
    internal const int INTERNAL_CLOSE = 13;
    internal const int INTERNAL_SEND = 15;

    internal const int INTERNAL_LANBROADCAST_ENABLE = 17;
    internal const int INTERNAL_LANBROADCAST_DISABLE = 19;

    internal const int INTERNAL_FORGETSESSIONS = 21;
    internal const int INTERNAL_FORGETSESSIONS_ALL = 23;

    internal const int INTERNAL_STREAM_CREATECHANNEL = 25;
    internal const int INTERNAL_STREAM_QUEUE = 27;
    internal const int INTERNAL_STREAM_SETBANDWIDTH = 29;

    internal const int INTERNAL_SESSION_CONNECT = 31;
    internal const int INTERNAL_SESSION_HOST_SETINFO = 33;

    internal const int INTERNAL_MASTERSERVER_CONNECT = 35;
    internal const int INTERNAL_MASTERSERVER_DISCONNECT = 37;
    internal const int INTERNAL_MASTERSERVER_SESSIONLISTREQUEST = 39;
    internal const int INTERNAL_MASTERSERVER_INFOREQUEST = 43;

    internal const int PUBLIC_CONNECT_REQUEST = 2;
    internal const int PUBLIC_CONNECT_FAILED = 4;
    internal const int PUBLIC_CONNECT_REFUSED = 6;

    internal const int PUBLIC_CONNECTED = 8;
    internal const int PUBLIC_CONNECT_ATTEMPT = 10;
    internal const int PUBLIC_DISCONNECTED = 12;

    internal const int PUBLIC_PACKET_LOST = 14;
    internal const int PUBLIC_PACKET_RECEIVED = 16;
    internal const int PUBLIC_PACKET_DELIVERED = 18;

    internal const int PUBLIC_STREAM_DATARECEIVED = 20;

    internal const int PUBLIC_START_DONE = 22;
    internal const int PUBLIC_START_FAILED = 24;

    internal const int PUBLIC_MASTERSERVER_CONNECTED = 26;
    internal const int PUBLIC_MASTERSERVER_DISCONNECTED = 28;
    internal const int PUBLIC_MASTERSERVER_NATPROBE_RESULT = 32;
    internal const int PUBLIC_MASTERSERVER_CONNECTFAILED = 34;

    internal const int PUBLIC_SESSION_LISTUPDATED = 30;
    internal const int PUBLIC_SESSION_CONNECTFAILED = 36;

    internal int Type;
    internal object Object0;
    internal object Object1;

    internal bool IsInternal {
      get { return (Type & 1) == 1; }
    }

    internal T As<T>() {
      return (T)Object0;
    }

    public UdpEventType EventType {
      get { return (UdpEventType)Type; }
    }
  }
}

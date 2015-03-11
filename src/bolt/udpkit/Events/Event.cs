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

  //class UdpEventAcceptArgs {
  //  public object UserObject;
  //}

  //class UdpEventBroadcastArgs {
  //  public UdpIPv4Address LocalAddress;
  //  public UdpIPv4Address BroadcastAddress;
  //  public ushort Port;
  //}

  //class UdpHostInfoArgs {
  //  public string Name;
  //  public byte[] Data;
  //  public bool Dedicated;
  //}

  //public class UdpTokenArgs {
  //  public byte[] ConnectToken;
  //  public byte[] DisconnectToken;
  //  public byte[] AcceptToken;
  //  public byte[] RefuseToken;
  //}

  [StructLayout(LayoutKind.Explicit)]
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

    [FieldOffset(0)]
    internal int Type;

    [FieldOffset(4)]
    internal object Object0;

    [FieldOffset(12)]
    internal object Object1;

    internal bool IsInternal {
      get { return (Type & 1) == 1; }
    }

    public T As<T>() {
      return (T)Object0;
    }

    public UdpEventType EventType {
      get { return (UdpEventType)Type; }
    }

    //internal int ChannelRate {
    //  get { return integer0; }
    //  set { integer0 = value; }
    //}

    //internal int ChannelNameId {
    //  get { return integer0; }
    //  set { integer0 = value; }
    //}

    //internal string ChannelNameString {
    //  get { return (string)Object0; }
    //  set {
    //    UdpAssert.Assert(Object0 == null);
    //    Object0 = value;
    //  }
    //}

    //internal UdpSessionSource SessionSource {
    //  get { return (UdpSessionSource)integer0; }
    //  set { integer0 = (int)value; }
    //}

    //internal UdpSocketMode SocketMode {
    //  get { return (UdpSocketMode)integer2; }
    //  set { integer2 = (int)value; }
    //}

    //internal UdpChannelName ChannelName {
    //  get { return new UdpChannelName(integer0, (string)Object0); }
    //  set {
    //    UdpAssert.Assert(Object0 == null);
    //    integer0 = value.Id;
    //    Object0 = value.Name;
    //  }
    //}

    //public UdpEventType EventType {
    //  get { return (UdpEventType)Type; }
    //}

    //public UdpEndPoint EndPoint {
    //  get { return endpoint; }
    //  internal set { endpoint = value; }
    //}

    //public UdpSendFailReason FailedReason {
    //  get { return (UdpSendFailReason)integer0; }
    //  internal set { integer0 = (int)value; }
    //}

    //public int ByteArraySize {
    //  get { return integer2; }
    //  set { integer2 = value; }
    //}

    ///*
    // * OBJECT 1
    // * */

    //public UdpConnection Connection {
    //  get { return (UdpConnection)Object1; }
    //  set { UdpAssert.Assert(Object1 == null); Object1 = value; }
    //}

    //public UdpTokenArgs Tokens {
    //  get { return (UdpTokenArgs)Object1; }
    //  set { UdpAssert.Assert(Object1 == null); Object1 = value; }
    //}

    //public Map<System.Guid, UdpSession> SessionList {
    //  get { return (Map<System.Guid, UdpSession>)Object1; }
    //  set { UdpAssert.Assert(Object1 == null); Object1 = value; }
    //}

    ///* 
    // * OBJECT 0
    // * */

    //public NatFeatures NatFeatures {
    //  get { return (NatFeatures)Object0; }
    //  set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    //}

    //public byte[] ByteArray {
    //  get { return (byte[])Object0; }
    //  set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    //}

    //internal UdpEventBroadcastArgs BroadcastArgs {
    //  get { return (UdpEventBroadcastArgs)Object0; }
    //  set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    //}

    //public ManualResetEvent ResetEvent {
    //  get { return (ManualResetEvent)Object0; }
    //  set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    //}

    //public UdpSession Session {
    //  get { return (UdpSession)Object0; }
    //  set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    //}

    //public UdpPacket Packet {
    //  get { return (UdpPacket)Object0; }
    //  set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    //}

    //public UdpStreamData StreamData {
    //  get { return (UdpStreamData)Object0; }
    //  set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    //}

    //internal UdpHostInfoArgs HostInfo {
    //  get { return (UdpHostInfoArgs)Object0; }
    //  set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    //}

    //internal UdpChannelConfig ChannelConfig {
    //  get { return (UdpChannelConfig)Object0; }
    //  set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    //}

    //internal UdpEventAcceptArgs AcceptArgs {
    //  get { return (UdpEventAcceptArgs)Object0; }
    //  set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    //}

    //internal UdpStreamOp StreamOp {
    //  get { return (UdpStreamOp)Object0; }
    //  set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    //}

  }
}

using System;
namespace UdpKit {
  public delegate float UdpNoise ();

  public class UdpConfig {
    public int PacketWindow = 256;
    public int PacketDatagramSize = 1400;

    public int StreamWindow = 1024;
    public int StreamDatagramSize = 4096;

    /// <summary>
    /// The default network ping for new connections, default: 0.1f (seconds)
    /// </summary>
    public float DefaultNetworkPing = 0.1f;

    /// <summary>
    /// The default aliased ping for new connections, default: 0.15f (seconds)
    /// </summary>
    public float DefaultAliasedPing = 0.15f;

    /// <summary>
    /// If we allow serialization to overflow MTU of the connection, default: false
    /// </summary>
    public bool AllowPacketOverflow = false;

    /// <summary>
    /// If we should flip the AutoResetEvent which signals to the
    /// user thread if we have available events on a socket, default: true
    /// </summary>
    public bool UseAvailableEventEvent = true;

    /// <summary>
    /// The timeout until we should make a new connect request, default: 1000 (milliseconds)
    /// </summary>
    public uint ConnectRequestTimeout = 1000;

    /// <summary>
    /// How many attempts we should make to connect before failing, default: 5
    /// </summary>
    public uint ConnectRequestAttempts = 5;

    /// <summary>
    /// How long before we time-out a connection we have not heard anything from, default: 5000 (ms)
    /// </summary>
    public uint ConnectionTimeout = 5000;

    /// <summary>
    /// How long we should wait to send a ping packet to the remote end if we 
    /// have not sent anything recently, default: 100 (ms)
    /// </summary>
    public uint PingTimeout = 100;

    /// <summary>
    /// How many packets we can receive before we force an ack packet to be sent, default: 8
    /// </summary>
    public uint RecvWithoutAckLimit = 8;

    /// <summary>
    /// How many % of the packets we should drop to simulate packet loss, default: 0. Only used in DEBUG builds.
    /// </summary>
    public float SimulatedLoss = 0f;

    /// <summary>
    /// Min ping we should simulate, default: 0 (ms). Only used in DEBUG builds.
    /// </summary>
    public int SimulatedPingMin = 0;

    /// <summary>
    /// Max ping we should simulate, default: 0 (ms). Only used in DEBUG builds.
    /// </summary>
    public int SimulatedPingMax = 0;


    /// <summary>
    /// How many connections we allow, default: 64
    /// </summary>
    public int ConnectionLimit = 64;

    /// <summary>
    /// If we allow incomming connections, default: true
    /// </summary>
    public bool AllowIncommingConnections = true;

    /// <summary>
    /// IF we automatically accept incomming connections if we have slots free, default: true
    /// </summary>
    public bool AutoAcceptIncommingConnections = true;

    /// <summary>
    /// If we allow clients which are connecting to a server to implicitly accept the connection
    /// if we get a non-rejected and non-accepted packet from the server, meaning the accept packet
    /// was lost, default: true
    /// </summary>
    public bool AllowImplicitAccept = true;

    /// <summary>
    /// How large the event queues should by default, default: 4096
    /// </summary>
    public int InitialEventQueueSize = 4096;

    /// <summary>
    /// Custom noise function for use in packet loss simulation, default: null
    /// </summary>
    public UdpNoise NoiseFunction = null;

    internal UdpConfig Duplicate () {
      return (UdpConfig) MemberwiseClone();
    }
  }
}

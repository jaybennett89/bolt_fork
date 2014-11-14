using System;
using System.Collections.Generic;
using UdpKit;

internal class BoltPacketInfo {
  public int commandBits;
  public int entityBits;
  public int eventBits;
}

internal class BoltPacket : IDisposable {
  public static int packetSize {
    get { return BoltCore._udpConfig.PacketSize - UdpHeader.SIZE_BYTES; }
  }

  internal Bolt.NetworkId NetworkIdBlock;

  internal int number;
  internal PacketStats stats;
  internal volatile bool pooled = true;
  internal List<Bolt.EventReliable> eventReliable = new List<Bolt.EventReliable>();
  internal Queue<EntityProxyEnvelope> ProxyEnvelopes = new Queue<EntityProxyEnvelope>();

  public int frame { get; internal set; }
  public UdpPacket stream { get; internal set; }
  public IDisposable userToken { get; set; }

  void IDisposable.Dispose() {
    BoltPacketPool.Dispose(this);
  }
}

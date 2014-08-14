using System;
using UdpKit;

internal class BoltPacketInfo {
  public int commandBits;
  public int entityBits;
  public int eventBits;
}

internal class BoltPacket : IDisposable {
  public static int packetSize {
    get { return BoltCore.udpConfig.PacketSize - BoltMath.BytesRequired(UdpSocket.HeaderBitSize); }
  }

  internal int number;
  internal volatile bool pooled = true;
  internal BoltPacketInfo info = null;
  internal BoltSingleList<BoltEntityProxyEnvelope> envelopes = new BoltSingleList<BoltEntityProxyEnvelope>();
  internal BoltSingleList<BoltEventReliable> eventReliable = new BoltSingleList<BoltEventReliable>();

  public int frame { get; internal set; }
  public UdpStream stream { get; internal set; }
  public IDisposable userToken { get; set; }

  void IDisposable.Dispose () {
    BoltPacketPool.Dispose(this);
  }
}

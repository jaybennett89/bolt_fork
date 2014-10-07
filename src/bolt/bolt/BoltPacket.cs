﻿using System;
using System.Collections.Generic;
using UdpKit;

internal class BoltPacketInfo {
  public int commandBits;
  public int entityBits;
  public int eventBits;
}

internal class BoltPacket : IDisposable {
  public static int packetSize {
    get { return BoltCore._udpConfig.PacketSize - BoltMath.BytesRequired(UdpSocket.HeaderBitSize); }
  }

  internal int number;
  internal volatile bool pooled = true;
  internal BoltPacketInfo info = null;
  internal BoltSingleList<EntityProxyEnvelope> envelopes = new BoltSingleList<EntityProxyEnvelope>();
  internal List<Bolt.EventReliable> eventReliable = new List<Bolt.EventReliable>();

  public int frame { get; internal set; }
  public UdpStream stream { get; internal set; }
  public IDisposable userToken { get; set; }

  void IDisposable.Dispose() {
    BoltPacketPool.Dispose(this);
  }
}

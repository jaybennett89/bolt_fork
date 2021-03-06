﻿using System;
using System.Collections.Generic;
using UdpKit;


namespace Bolt {
  internal class Packet : IDisposable {
    public volatile bool Pooled = true;

    public int Frame;
    public int Number;

    public PacketStats Stats;
    public UdpPacket UdpPacket;

    public List<Bolt.EventReliable> ReliableEvents = new List<Bolt.EventReliable>();
    public Queue<EntityProxyEnvelope> EntityUpdates = new Queue<EntityProxyEnvelope>();

    void IDisposable.Dispose() {
      PacketPool.Dispose(this);
    }
  }
}
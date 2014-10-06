using System;
using System.Collections.Generic;

class BoltEventChannel : BoltChannel {

  BoltSendBuffer<BoltEventReliable> _reliableSend;
  BoltRecvBuffer<BoltEventReliable> _reliableRecv;

  Queue<BoltEventBase> _unreliableSend;
  Queue<BoltEventBase> _unreliableRecv;

  Queue<BoltEventBase> _unreliableSyncedSend;
  Queue<BoltEventBase> _unreliableSyncedRecv;

  public BoltEventChannel () {
    _reliableSend = new BoltSendBuffer<BoltEventReliable>(BoltEventBase.RELIABLE_WINDOW_BITS, BoltEventBase.RELIABLE_SEQUENCE_BITS);
    _reliableRecv = new BoltRecvBuffer<BoltEventReliable>(BoltEventBase.RELIABLE_WINDOW_BITS, BoltEventBase.RELIABLE_SEQUENCE_BITS);

    _unreliableSend = new Queue<BoltEventBase>();
    _unreliableRecv = new Queue<BoltEventBase>();

    _unreliableSyncedSend = new Queue<BoltEventBase>();
    _unreliableSyncedRecv = new Queue<BoltEventBase>();
  }

  public void Queue (BoltEventBase evnt) {
    Assert.True(evnt._refCount > 0);

    if (evnt.FilterSend(connection)) {
      switch (evnt._deliveryMode) {
        case BoltEventDeliveryMode.Reliable:
          BoltEventReliable reliable = new BoltEventReliable();
          reliable.evnt = evnt;

          if (_reliableSend.TryEnqueue(reliable) == false) {
            BoltLog.Warn("reliable event send queue for {0} is full, disconnecting", connection);
            connection.Disconnect();
          }
          break;

        case BoltEventDeliveryMode.Unreliable:
          _unreliableSend.Enqueue(evnt);
          break;

        case BoltEventDeliveryMode.UnreliableSynced:
          _unreliableSyncedSend.Enqueue(evnt);
          break;
      }

      evnt.RefCountIncrement();
    }
  }

  public override void Pack (BoltPacket packet) {
    int startPos = packet.stream.Position;

    // write reliable events
    {
      BoltEventReliable reliable;

      //while (_reliableSend.TryNext(out reliable)) {
      //  if (PackReliableEvent(packet, reliable.evnt, reliable.Sequence)) {
      //    packet.eventReliable.AddLast(reliable);
      //  } else {
      //    _reliableSend.SetSend(reliable);
      //    break;
      //  }
      //}

      packet.stream.WriteStopMarker();
    }

    // write unreliable synced events
    {
      while (_unreliableSyncedSend.Count > 0) {
        int pos = packet.stream.Position;

        if (PackUnreliableEvent(packet, _unreliableSyncedSend.Peek())) {
          _unreliableSyncedSend.Dequeue().Dispose();
        } else {
          break;
        }
      }

      packet.stream.WriteStopMarker();
    }

    // write unreliable events
    {
      while (_unreliableSend.Count > 0) {
        int pos = packet.stream.Position;

        if (PackUnreliableEvent(packet, _unreliableSend.Peek())) {
          _unreliableSend.Dequeue().Dispose();
        } else {
          break;
        }
      }

      packet.stream.WriteStopMarker();
    }

    packet.info.eventBits = packet.stream.Position - startPos;

    // throw away frame synced events which could not be sent
    if (_unreliableSyncedSend.Count > 0) {
      BoltLog.Warn("could not send all frame synced events, {0} events will be disposed", _unreliableSyncedSend.Count);

      while (_unreliableSyncedSend.Count > 0) {
        _unreliableSyncedSend.Dequeue().Dispose();
      }
    }

    // warn if we have any unsent unreliable events
    if (_unreliableSend.Count > 0) {
      BoltLog.Warn("could not send all unreliable events, {0} events remain in queue", _unreliableSend.Count);
    }
  }

  public override void Read (BoltPacket packet) {
    // read reliable events
    {
      while (packet.stream.CanRead()) {
        BoltEventReliable reliable;

        if (ReadReliableEvent(packet, out reliable)) {
          if (_reliableRecv.TryEnqueue(reliable) == RecvBufferAddResult.Added) {
            //BoltLog.Debug("received {0}", reliable);

          } else {
            reliable.Dispose();
          }
        } else {
          break;
        }
      }
    }

    // read unreliable synced events
    {
      while (packet.stream.CanRead()) {
        BoltEventBase evnt;

        if (ReadUnreliableEvent(packet, out evnt)) {
          _unreliableSyncedRecv.Enqueue(evnt);
          //BoltLog.Debug("received {0}", evnt);
        } else {
          break;
        }
      }
    }

    // read unreliable events
    {
      while (packet.stream.CanRead()) {
        BoltEventBase evnt;

        if (ReadUnreliableEvent(packet, out evnt)) {
          _unreliableRecv.Enqueue(evnt);
          //BoltLog.Debug("received {0}", evnt);
        } else {
          break;
        }
      }
    }
  }

  public override void Lost (BoltPacket packet) {
    //// mark all lost events
    //while (packet.eventReliable.count > 0) {
    //  BoltEventReliable reliable = packet.eventReliable.RemoveFirst();
    //  Assert.True(reliable.evnt._refCount > 0);
    //  _reliableSend.SetSend(reliable);
    //  //BoltLog.Debug("lost {0} ... resending", reliable);
    //}
  }

  public override void Delivered (BoltPacket packet) {
    //// mark all delivered events
    //while (packet.eventReliable.count > 0) {
    //  BoltEventReliable reliable = packet.eventReliable.RemoveFirst();
    //  Assert.True(reliable.evnt._refCount > 0);
    //  _reliableSend.SetDelivered(reliable);
    //}

    //// dipose all events which have been delivered in order
    //{
    //  BoltEventReliable reliable;

    //  while (_reliableSend.TryRemove(out reliable)) {
    //    //BoltLog.Debug("delivered {0}", reliable);
    //    reliable.Dispose();
    //  }
    //}
  }

  public override void ReadDone () {
    // invoke reliable events
    {
      BoltEventReliable reliable;

      while (_reliableRecv.TryRemove(out reliable)) {
        BoltEventBase.Invoke(reliable.evnt);

        reliable.evnt = null;
        reliable.Dispose();
      }
    }

    // invoke unreliable events
    {
      while (_unreliableRecv.Count > 0) {
        BoltEventBase evnt = _unreliableRecv.Dequeue();
        evnt.GrabEntity(connection);
        BoltEventBase.Invoke(evnt);
      }
    }

    // grab entity objects for frame synced events
    {
      foreach (BoltEventBase evnt in _unreliableSyncedRecv) {
        evnt.GrabEntity(connection);
      }
    }
  }

  public override void StepRemoteFrame () {
    // invoke unreliable frame synced events
    {
      while (_unreliableSyncedRecv.Count > 0) {

        if (_unreliableSyncedRecv.Peek()._frame == connection.remoteFrame) {
          BoltEventBase evnt = _unreliableSyncedRecv.Dequeue();
          BoltLog.Warn("invoking {0} on correct frame ({1})", evnt, evnt._frame);
          BoltEventBase.Invoke(evnt);

        } else if (_unreliableSyncedRecv.Peek()._frame < connection.remoteFrame) {
          BoltEventBase evnt = _unreliableSyncedRecv.Dequeue();
          BoltLog.Warn("invoking {0} on invalid frame (expected: {1}, was: {2})", evnt, evnt._frame, connection.remoteFrame);
          BoltEventBase.Invoke(evnt);

        } else {
          if (_unreliableSyncedRecv.Peek()._frame - BoltCore.localInterpolationDelayMax > connection.remoteFrame) {
            BoltEventBase evnt = _unreliableSyncedRecv.Dequeue();
            BoltLog.Warn("disposing of uninvokved {0} as frame is out of sync (expected: {1}, was: {2})", evnt, connection.remoteFrame + BoltCore.localInterpolationDelayMax, evnt._frame);
            evnt.Dispose();
          }
          break;
        }
      }
    }
  }

  bool PackUnreliableEvent (BoltPacket packet, BoltEventBase evnt) {
    int pos = packet.stream.Position;

    Assert.True(evnt._refCount > 0);
    Assert.True(evnt._deliveryMode != BoltEventDeliveryMode.Reliable);

    packet.stream.WriteBool(true);
    packet.stream.WriteUShort(evnt._id);

    if (evnt._isEntityEvent) {
      // entity doesn't exist anymore
      if (!evnt._entity) {
        packet.stream.Position = pos;
        return true;
      }

      var networkId = connection._entityChannel.GetNetworkId(evnt._entity);

      // entity doesn't exist on remote
      if (networkId.Value == int.MaxValue) {
        packet.stream.Position = pos;
        return true;
      }

      packet.stream.WriteBool(ReferenceEquals(evnt._entity.Source, connection));
      packet.stream.WriteNetworkId(networkId);
    }

    if (evnt._deliveryMode == BoltEventDeliveryMode.UnreliableSynced) {
      int diff = packet.frame - evnt._frame;

      // if this event is older then our send rate then we're already too late
      if (diff >= BoltCore.localSendRate) {
        packet.stream.Position = pos;
        return true;
      }

      packet.stream.WriteByte((byte) diff);
    }

    evnt.Pack(packet.stream, connection);

    if (packet.stream.Overflowing) {
      packet.stream.Position = pos;
      return false;

    } else {
      return true;
    }
  }

  bool ReadUnreliableEvent (BoltPacket packet, out BoltEventBase evnt) {
    evnt = null;

    //Assert.False(packet.stream.Overflowing);

    //if (packet.stream.ReadBool() == false) {
    //  evnt = null;
    //  return false;
    //}

    //Assert.False(packet.stream.Overflowing);

    //// create event object
    ////evnt = BoltFactory.NewEvent(packet.stream.ReadUShort());

    //if (evnt._isEntityEvent) {
    //  evnt._entityIsOutgoing = packet.stream.ReadBool();
    //  evnt._entityNetworkId = packet.stream.ReadNetworkId();
    //}

    //if (evnt._deliveryMode == BoltEventDeliveryMode.UnreliableSynced) {
    //  evnt._frame = packet.frame - packet.stream.ReadByte();
    //}

    //Assert.False(packet.stream.Overflowing);
    //evnt._connection = connection;
    //evnt.Read(packet.stream, connection);
    //Assert.False(packet.stream.Overflowing);

    return true;
  }


  bool PackReliableEvent (BoltPacket packet, BoltEventBase evnt, uint sequence) {
    int pos = packet.stream.Position;
    Assert.True(evnt._refCount > 0);

    packet.stream.WriteBool(true);
    packet.stream.WriteUShort(evnt._id);
    packet.stream.WriteUInt(sequence);

    evnt.Pack(packet.stream, connection);

    if (packet.stream.Overflowing) {
      packet.stream.Position = pos;
      return false;

    } else {
      //BoltLog.Debug("sending {0}", evnt);
      return true;
    }
  }

  bool ReadReliableEvent (BoltPacket packet, out BoltEventReliable reliable) {
    reliable = null;

    //if (packet.stream.ReadBool() == false) {
    //  reliable = null;
    //  return false;
    //}

    //ushort eventid = packet.stream.ReadUShort();
    //uint sequence = packet.stream.ReadUInt();

    //reliable = new BoltEventReliable();
    //reliable.Sequence = sequence;
    //reliable.evnt = BoltFactory.NewEvent(eventid);
    //reliable.evnt._connection = connection;
    //reliable.evnt.Read(packet.stream, connection);

    //Assert.True(reliable.evnt._refCount > 0);
    return true;
  }
}

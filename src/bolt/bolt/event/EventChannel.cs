using System.Collections.Generic;
using UdpKit;

namespace Bolt {
  struct EventUnreliable {
    internal class PriorityComparer : IComparer<EventUnreliable> {
      public static readonly PriorityComparer Instance = new PriorityComparer();

      PriorityComparer() {

      }

      int IComparer<EventUnreliable>.Compare(EventUnreliable x, EventUnreliable y) {
        return y.Priority.CompareTo(x.Priority);
      }
    }

    public bool Skipped;
    public NetworkEvent NetworkEvent;
    public float Priority;

    public static EventUnreliable Wrap(NetworkEvent ev) {
      EventUnreliable r;

      r.NetworkEvent = ev;
      r.Priority = 0;
      r.Skipped = false;

      return r;
    }
  }

  struct EventReliable {
    public NetworkEvent NetworkEvent;
    public uint Sequence;

    public static EventReliable Wrap(NetworkEvent ev) {
      return Wrap(ev, 0);
    }

    public static EventReliable Wrap(Bolt.NetworkEvent ev, uint sequence) {
      EventReliable r;

      r.NetworkEvent = ev;
      r.Sequence = sequence;

      return r;
    }
  }

  class EventChannel : BoltChannel {
    List<EventUnreliable> unreliableSend;
    EventReliableSendBuffer reliableOrderedSend;
    EventReliableRecvBuffer reliableOrderedRecv;

    public EventChannel() {
      unreliableSend = new List<EventUnreliable>(256);
      reliableOrderedSend = new EventReliableSendBuffer(NetworkEvent.RELIABLE_WINDOW_BITS, NetworkEvent.RELIABLE_SEQUENCE_BITS);
      reliableOrderedRecv = new EventReliableRecvBuffer(NetworkEvent.RELIABLE_WINDOW_BITS, NetworkEvent.RELIABLE_SEQUENCE_BITS);
    }

    public void Queue(NetworkEvent ev) {
      if (ev.Reliability == ReliabilityModes.Unreliable) {
        // push on unreliable send queue
        unreliableSend.Add(EventUnreliable.Wrap(ev));

        // incr refs!
        ev.IncrementRefs();
      }
      else {
        if (reliableOrderedSend.TryEnqueue(EventReliable.Wrap(ev))) {
          ev.IncrementRefs();
        }
        else {
          BoltLog.Warn("The reliable-ordered event queue for {0} is full, disconnecting", connection);
          connection.Disconnect();
        }
      }
    }

    public override void Delivered(BoltPacket packet) {
      // set events as delivered
      for (int i = 0; i < packet.eventReliable.Count; ++i) {
        reliableOrderedSend.SetDelivered(packet.eventReliable[i]);
      }

      EventReliable reliable;

      while (reliableOrderedSend.TryRemove(out reliable)) {
        reliable.NetworkEvent.DecrementRefs();
      }

      // clear packet events out 
      packet.eventReliable.Clear();
    }

    public override void Lost(BoltPacket packet) {
      for (int i = 0; i < packet.eventReliable.Count; ++i) {
        reliableOrderedSend.SetSend(packet.eventReliable[i]);
      }

      packet.eventReliable.Clear();
    }

    public override void Pack(BoltPacket packet) {
      int startPos = packet.stream.Position;

      // prune events and calculate priority for remaining ones
      for (int i = 0; i < unreliableSend.Count; ++i) {
        EventUnreliable r = unreliableSend[i];

        if (r.NetworkEvent.IsEntityEvent) {
          var existsOnRemote = connection._entityChannel.ExistsOnRemote(r.NetworkEvent.TargetEntity);
          if (existsOnRemote == false) {
            unreliableSend[i].NetworkEvent.DecrementRefs();
            unreliableSend.RemoveAt(i);

            i -= 1;

            continue;
          }
        }

        r.Priority =
          r.NetworkEvent.IsEntityEvent
            ? r.NetworkEvent.TargetEntity.PriorityCalculator.CalculateEventPriority(connection, r.NetworkEvent)
            : 10;

        unreliableSend[i] = r;
      }

      // sort on priority (descending)
      unreliableSend.Sort(EventUnreliable.PriorityComparer.Instance);

      int maxBits = BoltCore._config.packetMaxEventSize * 8;
      int ptrStart = packet.stream.Ptr;

      // pack reliable events into packet
      EventReliable reliable;

      while (reliableOrderedSend.TryNext(out reliable)) {
        int ptr = packet.stream.Ptr;

        bool packOk = PackEvent(reliable.NetworkEvent, packet.stream, reliable.Sequence);
        bool notOverMaxBits = (packet.stream.Ptr - ptrStart) <= maxBits;
        bool notOverflowing = packet.stream.Overflowing == false;

        if (packOk && notOverMaxBits && notOverflowing) {
          packet.eventReliable.Add(reliable);
        }
        else {
          // reset ptr
          packet.stream.Ptr = ptr;

          // flag for sending
          reliableOrderedSend.SetSend(reliable);
          break;
        }
      }

      // pack unreliable events into packet
      for (int i = 0; i < unreliableSend.Count; ++i) {
        int ptr = packet.stream.Ptr;

        bool packOk = PackEvent(unreliableSend[i].NetworkEvent, packet.stream, 0);
        bool notOverMaxBits = (packet.stream.Ptr - ptrStart) <= maxBits;
        bool notOverflowing = packet.stream.Overflowing == false;

        if (packOk && notOverMaxBits && notOverflowing) {
          unreliableSend[i].NetworkEvent.DecrementRefs();
          unreliableSend.RemoveAt(i);
        }
        else {
          // reset ptr
          packet.stream.Ptr = ptr;
        }
      }

      packet.stream.WriteStopMarker();

      // prune entities which have been skipped twice
      for (int i = 0; i < unreliableSend.Count; ++i) {
        EventUnreliable r = unreliableSend[i];

        if (r.Skipped) {
          unreliableSend.RemoveAt(i);
        }
        else {
          // this will be pruned next time if it's not sent
          r.Skipped = true;

          unreliableSend[i] = r;
        }
      }

      packet.stats.EventBits = packet.stream.Position - startPos;
    }

    bool PackEvent(NetworkEvent ev, UdpPacket stream, uint sequence) {
      BoltLog.Debug("sending event {0}", ev);

      stream.WriteContinueMarker();

      // type id of this event
      stream.WriteTypeId(ev.Meta.TypeId);

      // targets of this event
      stream.WriteInt(ev.Targets, 5);

      if (stream.WriteBool(ev.Reliability == ReliabilityModes.ReliableOrdered)) {
        // write sequence number for reliable events
        stream.WriteUInt(sequence, NetworkEvent.RELIABLE_SEQUENCE_BITS);
      }
      else {
        if (ev.IsEntityEvent) {
          // write network id for entity events
          stream.WriteEntity(ev.TargetEntity);
        }
      }

      stream.WriteByteArraySimple(ev.BinaryData, BoltCore._config.packetSize / 2);
      return ev.Pack(connection, stream);
    }

    public override void Read(BoltPacket packet) {
      int startPtr = packet.stream.Position;

      while (packet.stream.ReadStopMarker()) {
        uint sequence = 0;
        NetworkEvent ev = ReadEvent(packet.stream, ref sequence);

        BoltLog.Debug("recv event {0}", ev);
        if (ev.Reliability == ReliabilityModes.Unreliable) {
          EventDispatcher.Received(ev);
        }
        else {
          switch (reliableOrderedRecv.TryEnqueue(EventReliable.Wrap(ev, sequence))) {
            case RecvBufferAddResult.Old:
            case RecvBufferAddResult.OutOfBounds:
            case RecvBufferAddResult.AlreadyExists:
              BoltLog.Debug("FAILED");
              ev.DecrementRefs();
              break;
          }
        }
      }

      EventReliable reliable;

      while (reliableOrderedRecv.TryRemove(out reliable)) {
        EventDispatcher.Received(reliable.NetworkEvent);
      }

      packet.stats.EventBits = packet.stream.Position - startPtr;
    }

    NetworkEvent ReadEvent(UdpPacket stream, ref uint sequence) {
      NetworkEvent ev;

      ev = Factory.NewEvent(stream.ReadTypeId());
      ev.Targets = stream.ReadInt(5);
      ev.SourceConnection = connection;

      if (stream.ReadBool()) {
        sequence = stream.ReadUInt(NetworkEvent.RELIABLE_SEQUENCE_BITS);

        // assign relability mode
        ev.Reliability = ReliabilityModes.ReliableOrdered;
      }
      else {
        if (ev.IsEntityEvent) {
          ev.TargetEntity = stream.ReadEntity();
        }

        // assign relability mode
        ev.Reliability = ReliabilityModes.Unreliable;
      }

      ev.BinaryData = stream.ReadByteArraySimple();
      ev.Read(connection, stream);
      return ev;
    }
  }
}

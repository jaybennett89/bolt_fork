using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public Event Event;
    public float Priority;

    public static EventUnreliable Wrap(Event ev) {
      EventUnreliable r;

      r.Event = ev;
      r.Priority = 0;
      r.Skipped = false;

      return r;
    }
  }

  struct EventReliable {
    public Event Event;
    public uint Sequence;

    public static EventReliable Wrap(Event ev) {
      return Wrap(ev, 0);
    }

    public static EventReliable Wrap(Bolt.Event ev, uint sequence) {
      EventReliable r;

      r.Event = ev;
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
      reliableOrderedSend = new EventReliableSendBuffer(Event.RELIABLE_WINDOW_BITS, Event.RELIABLE_SEQUENCE_BITS);
      reliableOrderedRecv = new EventReliableRecvBuffer(Event.RELIABLE_WINDOW_BITS, Event.RELIABLE_SEQUENCE_BITS);
    }

    public void Queue(Event ev) {
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
        reliable.Event.DecrementRefs();
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

        if (r.Event.IsEntityEvent) {
          var existsOnRemote = connection._entityChannel.ExistsOnRemote(r.Event.TargetEntity);
          if (existsOnRemote == false) {
            unreliableSend[i].Event.DecrementRefs();
            unreliableSend.RemoveAt(i);

            i -= 1;

            continue;
          }
        }

        r.Priority =
          r.Event.IsEntityEvent
            ? r.Event.TargetEntity.PriorityCalculator.CalculateEventPriority(connection, r.Event)
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

        bool packOk = PackEvent(reliable.Event, packet.stream, reliable.Sequence);
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

        bool packOk = PackEvent(unreliableSend[i].Event, packet.stream, 0);
        bool notOverMaxBits = (packet.stream.Ptr - ptrStart) <= maxBits;
        bool notOverflowing = packet.stream.Overflowing == false;

        if (packOk && notOverMaxBits && notOverflowing) {
          unreliableSend[i].Event.DecrementRefs();
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

    bool PackEvent(Event ev, UdpStream stream, uint sequence) {
      stream.WriteContinueMarker();

      // type id of this event
      ev.Meta.TypeId.Pack(stream);

      // targets of this event
      stream.WriteInt(ev.Targets, 5);

      if (stream.WriteBool(ev.Reliability == ReliabilityModes.ReliableOrdered)) {
        // write sequence number for reliable events
        stream.WriteUInt(sequence, Event.RELIABLE_SEQUENCE_BITS);
      }
      else {
        if (ev.IsEntityEvent) {
          // write network id for entity events
          stream.WriteEntity(ev.TargetEntity, connection);
        }
      }

      return ev.Pack(connection, stream);
    }

    public override void Read(BoltPacket packet) {
      int startPtr = packet.stream.Position;

      while (packet.stream.ReadStopMarker()) {
        uint sequence = 0;
        Event ev = ReadEvent(packet.stream, ref sequence);

        if (ev.Reliability == ReliabilityModes.Unreliable) {
          EventDispatcher.Received(ev);
        }
        else {
          switch (reliableOrderedRecv.TryEnqueue(EventReliable.Wrap(ev, sequence))) {
            case RecvBufferAddResult.Old:
            case RecvBufferAddResult.OutOfBounds:
            case RecvBufferAddResult.AlreadyExists:
              ev.DecrementRefs();
              break;
          }
        }
      }

      EventReliable reliable;

      while (reliableOrderedRecv.TryRemove(out reliable)) {
        EventDispatcher.Received(reliable.Event);
      }

      packet.stats.EventBits = packet.stream.Position - startPtr;
    }

    Event ReadEvent(UdpStream stream, ref uint sequence) {
      Event ev;

      ev = Factory.NewEvent(TypeId.Read(stream));
      ev.Targets = stream.ReadInt(5);
      ev.SourceConnection = connection;

      if (stream.ReadBool()) {
        sequence = stream.ReadUInt(Event.RELIABLE_SEQUENCE_BITS);

        // assign relability mode
        ev.Reliability = ReliabilityModes.ReliableOrdered;
      }
      else {
        if (ev.IsEntityEvent) {
          ev.TargetEntity = stream.ReadEntity(connection);
        }

        // assign relability mode
        ev.Reliability = ReliabilityModes.Unreliable;
      }

      ev.Read(connection, stream);
      return ev;
    }
  }
}

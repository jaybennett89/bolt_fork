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
    EventReliableSendBuffer reliableSend;
    EventReliableRecvBuffer reliableRecv;

    public EventChannel() {
      unreliableSend = new List<EventUnreliable>(256);
      reliableSend = new EventReliableSendBuffer(Event.RELIABLE_WINDOW_BITS, Event.RELIABLE_SEQUENCE_BITS);
      reliableRecv = new EventReliableRecvBuffer(Event.RELIABLE_WINDOW_BITS, Event.RELIABLE_SEQUENCE_BITS);
    }

    public void Queue(Event ev) {
      if (ev.IsEntityEvent) {
        // push on unreliable send queue
        unreliableSend.Add(EventUnreliable.Wrap(ev));

        // incr refs!
        ev.IncrementRefs();
      }
      else {
        if (reliableSend.TryEnqueue(EventReliable.Wrap(ev))) {
          ev.IncrementRefs();
        }
        else {
          BoltLog.Warn("The reliable event queue for {0} is full, disconnecting", connection);
          connection.Disconnect();
        }
      }
    }

    public override void Delivered(BoltPacket packet) {
      // set events as delivered
      for (int i = 0; i < packet.eventReliable.Count; ++i) {
        reliableSend.SetDelivered(packet.eventReliable[i]);
      }

      EventReliable reliable;

      while (reliableSend.TryRemove(out reliable)) {
        reliable.Event.DecrementRefs();
      }

      // clear packet events out 
      packet.eventReliable.Clear();
    }

    public override void Lost(BoltPacket packet) {
      for (int i = 0; i < packet.eventReliable.Count; ++i) {
        reliableSend.SetSend(packet.eventReliable[i]);
      }

      packet.eventReliable.Clear();
    }

    public override void Pack(BoltPacket packet) {
      // prune events and calculate priority for remaining ones
      for (int i = 0; i < unreliableSend.Count; ++i) {
        var existsOnRemote = connection._entityChannel.ExistsOnRemote(unreliableSend[i].Event.TargetEntity);
        if (existsOnRemote) {
          EventUnreliable r;

          r = unreliableSend[i];
          r.Priority = unreliableSend[i].Event.TargetEntity.PriorityCalculator.CalculateEventPriority(connection, r.Event);

          unreliableSend[i] = r;
        }
        else {
          unreliableSend[i].Event.DecrementRefs();
          unreliableSend.RemoveAt(i);
        }
      }

      // sort on priority (descending)
      unreliableSend.Sort(EventUnreliable.PriorityComparer.Instance);

      int maxBits = BoltCore._config.packetMaxEventSize * 8;
      int ptrStart = packet.stream.Ptr;

      // pack reliable events into packet
      EventReliable reliable;

      while (reliableSend.TryNext(out reliable)) {
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
          reliableSend.SetSend(reliable);
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
    }

    bool PackEvent(Event ev, UdpStream stream, uint sequence) {
      stream.WriteContinueMarker();

      // type id of this event
      ev.Meta.TypeId.Pack(stream);

      // targets of this event
      stream.WriteInt(ev.Targets, 4);

      if (ev.IsEntityEvent) {
        // write network id for entity events
        stream.WriteEntity(ev.TargetEntity, connection);
      }
      else {
        // write sequence number for global events
        stream.WriteUInt(sequence, Event.RELIABLE_SEQUENCE_BITS);
      }

      return ev.Pack(connection, stream);
    }

    public override void Read(BoltPacket packet) {
      while (packet.stream.ReadStopMarker()) {
        uint sequence = 0;
        Event ev = ReadEvent(packet.stream, ref sequence);

        if (ev.IsEntityEvent) {
          EventDispatcher.Enqueue(ev);
        }
        else {
          switch (reliableRecv.TryEnqueue(EventReliable.Wrap(ev, sequence))) {
            case RecvBufferAddResult.Old:
            case RecvBufferAddResult.OutOfBounds:
            case RecvBufferAddResult.AlreadyExists:
              ev.DecrementRefs();
              break;
          }
        }
      }

      EventReliable reliable;

      while (reliableRecv.TryRemove(out reliable)) {
        EventDispatcher.Enqueue(reliable.Event);
      }
    }

    Event ReadEvent(UdpStream stream, ref uint sequence) {
      Event ev;

      ev = Factory.NewEvent(TypeId.Read(stream));
      ev.Targets = stream.ReadInt(4);
      ev.SourceConnection = connection;

      if (ev.IsEntityEvent) {
        ev.TargetEntity = stream.ReadEntity(connection);
      }
      else {
        sequence = stream.ReadUInt(Event.RELIABLE_SEQUENCE_BITS);
      }

      ev.Read(connection, stream);
      return ev;
    }
  }
}

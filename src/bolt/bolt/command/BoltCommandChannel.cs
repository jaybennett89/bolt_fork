using Bolt;
using System.Collections.Generic;
using System.Linq;
using UdpKit;
using UnityEngine;

partial class BoltEntityChannel {
  public class CommandChannel : BoltChannel {

    #region sequence

    static void WriteSequence(UdpStream stream, ushort sequence) {
      stream.WriteUShort(sequence, EntityObject.COMMAND_SEQ_BITS);
    }

    static ushort ReadSequence(UdpStream stream) {
      return stream.ReadUShort(EntityObject.COMMAND_SEQ_BITS);
    }

    #endregion

    int pingFrames {
      get { return Mathf.CeilToInt((connection.udpConnection.AliasedPing * BoltCore._config.commandPingMultiplier) / BoltCore.frameDeltaTime); }
    }

    Dictionary<InstanceId, EntityProxy> incommingProxiesByEntityId {
      get { return connection._entityChannel._incommingProxiesByInstanceId; }
    }

    Dictionary<NetId, EntityProxy> incommingProxiesByNetworkId {
      get { return connection._entityChannel._incommingProxiesByNetId; }
    }

    Dictionary<InstanceId, EntityProxy> outgoingProxiesByEntityId {
      get { return connection._entityChannel._outgoingProxiesByInstanceId; }
    }

    Dictionary<NetId, EntityProxy> outgoingProxiesByNetworkId {
      get { return connection._entityChannel._outgoingProxiesByNetId; }
    }

    public CommandChannel() {
    }

    public override void Pack(BoltPacket packet) {
      int pos = packet.stream.Position;

      PackStates(packet);
      PackCommands(packet);

      packet.info.commandBits = packet.stream.Position - pos;
    }

    public override void Read(BoltPacket packet) {
      ReadStates(packet);
      ReadCommands(packet);
    }


    bool EntityHasUnsentState(EntityObject entity) {
      BoltCommand cmd = null;
      var cmdIter = entity.CommandQueue.GetIterator();

      while (cmdIter.Next(out cmd)) {
        if (cmd._hasExecuted == true && cmd._stateSent == false) {
          return true;
        }
      }

      return false;
    }

    void PackStates(BoltPacket packet) {
      foreach (EntityProxy proxy in outgoingProxiesByEntityId.Values) {
        EntityObject entity = proxy.Entity;

        if ((entity != null) && ReferenceEquals(entity.Controller, connection) && EntityHasUnsentState(entity)) {
          int proxyPos = packet.stream.Position;

          packet.stream.WriteBool(true);
          packet.stream.WriteNetworkId(proxy.NetId);

          //packet.stream.WriteInt(!entity._origin ? 0 : BoltCore.resolveTransformId(entity._origin));

          BoltCommand cmd = null;
          var cmdIter = entity.CommandQueue.GetIterator();

          while (cmdIter.Next(out cmd)) {
            if (cmd._hasExecuted && cmd._stateSent == false) {
              int cmdPos = packet.stream.Position;
              packet.stream.WriteBool(true);
              packet.stream.WriteUShort(cmd._id);
              WriteSequence(packet.stream, cmd._sequence);
              cmd.PackState(connection, packet.stream);

              if (packet.stream.Overflowing) {
                packet.stream.Position = cmdPos;
                break;
              }
              else {
                cmd._stateSent = true;
              }
            }
          }

          if (packet.stream.Overflowing) {
            packet.stream.Position = proxyPos;
            break;

          }
          else {
            // stop marker for states
            packet.stream.WriteStopMarker();
          }

          // dipose commands we dont need anymore
          while (entity.CommandQueue.count > 1 && entity.CommandQueue.first._hasExecuted && entity.CommandQueue.first._stateSent) {
            entity.CommandQueue.RemoveFirst().Dispose();
          }
        }
      }

      // stop marker for proxies
      packet.stream.WriteStopMarker();
    }

    void ReadStates(BoltPacket packet) {
      while (packet.stream.CanRead()) {
        if (packet.stream.ReadBool() == false) { break; }

        NetId networkId = packet.stream.ReadNetworkId();
        EntityProxy proxy = incommingProxiesByNetworkId[networkId];
        EntityObject entity = proxy.Entity;


        while (packet.stream.CanRead()) {
          if (packet.stream.ReadBool() == false) { break; }

          ushort commandId = packet.stream.ReadUShort();
          ushort sequence = ReadSequence(packet.stream);

          BoltCommand cmd = null;

          if (entity != null) {
            BoltCommand c = null;
            var cIter = entity.CommandQueue.GetIterator();

            while (cIter.Next(out c)) {
              int dist = UdpMath.SeqDistance(c._sequence, sequence, EntityObject.COMMAND_SEQ_SHIFT);

              if (dist > 0) { break; }
              if (dist < 0) { c._dispose = true; }
              if (dist == 0) {
                cmd = c;
                break;
              }
            }
          }

          if (cmd) {
            cmd.ReadState(connection, packet.stream);
            cmd._stateRecv = true;
            ////BoltLog.Debug("command for frame {0} got state, queue size: {1}", cmd._frame, proxy.commands.count);

          }
          else {
            cmd = BoltFactory.NewCommand(commandId);
            cmd._sequence = sequence;
            cmd.ReadState(connection, packet.stream);
            cmd.Dispose();
          }
        }

        // remove all disposable commands

        if (entity != null) {
          while (entity.CommandQueue.count > 1 && entity.CommandQueue.first._dispose) {
            entity.CommandQueue.RemoveFirst().Dispose();
          }
        }
      }
    }

    void PackCommands(BoltPacket packet) {
      foreach (EntityProxy proxy in incommingProxiesByNetworkId.Values) {
        EntityObject entity = proxy.Entity;

        ////BoltLog.Debug("count: {0}", incommingProxiesByEntityId.Count);
        ////BoltLog.Debug("packing cmd for {0}: {1}/{2}/{3}", entity, (bool) (entity), (bool) (entity._flags & BoltEntity.FLAG_IS_CONTROLLING), (bool) (entity._commands.count > 0));

        if (entity && entity.HasControl && (entity.CommandQueue.count > 0)) {
          ////BoltLog.Debug("packing cmd for {0} #2", entity);

          int proxyPos = packet.stream.Position;
          packet.stream.WriteBool(true);
          packet.stream.WriteNetworkId(proxy.NetId);

          BoltCommand cmd = entity.CommandQueue.last;

          // how many commands we should send at most
          int redundancy = Mathf.Min(entity.CommandQueue.count, BoltCore._config.commandRedundancy);

          // go to first command
          for (int i = 0; i < (redundancy - 1); ++i) {
            cmd = entity.CommandQueue.Prev(cmd);
          }

          // write all commands into the packet
          for (int i = 0; i < redundancy; ++i) {
            ////BoltLog.Debug("PACK | cmd._frame: {0}, Network._frame: {1}", cmd._frame, BoltNetworkCore._frame);

            int cmdPos = packet.stream.Position;

            packet.stream.WriteBool(true);
            packet.stream.WriteUShort(cmd._id);
            WriteSequence(packet.stream, cmd._sequence);
            packet.stream.WriteInt(cmd._serverFrame);

            ////BoltLog.Debug("packing cmd sequence {0}", cmd._sequence);

            cmd.PackInput(connection, packet.stream);
            cmd = entity.CommandQueue.Next(cmd);

            if (packet.stream.Overflowing) {
              packet.stream.Position = cmdPos;
              break;
            }
          }

          // overflowing, reset before this proxy and break
          if (packet.stream.Overflowing) {
            packet.stream.Position = proxyPos;
            break;
          }
          else {
            // stop marker for commands
            packet.stream.WriteStopMarker();
          }
        }
      }

      // stop marker for proxies
      packet.stream.WriteStopMarker();
    }

    void ReadCommands(BoltPacket packet) {
      int maxFrame = BoltCore._frame;
      int minFrame = maxFrame - (BoltCore._config.commandDelayAllowed + pingFrames);

      while (packet.stream.CanRead()) {
        if (packet.stream.ReadBool() == false) { break; }

        NetId netId = packet.stream.ReadNetworkId();
        EntityProxy proxy = outgoingProxiesByNetworkId[netId];

        while (packet.stream.CanRead()) {
          if (packet.stream.ReadBool() == false) { break; }

          BoltCommand cmd = BoltFactory.NewCommand(packet.stream.ReadUShort());
          cmd._sequence = ReadSequence(packet.stream);
          cmd._serverFrame = packet.stream.ReadInt();
          cmd.ReadInput(connection, packet.stream);

          // no proxy or entity
          if (!proxy || !proxy.Entity) { continue; }

          EntityObject entity = proxy.Entity;

          // remote is not controller
          if (ReferenceEquals(entity.Controller, connection) == false) { continue; }

          // sequence is old
          if (UdpMath.SeqDistance(cmd._sequence, entity.CommandSequence, EntityObject.COMMAND_SEQ_SHIFT) <= 0) { continue; }

          // put on command queue
          entity.CommandQueue.AddLast(cmd);
          entity.CommandSequence = cmd._sequence;
        }
      }
    }
  }
}

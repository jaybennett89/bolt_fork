using Bolt;
using System.Collections.Generic;
using System.Linq;
using UdpKit;
using UnityEngine;

partial class BoltEntityChannel {
  public class CommandChannel : BoltChannel {

    #region sequence

    static void WriteSequence(UdpStream stream, ushort sequence) {
      stream.WriteUShort(sequence, Command.SEQ_BITS);
    }

    static ushort ReadSequence(UdpStream stream) {
      return stream.ReadUShort(Command.SEQ_BITS);
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

      PackResult(packet);
      PackInput(packet);

      packet.info.commandBits = packet.stream.Position - pos;
    }

    public override void Read(BoltPacket packet) {
      ReadResult(packet);
      ReadInput(packet);
    }


    bool EntityHasUnsentState(Entity entity) {
      var it = entity.CommandQueue.GetIterator();

      while (it.Next()) {
        if (it.val.Flags & CommandFlags.SEND_STATE) {
          return true;
        }
      }

      return false;
    }

    void PackResult(BoltPacket packet) {
      foreach (EntityProxy proxy in outgoingProxiesByEntityId.Values) {
        Entity entity = proxy.Entity;

        // four conditions have to hold
        // 1) Entity must exist locally (not null)
        // 2) The connection must be the controller
        // 3) The entity must exist remotely
        // 4) The entity has to have unsent states

        if ((entity != null) && ReferenceEquals(entity.Controller, connection) && connection._entityChannel.ExistsOnRemote(entity) && EntityHasUnsentState(entity)) {
          Assert.True(entity.IsOwner);

          int proxyPos = packet.stream.Position;
          int cmdWriteCount = 0;

          packet.stream.WriteBool(true);
          packet.stream.WriteNetworkId(proxy.NetId);

          var it = entity.CommandQueue.GetIterator();

          while (it.Next()) {
            if (it.val.Flags & CommandFlags.HAS_EXECUTED) {
              if (it.val.Flags & CommandFlags.SEND_STATE) {
                int cmdPos = packet.stream.Position;

                packet.stream.WriteBool(true);

                it.val.Meta.TypeId.Pack(packet.stream);

                WriteSequence(packet.stream, it.val.Sequence);

                it.val.PackResult(connection, packet.stream);

                if (packet.stream.Overflowing) {
                  packet.stream.Position = cmdPos;
                  break;
                }
                else {
                  cmdWriteCount += 1;

                  it.val.Flags &= ~CommandFlags.SEND_STATE;
                  it.val.Flags |= CommandFlags.SEND_STATE_PERFORMED;
                }
              }
            }
          }

          // we wrote too much or nothing at all
          if (packet.stream.Overflowing || (cmdWriteCount == 0)) {
            packet.stream.Position = proxyPos;
            break;
          }
          else {
            // stop marker for states
            packet.stream.WriteStopMarker();
          }

          // dipose commands we dont need anymore
          while ((entity.CommandQueue.count > 1) && (entity.CommandQueue.first.Flags & CommandFlags.SEND_STATE_PERFORMED)) {
            entity.CommandQueue.RemoveFirst().Free();
          }
        }
      }

      // stop marker for proxies
      packet.stream.WriteStopMarker();
    }

    void ReadResult(BoltPacket packet) {
      while (packet.stream.CanRead()) {
        if (packet.stream.ReadBool() == false) { break; }

        NetId netId = packet.stream.ReadNetworkId();
        EntityProxy proxy = incommingProxiesByNetworkId[netId];
        Entity entity = proxy.Entity;

        while (packet.stream.CanRead()) {
          if (packet.stream.ReadBool() == false) { break; }

          TypeId typeId = TypeId.Read(packet.stream);
          ushort sequence = ReadSequence(packet.stream);

          Command cmd = null;

          if (entity != null) {
            var it = entity.CommandQueue.GetIterator();

            while (it.Next()) {
              int dist = UdpMath.SeqDistance(it.val.Sequence, sequence, Command.SEQ_SHIFT);
              if (dist > 0) { break; }
              if (dist < 0) { it.val.Flags |= CommandFlags.DISPOSE; }
              if (dist == 0) {
                cmd = it.val;
                break;
              }
            }
          }

          if (cmd) {
            if (cmd.Meta.SmoothFrames > 0) {
              cmd.SmoothTo = cmd.ResultData.CloneArray();
              cmd.SmoothFrom = cmd.ResultData.CloneArray();

              cmd.SmoothStart = BoltCore.frame;
              cmd.SmoothEnd = cmd.SmoothStart + cmd.Meta.SmoothFrames;

              cmd.ReadResult(connection, cmd.SmoothTo, packet.stream);
            }
            else {
              cmd.ReadResult(connection, cmd.ResultData, packet.stream);
            }

            cmd.Flags |= CommandFlags.CORRECTION_RECEIVED;
          }
          else {
            cmd = Factory.NewCommand(typeId);
            cmd.ReadResult(connection, cmd.ResultData, packet.stream);
            cmd.Free();
          }
        }

        // remove all disposable commands
        if (entity != null) {
          while ((entity.CommandQueue.count > 1) && (entity.CommandQueue.first.Flags & CommandFlags.DISPOSE)) {
            entity.CommandQueue.RemoveFirst().Free();
          }
        }
      }
    }

    void PackInput(BoltPacket packet) {
      foreach (EntityProxy proxy in incommingProxiesByNetworkId.Values) {
        Entity entity = proxy.Entity;

        ////BoltLog.Debug("count: {0}", incommingProxiesByEntityId.Count);
        ////BoltLog.Debug("packing cmd for {0}: {1}/{2}/{3}", entity, (bool) (entity), (bool) (entity._flags & BoltEntity.FLAG_IS_CONTROLLING), (bool) (entity._commands.count > 0));

        if (entity && entity.HasControl && (entity.CommandQueue.count > 0)) {
          ////BoltLog.Debug("packing cmd for {0} #2", entity);

          int proxyPos = packet.stream.Position;
          packet.stream.WriteBool(true);
          packet.stream.WriteNetworkId(proxy.NetId);

          Command cmd = entity.CommandQueue.last;

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

            cmd.Meta.TypeId.Pack(packet.stream);

            WriteSequence(packet.stream, cmd.Sequence);

            packet.stream.WriteInt(cmd.ServerFrame);

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

    void ReadInput(BoltPacket packet) {
      int maxFrame = BoltCore._frame;
      int minFrame = maxFrame - (BoltCore._config.commandDelayAllowed + pingFrames);

      while (packet.stream.CanRead()) {
        if (packet.stream.ReadBool() == false) { break; }

        NetId netId = packet.stream.ReadNetworkId();
        EntityProxy proxy = outgoingProxiesByNetworkId[netId];

        while (packet.stream.CanRead()) {
          if (packet.stream.ReadBool() == false) { break; }

          Bolt.Command cmd = Factory.NewCommand(TypeId.Read(packet.stream));
          cmd.Sequence = ReadSequence(packet.stream);
          cmd.Frame = packet.stream.ReadInt();
          cmd.ReadInput(connection, packet.stream);

          // no proxy or entity
          if (!proxy || !proxy.Entity) { continue; }

          Entity entity = proxy.Entity;

          // remote is not controller
          if (ReferenceEquals(entity.Controller, connection) == false) { continue; }

          // sequence is old
          if (UdpMath.SeqDistance(cmd.Sequence, entity.CommandSequence, Command.SEQ_SHIFT) <= 0) { continue; }

          // put on command queue
          entity.CommandQueue.AddLast(cmd);
          entity.CommandSequence = cmd.Sequence;
        }
      }
    }
  }

}

using Bolt;
using System.Collections.Generic;
using System.Linq;
using UdpKit;
using UnityEngine;

partial class EntityChannel {
  public class CommandChannel : BoltChannel {

    int pingFrames {
      get { return Mathf.CeilToInt((connection.udpConnection.AliasedPing * BoltCore._config.commandPingMultiplier) / BoltCore.frameDeltaTime); }
    }

    Dictionary<NetworkId, EntityProxy> incommingProxiesByNetworkId {
      get { return connection._entityChannel._incomming; }
    }

    Dictionary<NetworkId, EntityProxy> outgoingProxiesByNetworkId {
      get { return connection._entityChannel._outgoing; }
    }

    public CommandChannel() {
    }

    public override void Pack(BoltPacket packet) {
      int pos = packet.stream.Position;

      PackResult(packet);
      PackInput(packet);

      packet.stats.CommandBits = packet.stream.Position - pos;
    }

    public override void Read(BoltPacket packet) {
      int startPtr = packet.stream.Position;

      ReadResult(packet);
      ReadInput(packet);

      packet.stats.CommandBits = packet.stream.Position - startPtr;
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
      foreach (EntityProxy proxy in outgoingProxiesByNetworkId.Values) {
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
          packet.stream.WriteNetworkId(proxy.NetworkId);

          var it = entity.CommandQueue.GetIterator();

          while (it.Next()) {
            if (it.val.Flags & CommandFlags.HAS_EXECUTED) {
              if (it.val.Flags & CommandFlags.SEND_STATE) {
                int cmdPos = packet.stream.Position;

                packet.stream.WriteBool(true);
                packet.stream.WriteTypeId(it.val.ResultObject.Meta.TypeId);
                packet.stream.WriteUShort(it.val.Sequence, Command.SEQ_BITS);
                packet.stream.WriteToken(it.val.ResultObject.Token);

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

        NetworkId netId = packet.stream.ReadNetworkId();
        EntityProxy proxy = incommingProxiesByNetworkId[netId];
        Entity entity = proxy.Entity;

        while (packet.stream.CanRead()) {
          if (packet.stream.ReadBool() == false) { break; }

          TypeId typeId = packet.stream.ReadTypeId();
          ushort sequence = packet.stream.ReadUShort(Command.SEQ_BITS);
          IProtocolToken resultToken = packet.stream.ReadToken();

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
            cmd.ResultObject.Token = resultToken;
            cmd.Flags |= CommandFlags.CORRECTION_RECEIVED;

            if (cmd.Meta.SmoothFrames > 0) {
              cmd.BeginSmoothing();
            }

            cmd.ReadResult(connection, packet.stream);
          }
          else {
            cmd = Factory.NewCommand(typeId);
            cmd.ReadResult(connection, packet.stream);
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

        if (entity && entity.HasControl && (entity.CommandQueue.count > 0)) {
          int proxyPos = packet.stream.Position;
          packet.stream.WriteContinueMarker();
          packet.stream.WriteNetworkId(proxy.NetworkId);

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

            packet.stream.WriteContinueMarker();
            packet.stream.WriteTypeId(cmd.Meta.TypeId);
            packet.stream.WriteUShort(cmd.Sequence, Command.SEQ_BITS);
            packet.stream.WriteInt(cmd.ServerFrame);
            packet.stream.WriteToken(cmd.InputObject.Token);

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

      while (packet.stream.ReadStopMarker()) {
        NetworkId netId = packet.stream.ReadNetworkId();
        EntityProxy proxy = null;

        if (outgoingProxiesByNetworkId.ContainsKey(netId)) {
          proxy = outgoingProxiesByNetworkId[netId];
        }

        while (packet.stream.ReadStopMarker()) {
          Bolt.Command cmd = Factory.NewCommand(packet.stream.ReadTypeId());
          cmd.Sequence = packet.stream.ReadUShort(Command.SEQ_BITS);
          cmd.ServerFrame = packet.stream.ReadInt();
          cmd.InputObject.Token = packet.stream.ReadToken();
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

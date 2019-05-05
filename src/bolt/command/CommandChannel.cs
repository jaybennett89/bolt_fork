using Bolt;
using System.Collections.Generic;
using System.Linq;
using UdpKit;
using UnityEngine;

partial class EntityChannel {
  internal class CommandChannel : BoltChannel {

    int pingFrames {
      get { return Mathf.CeilToInt((connection.udpConnection.AliasedPing * BoltCore._config.commandPingMultiplier) / BoltCore.frameDeltaTime); }
    }

    Dictionary<NetworkId, EntityProxy> incommingProxiesByNetworkId {
      get { return connection._entityChannel._incommingDict; }
    }

    Dictionary<NetworkId, EntityProxy> outgoingProxiesByNetworkId {
      get { return connection._entityChannel._outgoingDict; }
    }

    public CommandChannel() {
    }

    public override void Pack(Packet packet) {
      int pos = packet.UdpPacket.Position;

      PackResult(packet);
      PackInput(packet);

      packet.Stats.CommandBits = packet.UdpPacket.Position - pos;
    }

    public override void Read(Packet packet) {
      int startPtr = packet.UdpPacket.Position;

      ReadResult(packet);
      ReadInput(packet);

      packet.Stats.CommandBits = packet.UdpPacket.Position - startPtr;
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

    void PackResult(Packet packet) {
      foreach (EntityProxy proxy in outgoingProxiesByNetworkId.Values) {
        Entity entity = proxy.Entity;

        // four conditions have to hold
        // 1) Entity must exist locally (not null)
        // 2) The connection must be the controller
        // 3) The entity must exist remotely
        // 4) The entity has to have unsent states

        if ((entity != null) && ReferenceEquals(entity.Controller, connection) && connection._entityChannel.ExistsOnRemote(entity) && EntityHasUnsentState(entity)) {
          Assert.True(entity.IsOwner);

          int proxyPos = packet.UdpPacket.Position;
          int cmdWriteCount = 0;

          packet.UdpPacket.WriteBool(true);
          packet.UdpPacket.WriteNetworkId(proxy.NetworkId);
          packet.UdpPacket.WriteEntity(proxy.Entity.Parent);

          var it = entity.CommandQueue.GetIterator();

          while (it.Next()) {
            if (it.val.Flags & CommandFlags.HAS_EXECUTED) {
              if (it.val.Flags & CommandFlags.SEND_STATE) {
                int cmdPos = packet.UdpPacket.Position;

                packet.UdpPacket.WriteBool(true);
                packet.UdpPacket.WriteTypeId(it.val.ResultObject.Meta.TypeId);
                packet.UdpPacket.WriteUShort(it.val.Sequence, Command.SEQ_BITS);
                packet.UdpPacket.WriteToken(it.val.ResultObject.Token);

                it.val.PackResult(connection, packet.UdpPacket);

                if (packet.UdpPacket.Overflowing) {
                  packet.UdpPacket.Position = cmdPos;
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
          if (packet.UdpPacket.Overflowing || (cmdWriteCount == 0)) {
            packet.UdpPacket.Position = proxyPos;
            break;
          }
          else {
            // stop marker for states
            packet.UdpPacket.WriteStopMarker();
          }

          // dipose commands we dont need anymore
          while ((entity.CommandQueue.count > 1) && (entity.CommandQueue.first.Flags & CommandFlags.SEND_STATE_PERFORMED)) {
            entity.CommandQueue.RemoveFirst().Free();
          }
        }
      }

      // stop marker for proxies
      packet.UdpPacket.WriteStopMarker();
    }

    void ReadResult(Packet packet) {
      while (packet.UdpPacket.CanRead()) {
        if (packet.UdpPacket.ReadBool() == false) { break; }

        NetworkId netId = packet.UdpPacket.ReadNetworkId();
        Entity parent = packet.UdpPacket.ReadEntity();
        EntityProxy proxy = incommingProxiesByNetworkId[netId];
        Entity entity = proxy.Entity;

        while (packet.UdpPacket.CanRead()) {
          if (packet.UdpPacket.ReadBool() == false) { break; }

          TypeId typeId = packet.UdpPacket.ReadTypeId();
          ushort sequence = packet.UdpPacket.ReadUShort(Command.SEQ_BITS);
          IProtocolToken resultToken = packet.UdpPacket.ReadToken();

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

          entity.SetParentInternal(parent);

          if (cmd) {
            cmd.ResultObject.Token = resultToken;
            cmd.Flags |= CommandFlags.CORRECTION_RECEIVED;

            if (cmd.Meta.SmoothFrames > 0) {
              cmd.BeginSmoothing();
            }

            cmd.ReadResult(connection, packet.UdpPacket);
          }
          else {
            cmd = Factory.NewCommand(typeId);
            cmd.ReadResult(connection, packet.UdpPacket);
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

    void PackInput(Packet packet) {
      foreach (EntityProxy proxy in incommingProxiesByNetworkId.Values) {
        Entity entity = proxy.Entity;

        if (entity && entity.HasControl && (entity.CommandQueue.count > 0)) {
          int proxyPos = packet.UdpPacket.Position;
          packet.UdpPacket.WriteContinueMarker();
          packet.UdpPacket.WriteNetworkId(proxy.NetworkId);

          var redundancy = Mathf.Min(entity.CommandQueue.count, BoltCore._config.commandRedundancy);

          // if we are sending the entire command queue, then make sure we're not sending a command we already received a correction for
          if ((entity.CommandQueue.count == redundancy) && (entity.CommandQueue.first.Flags & Bolt.CommandFlags.CORRECTION_RECEIVED)) {
            redundancy -= 1;
          }

          var cmd = entity.CommandQueue.last;

          // go to first command
          for (int i = 0; i < (redundancy - 1); ++i) {
            cmd = entity.CommandQueue.Prev(cmd);
          }

          // write all commands into the packet
          for (int i = 0; i < redundancy; ++i) {
            ////BoltLog.Debug("PACK | cmd._frame: {0}, Network._frame: {1}", cmd._frame, BoltNetworkCore._frame);

            int cmdPos = packet.UdpPacket.Position;

            packet.UdpPacket.WriteContinueMarker();
            packet.UdpPacket.WriteTypeId(cmd.Meta.TypeId);
            packet.UdpPacket.WriteUShort(cmd.Sequence, Command.SEQ_BITS);
            packet.UdpPacket.WriteIntVB(cmd.ServerFrame);
            packet.UdpPacket.WriteToken(cmd.InputObject.Token);

            cmd.PackInput(connection, packet.UdpPacket);
            cmd = entity.CommandQueue.Next(cmd);

            if (packet.UdpPacket.Overflowing) {
              packet.UdpPacket.Position = cmdPos;
              break;
            }
          }

          // overflowing, reset before this proxy and break
          if (packet.UdpPacket.Overflowing) {
            packet.UdpPacket.Position = proxyPos;
            break;
          }
          else {
            // stop marker for commands
            packet.UdpPacket.WriteStopMarker();
          }
        }
      }

      // stop marker for proxies
      packet.UdpPacket.WriteStopMarker();
    }

    void ReadInput(Packet packet) {
      int maxFrame = BoltCore._frame;
      int minFrame = maxFrame - (BoltCore._config.commandDelayAllowed + pingFrames);

      while (packet.UdpPacket.ReadStopMarker()) {
        NetworkId netId = packet.UdpPacket.ReadNetworkId();
        EntityProxy proxy = null;

        if (outgoingProxiesByNetworkId.ContainsKey(netId)) {
          proxy = outgoingProxiesByNetworkId[netId];
        }

        while (packet.UdpPacket.ReadStopMarker()) {
          Bolt.Command cmd = Factory.NewCommand(packet.UdpPacket.ReadTypeId());
          cmd.Sequence = packet.UdpPacket.ReadUShort(Command.SEQ_BITS);
          cmd.ServerFrame = packet.UdpPacket.ReadIntVB();
          cmd.InputObject.Token = packet.UdpPacket.ReadToken();
          cmd.ReadInput(connection, packet.UdpPacket);

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
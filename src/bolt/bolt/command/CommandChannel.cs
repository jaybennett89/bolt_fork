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


    bool EntityHasUnsentResults(Entity entity) {
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
        // 4) The entity has to have unsent results

        if ((entity != null) && ReferenceEquals(entity.Controller, connection) && connection._entityChannel.ExistsOnRemote(entity) && EntityHasUnsentResults(entity)) {
          Assert.True(entity.IsOwner);

          int proxyPos = packet.UdpPacket.Position;
          int cmdWriteCount = 0;

          packet.UdpPacket.WriteBool(true);
          packet.UdpPacket.WriteNetworkId(proxy.NetworkId);

          var it = entity.CommandQueue.GetIterator();

          while (it.Next()) {
            if (it.val.Flags & CommandFlags.HAS_EXECUTED) {
              if (it.val.Flags & CommandFlags.SEND_STATE) {
                int cmdPos = packet.UdpPacket.Position;

                packet.UdpPacket.WriteBool(true);
                packet.UdpPacket.WriteTypeId(it.val.ResultObject.Meta.TypeId);
                packet.UdpPacket.WriteIntVB(it.val.Sequence);
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
      var p = packet.UdpPacket;

      while (p.CanRead()) {
        if (p.ReadBool() == false) { break; }

        NetworkId netId = p.ReadNetworkId();
        EntityProxy proxy = incommingProxiesByNetworkId[netId];
        Entity entity = proxy.Entity;

        while (p.CanRead()) {
          if (p.ReadBool() == false) { break; }

          var typeId = p.ReadTypeId();
          var sequence = p.ReadIntVB();
          var token = p.ReadToken();

          Command cmd = null;

          if (entity != null) {
            var it = entity.CommandQueue.GetIterator();

            while (it.Next()) {
              // this is later than we received, skip
              if (it.val.Sequence > sequence) {
                break;
              }

              // this is 
              else if (it.val.Sequence < sequence) {
                it.val.Flags |= CommandFlags.DISPOSE;
              }

              // we found what we need
              else {
                cmd = it.val;
                break;
              }
            }
          }

          if (cmd) {
            cmd.Flags |= CommandFlags.CORRECTION_RECEIVED;
            cmd.ResultObject.Token = token;

            if (cmd.Meta.SmoothFrames > 0) {
              cmd.BeginSmoothing();
            }

            cmd.ReadResult(connection, p);
          }
          else {
            cmd = Factory.NewCommand(typeId);
            cmd.ReadResult(connection, p);
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
      var p = packet.UdpPacket;

      foreach (EntityProxy proxy in incommingProxiesByNetworkId.Values) {
        Entity entity = proxy.Entity;

        if (entity && entity.HasControl && (entity.CommandQueue.count > 0)) {
          int proxyPos = p.Position;
          p.WriteContinueMarker();
          p.WriteNetworkId(proxy.NetworkId);

          Command cmd = entity.CommandQueue.last;

          // how many commands we should send at most
          int redundancy = Mathf.Min(entity.CommandQueue.count, BoltCore._config.commandRedundancy);

          // go to first command
          for (int i = 0; i < (redundancy - 1); ++i) {
            cmd = entity.CommandQueue.Prev(cmd);
          }

          // write all commands into the packet
          for (int i = 0; i < redundancy; ++i) {
            int cmdPos = p.Position;

            p.WriteContinueMarker();
            p.WriteTypeId(cmd.Meta.TypeId);
            p.WriteIntVB(cmd.Sequence);
            p.WriteIntVB(cmd.ServerFrame);
            p.WriteToken(cmd.InputObject.Token);

            cmd.PackInput(connection, p);
            cmd = entity.CommandQueue.Next(cmd);

            if (p.Overflowing) {
              p.Position = cmdPos;
              break;
            }
          }

          // overflowing, reset before this proxy and break
          if (p.Overflowing) {
            p.Position = proxyPos;
            break;
          }
          else {
            // stop marker for commands
            p.WriteStopMarker();
          }
        }
      }

      // stop marker for proxies
      p.WriteStopMarker();
    }

    void ReadInput(Packet packet) {
      var p = packet.UdpPacket;

      int maxFrame = BoltCore._frame;
      int minFrame = maxFrame - (BoltCore._config.commandDelayAllowed + pingFrames);

      while (packet.UdpPacket.ReadStopMarker()) {
        NetworkId netId = packet.UdpPacket.ReadNetworkId();
        EntityProxy proxy = null;

        if (outgoingProxiesByNetworkId.ContainsKey(netId)) {
          proxy = outgoingProxiesByNetworkId[netId];
        }

        while (p.ReadStopMarker()) {
          Bolt.Command cmd = Factory.NewCommand(p.ReadTypeId());
          cmd.Sequence = p.ReadIntVB();
          cmd.ServerFrame = p.ReadIntVB();
          cmd.InputObject.Token = p.ReadToken();
          cmd.ReadInput(connection, p);

          // no proxy or entity
          if (!proxy || !proxy.Entity) { continue; }

          Entity entity = proxy.Entity;

          // remote is not controller
          if (ReferenceEquals(entity.Controller, connection) == false) { continue; }

          // sequence is old
          if (cmd.Sequence < entity.CommandSequence) { continue; }

          // put on command queue
          entity.CommandQueue.AddLast(cmd);
          entity.CommandSequence = cmd.Sequence;
        }
      }
    }
  }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  abstract class NetworkIdChannel : BoltChannel {
    NetworkIdChannel() {

    }

    public static NetworkIdChannel CreateServer() {
      return new Server();
    }

    public static NetworkIdChannel CreateClient() {
      return new Client();
    }

    class Server : NetworkIdChannel {
      enum States {
        Idle,
        Send,
        Pending,
      }

      States state = States.Idle;
      NetworkId block = default(NetworkId);

      public override void Delivered(BoltPacket packet) {
        if (packet.NetworkIdBlock.Value != 0UL) {
          if (packet.NetworkIdBlock.Value == block.Value) {
            state = States.Idle;
          }
          else {
            Assert.True(packet.NetworkIdBlock.Value < block.Value);
          }
        }
      }

      public override void Pack(BoltPacket packet) {
        if (packet.stream.WriteBool(state == States.Send)) {
          packet.stream.PackNetworkId(block);
        }
      }

      public override void Read(BoltPacket packet) {
        if (packet.stream.ReadBool()) {
          if (state == States.Idle) {
            block = NetworkIdAllocator.AllocateBlock();
            state = States.Send;
          }
        }
      }
    }

    class Client : NetworkIdChannel {
      NetworkId block = default(NetworkId);

      public override void Pack(BoltPacket packet) {
        packet.stream.WriteBool(NetworkIdAllocator.RequestMoreBlocks);
      }

      public override void Read(BoltPacket packet) {
        if (packet.stream.ReadBool()) {
          NetworkId blockReceived = packet.stream.ReadNetworkId();

          if (blockReceived.Value > block.Value) {
            NetworkIdAllocator.AddBlock(blockReceived);

            block = blockReceived;
          }
        }
      }
    }
  }
}

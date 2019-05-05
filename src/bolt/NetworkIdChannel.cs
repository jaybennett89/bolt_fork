namespace Bolt {
  abstract class NetworkIdChannel : BoltChannel {
    NetworkIdChannel() {

    }

    public static NetworkIdChannel Create(bool isServer) {
      return isServer ? (NetworkIdChannel)new Server() : (NetworkIdChannel)new Client();
    }

    class Server : NetworkIdChannel {
      NetworkId localBlock = default(NetworkId);
      NetworkId remoteBlock = default(NetworkId);

      public override void Pack(BoltPacket packet) {
        if (packet.stream.WriteBool(remoteBlock.Packed != localBlock.Packed)) {
          packet.stream.PackNetworkId(localBlock);
        }
      }

      public override void Read(BoltPacket packet) {
        if (packet.stream.ReadBool()) {
          remoteBlock = packet.stream.ReadNetworkId();

          if (remoteBlock == localBlock) {
            localBlock = NetworkIdAllocator.AllocateBlock();
          }
        }
      }
    }

    class Client : NetworkIdChannel {
      NetworkId localBlock = default(NetworkId);

      public override void Pack(BoltPacket packet) {
        if (packet.stream.WriteBool(NetworkIdAllocator.RequestMoreBlocks)) {
          packet.stream.PackNetworkId(localBlock);
        }
      }

      public override void Read(BoltPacket packet) {
        if (packet.stream.ReadBool()) {
          var remoteBlock = packet.stream.ReadNetworkId();
          if (remoteBlock != localBlock) {
            Assert.True(remoteBlock.Packed > localBlock.Packed);

            NetworkIdAllocator.AddBlock(remoteBlock);
            localBlock = remoteBlock;
          }
        }
      }
    }
  }
}

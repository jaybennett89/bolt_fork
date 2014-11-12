using System.Collections.Generic;
using System.Linq;

namespace Bolt {
  static class NetworkIdAllocator {
    static ulong BlockSize = 256;
    static Queue<NetworkIdBlock> Blocks = new Queue<NetworkIdBlock>();

    public static bool RequestMoreBlocks {
      get {
        ulong sum = 0UL;

        foreach (NetworkIdBlock block in Blocks) {
          sum += block.Count;
        }

        return sum <= (BlockSize >> 1);
      }
    }

    public static void ResetServer() {
      Blocks = new Queue<NetworkIdBlock>();
      AddBlock(new NetworkId(1UL), ulong.MaxValue - 1UL);
    }

    public static void ResetClient() {
      Blocks = new Queue<NetworkIdBlock>();
    }

    public static NetworkId Allocate() {
      // clear out old used blocks
      while (Blocks.Count > 0 && Blocks.Peek().Count == 0) {
        Blocks.Dequeue();
      }

      // this will only ever be true on the client
      if (Blocks.Count == 0) {
        return default(NetworkId);
      }

      NetworkIdBlock block = Blocks.Peek();
      NetworkId id = block.Start;

      block.Count -= 1;
      block.Start = new NetworkId(block.Start.Value + 1UL);

      return id;
    }

    public static void AddBlock(NetworkId start) {
      AddBlock(start, BlockSize);
    }

    internal static void AddBlock(NetworkId start, ulong count) {
      Blocks.Enqueue(new NetworkIdBlock { Start = start, Count = count });
    }

    public static NetworkId AllocateBlock() {
      Assert.True(BoltCore.isServer);

      NetworkIdBlock block = Blocks.Peek();
      NetworkId id = block.Start;

      block.Start = new NetworkId(block.Start.Value + BlockSize);
      block.Count -= BlockSize;

      return id;
    }
  }

  class NetworkIdBlock {
    public NetworkId Start;
    public ulong Count;
  }

  public struct NetworkId {
    public class EqualityComparer : IEqualityComparer<NetworkId> {
      public static readonly EqualityComparer Instance = new EqualityComparer();

      EqualityComparer() {

      }

      bool IEqualityComparer<NetworkId>.Equals(NetworkId x, NetworkId y) {
        return x.Value == y.Value;
      }

      int IEqualityComparer<NetworkId>.GetHashCode(NetworkId x) {
        return x.Value.GetHashCode();
      }
    }

    public bool IsZero {
      get { return Value == 0UL; }
    }

    internal readonly ulong Value;

    internal NetworkId(ulong value) {
      Value = value;
    }

    public override string ToString() {
      byte b0 = (byte)(Value >> 56);
      byte b1 = (byte)(Value >> 48);
      byte b2 = (byte)(Value >> 40);
      byte b3 = (byte)(Value >> 32);
      byte b4 = (byte)(Value >> 24);
      byte b5 = (byte)(Value >> 16);
      byte b6 = (byte)(Value >> 8);
      byte b7 = (byte)(Value >> 0);

      return string.Format("[NetworkId {0:X0}-{1:X0}-{2:X0}-{3:X0}-{4:X0}-{5:X0}-{6:X0}-{7:X0}]", b0, b1, b2, b3, b4, b5, b6, b7);
    }
  }
}

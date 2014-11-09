using UdpKit;

namespace Bolt {
  public struct NetworkId {
    internal readonly ulong Value;

    internal NetworkId(ulong value) {
      Value = value;
    }
  }
}

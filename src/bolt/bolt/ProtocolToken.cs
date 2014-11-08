using UdpKit;

namespace Bolt {
  public interface IProtocolToken {
    void Pack(UdpPacket packet);
    void Read(UdpPacket packet);
  }
}

using System;
using UdpKit;

namespace Bolt {
  public interface IProtocolToken {
    void Pack(UdpPacket packet);
    void Read(UdpPacket packet);
  }

  static class ProtocolTokenUtils {
    public static byte[] ToByteArray(this IProtocolToken token) {
      UdpPacket packet;
      packet = new UdpPacket(new byte[BoltCore._config.packetSize - 256]);
      packet.WriteByte(Factory.GetTokenId(token));

      token.Pack(packet);

      byte[] data = new byte[Bolt.Math.BytesRequired(packet.Position)];
      Array.Copy(packet.Data, 0, data, 0, data.Length);

      return data;
    }

    public static IProtocolToken ToToken(this byte[] bytes) {
      UdpPacket packet;
      packet = new UdpPacket(bytes);
      packet.Ptr = 8;

      IProtocolToken token;
      
      token = Factory.NewToken(bytes[0]);
      token.Read(packet);

      return token;
    }
  }
}

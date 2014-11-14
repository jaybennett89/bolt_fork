using System;
using UdpKit;

namespace Bolt {
  public interface IProtocolToken {
    void Read(UdpPacket packet);
    void Write(UdpPacket packet);
  }

  static class ProtocolTokenUtils {
    static byte[] tempBytes;
    static UdpPacket tempPacket;

    public static byte[] ToByteArray(this IProtocolToken token) {
      if (token == null) {
        return null;
      }

      if ((tempBytes == null) || (tempBytes.Length != (BoltCore._config.packetSize - 256))) {
        tempBytes = new byte[BoltCore._config.packetSize - 256];
      }

      if (tempPacket == null) {
        tempPacket = new UdpPacket();
      }

      // clear data
      Array.Clear(tempBytes, 0, tempBytes.Length);

      // setup packet 
      tempPacket.Ptr = 0;
      tempPacket.Data = tempBytes;
      tempPacket.Size = tempBytes.Length << 3;
      tempPacket.WriteByte(Factory.GetTokenId(token));

      // write token
      token.Write(tempPacket);

      return tempPacket.DuplicateData();
    }

    public static IProtocolToken ToToken(this byte[] bytes) {
      if ((bytes == null) || (bytes.Length == 0)) {
        return null;
      }

      if (tempPacket == null) {
        tempPacket = new UdpPacket();
      }

      // setup packet
      tempPacket.Ptr = 8;
      tempPacket.Data = bytes;
      tempPacket.Size = bytes.Length << 3;

      IProtocolToken token;
      token = Factory.NewToken(bytes[0]);
      token.Read(tempPacket);

      return token;
    }
  }
}

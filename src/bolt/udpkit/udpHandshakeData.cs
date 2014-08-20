namespace UdpKit {
  public class UdpHandshakeData {
    public string Name { get; private set; }
    public byte[] Data { get; private set; }

    public UdpHandshakeData (string name, byte[] data) {
      Name = name;
      Data = data;
    }
  }

  struct UdpHandshakeResult {
    public UdpHandshakeResultType type;
    public int failDataIndex;
    public int failBufferOffset;
    public int failBufferLength;
  }

  enum UdpHandshakeResultType {
    Success,
    InvalidSize,
    InvalidValue
  }
}

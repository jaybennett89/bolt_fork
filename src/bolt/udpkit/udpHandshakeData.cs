namespace UdpKit {
  public class UdpHandshakeData {
    public string Name { get; private set; }
    public byte[] Data { get; private set; }

    public UdpHandshakeData (string name, byte[] data) {
      Name = name;
      Data = data;
    }
  }

  enum UdpHandshakeResult {
    Success,
    InvalidSize,
    InvalidValue
  }
}

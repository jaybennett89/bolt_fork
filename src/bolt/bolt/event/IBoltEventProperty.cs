using UdpKit;

public interface IBoltCustomProperty {
  void Pack (UdpStream stream, BoltConnection connection);
  void Read (UdpStream stream, BoltConnection connection);
}

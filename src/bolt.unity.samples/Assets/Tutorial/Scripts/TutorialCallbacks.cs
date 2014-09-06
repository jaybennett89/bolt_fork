
using UdpKit;
[BoltGlobalBehaviour]
public class TutorialCallbacks : BoltCallbacks {
  public override void ClientConnected (BoltConnection connection) {



    BoltLog.Info("Client from {0} is now connected", connection.remoteEndPoint);
  }

  public override void ConnectedToServer (BoltConnection connection) {
    BoltLog.Info("We are connected to the server at {0}", connection.remoteEndPoint);
  }
}

using UnityEngine;
using System.Collections;
using System.Text;

[BoltGlobalBehaviour(BoltNetworkModes.Server)]
public class ServerConnectCallbacks : BoltCallbacks {
  public override void ConnectRequest(UdpKit.UdpEndPoint endpoint, byte[] token) {
    Player p;

    p = new Player();
    p.name = token == null ? ("CLIENT:" + endpoint.Port) : Encoding.ASCII.GetString(token);

    BoltNetwork.Accept(endpoint, p);
  }

  public override void Connected(BoltConnection c) {
    c.GetPlayer().connection = c;
  }

  public override void Disconnected(BoltConnection arg) {

  }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[BoltGlobalBehaviour(BoltNetworkModes.Server, "Level1")]
public class ServerCallbacks : BoltCallbacks {
  public static bool ListenServer = true;

  void Awake() {
    if (ListenServer) {
      Player.CreateServerPlayer();
    }
  }

  public override void ConnectRequest(UdpKit.UdpEndPoint endpoint, byte[] token) {
    Player p;

    p = new Player();
    p.name = token == null ? "UNKNOWN" : Encoding.ASCII.GetString(token);

    BoltNetwork.Accept(endpoint, p);
  }

  public override void Connected(BoltConnection arg) {
    Player p;

    p = new Player();
    p.connection = arg;

    arg.userToken = p;
  }

  public override void Disconnected(BoltConnection arg) {

  }

  public override void SceneLoadRemoteDone(BoltConnection connection, string map) {
    connection.GetPlayer().InstantiateEntity();
  }

  public override void SceneLoadLocalDone(string map) {
    if (Player.serverIsPlaying) {
      Player.serverPlayer.InstantiateEntity();
    }
  }

  public override void SceneLoadLocalBegin(string map) {
    foreach (Player p in Player.allPlayers) {
      p.entity = null;
    }
  }

  public BoltEntity SpawnPlayerCharacter() {
    return BoltNetwork.Instantiate(BoltPrefabs.Player, new Vector3(0, 15, 0), Quaternion.identity);
  }
}

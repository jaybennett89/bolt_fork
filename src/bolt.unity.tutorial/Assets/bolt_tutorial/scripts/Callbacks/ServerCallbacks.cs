using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[BoltGlobalBehaviour(BoltNetworkModes.Server, "Level1")]
public class ServerCallbacks : BoltCallbacks {
  void Awake() {
    Player.CreateServerPlayer();
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

  public override void SceneLoadRemoteDone(BoltConnection connection, string map) {
    Player player;

    player = Player.GetPlayer(connection);
    player.entity = SpawnPlayerCharacter();
    player.entity.GiveControl(connection);
  }

  public override void SceneLoadLocalDone(string map) {
    if (Player.serverIsPlaying) {
      Player.serverPlayer.entity = SpawnPlayerCharacter();
      Player.serverPlayer.entity.TakeControl();
    }
  }

  public BoltEntity SpawnPlayerCharacter() {
    return BoltNetwork.Instantiate(BoltPrefabs.Player, new Vector3(0, 15, 0), Quaternion.identity);
  }
}

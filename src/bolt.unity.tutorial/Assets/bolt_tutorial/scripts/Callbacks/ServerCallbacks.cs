using UnityEngine;
using System.Collections;

[BoltGlobalBehaviour(BoltNetworkModes.Server, "Level1")]
public class ServerCallbacks : BoltCallbacks {

  void Awake() {
    Player.CreateServerPlayer();
  }

  public override void ClientConnected(BoltConnection arg) {
    arg.userToken = new Player();
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

using UnityEngine;
using System.Collections;

[BoltGlobalBehaviour(BoltNetworkModes.Server, "Sample_TeleportAndElevators")]
public class TeleportServerCallbacks : BoltCallbacks {
  void Awake () {
    GameObject.DontDestroyOnLoad(gameObject);
  }

  public override void ClientDisconnected (BoltConnection arg) {
    BoltEntity entity = arg.userToken as BoltEntity;

    if (entity) {
      BoltNetwork.Destroy(entity);
    }
  }

  public override void MapLoadRemoteDone (BoltConnection connection, string map) {
    BoltEntity entity = SpawnAvatar();
    connection.userToken = entity;
    entity.GiveControl(connection);
  }

  public override void MapLoadLocalDone (string arg) {
    SpawnAvatar().TakeControl();
  }

  BoltEntity SpawnAvatar () {
    BoltEntity entity = BoltNetwork.Instantiate(BoltPrefabs.TeleportPlayer);

    entity.transform.position = new Vector3(
      Random.Range(-8f, 8f),
      0f,
      Random.Range(-8f, 8f)
    );

    return entity;
  }

}

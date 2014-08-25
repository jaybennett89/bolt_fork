using UnityEngine;

public class ServerCallbacks : BoltCallbacks {
  public override void MapLoadLocalDone (string arg) {
    // Spawn entity for server
    BoltEntity entity = BoltNetwork.Instantiate(BoltPrefabs.Player);
    entity.transform.position = new Vector3(0, 1f, 0);
    entity.GetComponent<PlayerSerializer>().boltState.name = "Server";

    // let server take local control
    entity.TakeControl();
  }

  public override void MapLoadRemoteDone (string map, BoltConnection arg) {
    // spawn new entity
    BoltEntity entity = BoltNetwork.Instantiate(BoltPrefabs.Player);

    // set position for entity
    entity.transform.position = new Vector3(Random.Range(-8f, 8f), 1f, Random.Range(-8f, 8f));
    entity.GetComponent<PlayerSerializer>().boltState.name = "Client:" + arg.id;

    // assign entity to the client connection userToken property
    arg.userToken = entity;

    // give control of this entity to the client we spawned it for
    entity.GiveControl(arg);
  }

  void OnGUI () {
    if (GUI.Button(new Rect(10, Screen.height - 50, 100, 40), "Toggle Rain")) {
      IToggleRain evnt = BoltFactory.NewEvent<IToggleRain>();
      BoltNetwork.Raise(evnt);
    }
  }
}

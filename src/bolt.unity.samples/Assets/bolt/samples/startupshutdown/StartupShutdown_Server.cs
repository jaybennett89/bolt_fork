using UnityEngine;
using System.Collections;

[BoltGlobalBehaviour(BoltNetworkModes.Server, "StartupShutdown_MapGreen", "StartupShutdown_MapRed")]
public class StartupShutdown_Server : BoltCallbacks {
  void Awake () {
    BoltNetwork.ShutdownComplete = BoltNetwork.ShutdownComplete.Combine(BoltNetwork_ShutdownComplete);
  }

  void BoltNetwork_ShutdownComplete () {
    Application.LoadLevel(0);
  }

  void OnGUI () {
    GUILayout.BeginArea(new Rect(10, Screen.height * 0.55f, Screen.width - 20, Screen.height * 0.45f));

    if (GUILayout.Button("Load Green Map", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
      BoltNetwork.LoadMap("StartupShutdown_MapGreen");
    }

    if (GUILayout.Button("Load Red Map", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
      BoltNetwork.LoadMap("StartupShutdown_MapRed");
    }

    if (GUILayout.Button("Shutdown Server", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
      BoltNetwork.Shutdown();
    }

    GUILayout.EndArea();
  }

  public override void ClientDisconnected (BoltConnection arg) {
    BoltEntity cube = arg.userToken as BoltEntity;

    if (cube) {
      BoltNetwork.Destroy(cube);
    }
  }

  public override void SceneLoadRemoteDone (BoltConnection arg, string map) {
    BoltEntity cube =  BoltNetwork.Instantiate(BoltPrefabs.ClientCube);
    cube.transform.position = new Vector3(Random.Range(-8f, 8f), 0, Random.Range(-8f, 8f));
    cube.GiveControl(arg);

    arg.userToken = cube;
  }
}

using UnityEngine;
using System.Collections;

[BoltGlobalBehaviour(BoltNetworkModes.Client, "StartupShutdown_MapGreen", "StartupShutdown_MapRed")]
public class StartupShutdown_Client : BoltCallbacks {
  void Awake () {
    DontDestroyOnLoad(gameObject);
    BoltNetwork.ShutdownComplete += BoltNetwork_ShutdownComplete;
  }

  void BoltNetwork_ShutdownComplete () {
    Application.LoadLevel(0);
  }

  void OnGUI () {
    GUILayout.BeginArea(new Rect(10, Screen.height * 0.55f, Screen.width - 20, Screen.height * 0.45f));

    if (GUILayout.Button("Disconnect Client", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
      BoltNetwork.Shutdown();
    }

    GUILayout.EndArea();
  }

  public override void DisconnectedFromServer (BoltConnection arg) {
    BoltNetwork.Shutdown();
  }
}

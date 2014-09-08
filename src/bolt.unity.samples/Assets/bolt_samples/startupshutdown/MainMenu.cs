using UnityEngine;
using System.Collections;
using UdpKit;

public class MainMenu : MonoBehaviour {
  void OnGUI () {
    GUILayout.BeginArea(new Rect(10, Screen.height * 0.55f, Screen.width - 20, Screen.height * 0.45f));

    if (GUILayout.Button("Server", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
      BoltNetwork.StartServer(new UdpEndPoint(UdpIPv4Address.Localhost, 40000));
      BoltNetwork.LoadMap("StartupShutdown_MapGreen");
    }

    if (GUILayout.Button("Client", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
      BoltNetwork.StartClient();
      BoltNetwork.Connect(new UdpEndPoint(UdpIPv4Address.Localhost, 40000));
    }

    GUILayout.EndArea();
  }
}

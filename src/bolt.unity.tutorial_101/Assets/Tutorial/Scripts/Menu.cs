using UnityEngine;
using System.Collections;
using UdpKit;

public class Menu : MonoBehaviour {
  void OnGUI() {
    GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, (Screen.height / 2) - 20));

    if (GUILayout.Button("Start Server", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
      BoltLauncher.StartServer(UdpKit.UdpEndPoint.Parse("192.168.2.173:27000"));
      BoltNetwork.SetHostInfo("TestServer", null);
      BoltNetwork.EnableLanBroadcast();
      BoltNetwork.LoadScene("Tutorial1");
    }

    if (GUILayout.Button("Start Client", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
      BoltLauncher.StartClient();
      BoltNetwork.EnableLanBroadcast();
    }

    GUILayout.EndArea();

    GUILayout.BeginArea(new Rect(10, (Screen.height / 2) + 10, Screen.width / 2, (Screen.height / 2) - 20), GUI.skin.box);
    GUILayout.Label(string.Format("Sessions: {0}", BoltNetwork.SessionList.Count));

    foreach (var kvp in BoltNetwork.SessionList) {
      GUILayout.Label(kvp.Value.HostName);
    }

    GUILayout.EndArea();
  }
}

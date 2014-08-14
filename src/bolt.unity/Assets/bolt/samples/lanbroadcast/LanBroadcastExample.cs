using UdpKit;
using UnityEngine;

public class LanBroadcastExample : MonoBehaviour {
  bool ExpandButton (string label) {
    return GUILayout.Button(label, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
  }

  void OnGUI () {

    GUILayout.BeginArea(new Rect(10, Screen.height - 110, Screen.width - 20, 100));
    GUILayout.BeginHorizontal();

    if (BoltNetwork.isClient == false && BoltNetwork.isServer == false) {
      if (ExpandButton("Start Server")) {
        // Note: we can initialize the server with "any" endpoint, 
        // as the clients will find it through the lan tbroadcasting

        BoltNetwork.InitializeServer(UdpEndPoint.Any);
        BoltNetwork.SetSessionData("ExampleServer", "Here's some test data also!");
        BoltNetwork.EnableLanBroadcast();
      }

      if (ExpandButton("Start Client")) {
        BoltNetwork.StartClient(UdpEndPoint.Any);
        BoltNetwork.EnableLanBroadcast();
      }
    }

    if (BoltNetwork.isClient) {
      UdpSession[] sessions = BoltNetwork.GetSessions();

      for (int i = 0; i < sessions.Length; ++i) {
        if (ExpandButton(sessions[i].ServerName + ": " + sessions[i].UserData)) {
          BoltNetwork.Connect(sessions[i].EndPoint);
        }
      }
    }

    GUILayout.EndHorizontal();
    GUILayout.EndArea();
  }
}

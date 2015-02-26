using UnityEngine;
using System.Collections;
using UdpKit;

public class Test : MonoBehaviour {
	void OnGUI () {
    if (GUILayout.Button("StartServer", GUILayout.Width(400), GUILayout.Height(400) )) {
      BoltLauncher.StartServer(UdpEndPoint.Parse("0.0.0.0:27000"));
    }

    if (GUILayout.Button("StartClient", GUILayout.Width(400), GUILayout.Height(400))) {
      BoltLauncher.StartClient();
      BoltNetwork.Connect(UdpEndPoint.Parse("192.168.2.173:27000"));
    }
	}
}

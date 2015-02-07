﻿using UnityEngine;
using System.Collections;
using UdpKit;

public class Menu : MonoBehaviour {
  void OnGUI() {
    GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, (Screen.height / 2) - 20));

    if (GUILayout.Button("Start Server", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
      BoltLauncher.StartServer(UdpKit.UdpEndPoint.Parse("0.0.0.0:27000"));
      BoltNetwork.SetHostInfo("TestServer", null);
      BoltNetwork.LoadScene("Level1", new TestToken());
    }

    if (GUILayout.Button("Start Client", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
      BoltLauncher.StartClient();
      BoltNetwork.Connect(UdpEndPoint.Parse("127.0.0.1:27000"));
    }

    GUILayout.EndArea();
  }
}

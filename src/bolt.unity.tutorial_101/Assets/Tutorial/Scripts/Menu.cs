﻿using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {
  void OnGUI() {
    GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));

    if (GUILayout.Button("Start Server", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
      BoltLauncher.StartServer(UdpKit.UdpEndPoint.Parse("192.168.2.173:27000"));
      BoltNetwork.LoadScene("Tutorial1");
    }

    if (GUILayout.Button("Start Client", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
      BoltLauncher.StartClient();
      BoltNetwork.Connect(UdpKit.UdpEndPoint.Parse("192.168.2.173:27000"));
    }

    GUILayout.EndArea();
  }
}

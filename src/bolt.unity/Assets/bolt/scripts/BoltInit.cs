using UnityEngine;
using System.Collections;
using System;
using UdpKit;

public class BoltInit : MonoBehaviour {
  enum State {
    SelectMode,
    SelectMap,
    StartServer,
    StartClient,
    Started,
  }

  State state;
  string map;
  
  [SerializeField]
  string serverAddress = "127.0.0.1";

  [SerializeField]
  int serverPort = 40000;

  void OnGUI () {
    GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));

    switch (state) {
      case State.SelectMode: State_SelectMode(); break;
      case State.SelectMap: State_SelectMap(); break;
      case State.StartClient: State_StartClient(); break;
      case State.StartServer: State_StartServer(); break;
    }

    GUILayout.EndArea();
  }


  void State_SelectMode () {
    if (ExpandButton("Server")) {
      state = State.SelectMap;
    }
    if (ExpandButton("Client")) {
      state = State.StartClient;
    }
  }

  void State_SelectMap () {
    foreach (string value in Enum.GetNames(typeof(BoltMapNames))) {
      GUI.color = (map == value) ? Color.green : Color.white;
      if (ExpandButton(value)) {
        map = value;
      }
      GUI.color = Color.white;
    }

    if (ExpandButton("Start Server")) {
      if (string.IsNullOrEmpty(map)) {
        Debug.LogError("Select a map first");
      }
      else {
        state = State.StartServer;
      }
    }
  }

  void State_StartServer () {
    BoltNetwork.StartServer(new UdpEndPoint(UdpIPv4Address.Parse(serverAddress), (ushort) serverPort));
    BoltNetwork.LoadMap(map);
    state = State.Started;
  }

  void State_StartClient () {
    BoltNetwork.StartClient(UdpEndPoint.Any);
    BoltNetwork.Connect(new UdpEndPoint(UdpIPv4Address.Parse(serverAddress), (ushort) serverPort));
    state = State.Started;
  }

  bool ExpandButton (string text) {
    return GUILayout.Button(text, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
  }
}

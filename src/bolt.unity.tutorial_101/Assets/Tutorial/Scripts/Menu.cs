using UnityEngine;
using System.Collections;
using UdpKit;

public class Menu : Bolt.GlobalEventListener {

  enum State {
    SelectPeer,
    ServerBrowser
  }

  State state = State.SelectPeer;

  public override void ZeusConnected(UdpEndPoint endpoint) {
    Bolt.Zeus.RequestSessionList();
  }

  void SelectPeer() {
    if (GUILayout.Button("Start Server", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
      BoltLauncher.StartServer(UdpKit.UdpEndPoint.Parse("192.168.2.173:27000"));
      BoltNetwork.SetHostInfo("TestServer", null);
      BoltNetwork.LoadScene("Tutorial1");
    }

    if (GUILayout.Button("Start Client", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
      BoltLauncher.StartClient();
      state = State.ServerBrowser;
    }

    GUILayout.EndArea();

    GUILayout.BeginArea(new Rect(10, (Screen.height / 2) + 10, Screen.width / 2, (Screen.height / 2) - 20), GUI.skin.box);
    GUILayout.Label(string.Format("Sessions: {0}", BoltNetwork.SessionList.Count));

    foreach (var kvp in BoltNetwork.SessionList) {
      GUILayout.Label(kvp.Value.HostName);
    }
  }

  void ServerBrowser() {
    GUILayout.Label("Server Browser");

    GUILayout.BeginVertical(GUI.skin.box);

    foreach (var session in BoltNetwork.SessionList) {
      GUILayout.BeginHorizontal();

      GUILayout.Label(session.Value.HostName);

      if (GUILayout.Button("Join")) {
        BoltNetwork.Connect(session.Value);
      }

      GUILayout.EndHorizontal();
    }

    GUILayout.EndVertical();
  }

  void OnGUI() {
    GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, (Screen.height / 2) - 20));

    switch (state) {
      case State.SelectPeer: SelectPeer(); break;
      case State.ServerBrowser: ServerBrowser(); break;
    }
 
    GUILayout.EndArea();
  }
}

using UnityEngine;
using System.Collections;
using System;

public class Init : MonoBehaviour {
  string host = "0.0.0.0:0000";

	void Start () {
    string[] args = System.Environment.GetCommandLineArgs();

    if (System.Array.IndexOf(args, "-batchmode") >= 0) {
      Bolt.ConsoleWriter.Open();

      int map = System.Array.IndexOf(args, "-map");

      if (map >= 0) {
        BoltLauncher.StartServer();
        BoltNetwork.LoadScene(args[map + 1]);
      }
      else {
        Console.WriteLine("You have to specify a map");
        Application.Quit();
      }
    }
	}
	
	void OnGUI () {
    host = GUILayout.TextField(host, GUILayout.ExpandWidth(true));

    if (GUILayout.Button("Connect To Server", GUILayout.ExpandWidth(true))) {
      BoltLauncher.StartClient();
      BoltNetwork.Connect(UdpKit.UdpEndPoint.Parse(host));
    }
	}
}

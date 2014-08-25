using UdpKit;
using UnityEngine;
using Process = System.Diagnostics.Process;

public partial class BoltDebugStart : BoltCallbacksBase {
  [SerializeField]
  Texture2D logo;

  void Awake () {
    DontDestroyOnLoad(gameObject);
  }

  void Start () {
#if UNITY_EDITOR_OSX
    Process p = new Process();
    p.StartInfo.FileName = "osascript";
    p.StartInfo.Arguments = 

@"-e 'tell application """ + UnityEditor.PlayerSettings.productName + @"""
  activate
end tell'";

    p.Start();
#endif
    UdpEndPoint _serverEndPoint = new UdpEndPoint(UdpIPv4Address.Localhost, (ushort) BoltRuntimeSettings.instance.debugStartPort);
    UdpEndPoint _clientEndPoint = new UdpEndPoint(UdpIPv4Address.Localhost, 0);

    if (string.IsNullOrEmpty(BoltRuntimeSettings.instance.debugStartMapName) == false) {
      if (BoltDebugStartSettings.startServer) {
        BoltNetwork.StartServer(_serverEndPoint, BoltRuntimeSettings.instance.GetConfigCopy());
        BoltNetwork.LoadMap(BoltRuntimeSettings.instance.debugStartMapName);
      }
      else if (BoltDebugStartSettings.startClient) {
        BoltNetwork.StartClient(_clientEndPoint, BoltRuntimeSettings.instance.GetConfigCopy());
        BoltNetwork.Connect(_serverEndPoint);
      }

      BoltDebugStartSettings.PositionWindow();
    }
    else {
      BoltLog.Error("No map found to start from");
    }

    if (!BoltNetwork.isClient && !BoltNetwork.isServer) {
      BoltLog.Error("failed to start debug mode");
    }
  }

  void OnGUI () {
    if (logo) {
      GUI.DrawTexture(new Rect(10, Screen.height - 148, 256, 138), logo);
    }
  }

  void OnTriggerEnter () {
    BoltNetworkUtils.CreateUdpPlatform();
    BoltNetworkUtils.FindBroadcastAddress();
  }

  public override void MapLoadLocalDone (string arg) {
    Destroy(gameObject);
  }
}

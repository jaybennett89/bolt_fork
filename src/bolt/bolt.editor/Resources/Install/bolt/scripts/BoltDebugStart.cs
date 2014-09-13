using UdpKit;
using UnityEngine;
using Process = System.Diagnostics.Process;

public partial class BoltDebugStart : BoltCallbacksBase
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
#if UNITY_EDITOR_OSX
    Process p = new Process();
    p.StartInfo.FileName = "osascript";
    p.StartInfo.Arguments = 

@"-e 'tell application """ + UnityEditor.PlayerSettings.productName + @"""
  activate
end tell'";

    p.Start();
#endif
        UdpEndPoint _serverEndPoint = new UdpEndPoint(UdpIPv4Address.Localhost, (ushort)BoltRuntimeSettings.instance.debugStartPort);
        UdpEndPoint _clientEndPoint = new UdpEndPoint(UdpIPv4Address.Localhost, 0);

        BoltConfig cfg;

        cfg = BoltRuntimeSettings.instance.GetConfigCopy();
        cfg.connectionTimeout = 60000000;
        cfg.connectionRequestTimeout = 500;
        cfg.connectionRequestAttempts = 1000;

        if (string.IsNullOrEmpty(BoltRuntimeSettings.instance.debugStartMapName) == false)
        {
            if (BoltDebugStartSettings.startServer)
            {
                BoltNetwork.StartServer(_serverEndPoint, cfg);
                BoltNetwork.LoadScene(BoltRuntimeSettings.instance.debugStartMapName);
            }
            else if (BoltDebugStartSettings.startClient)
            {
                BoltNetwork.StartClient(_clientEndPoint, cfg);
                BoltNetwork.Connect(_serverEndPoint);
            }

            BoltDebugStartSettings.PositionWindow();
        }
        else
        {
            BoltLog.Error("No map found to start from");
        }

        if (!BoltNetwork.isClient && !BoltNetwork.isServer)
        {
            BoltLog.Error("failed to start debug mode");
        }
    }

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(10, Screen.height - 148, 256, 138), (Texture2D)Resources.Load("BoltLogo", typeof(Texture2D)));
    }

    void OnTriggerEnter()
    {
        BoltNetworkUtils.CreateUdpPlatform();
        BoltNetworkUtils.FindBroadcastAddress();
    }

    public override void SceneLoadLocalDone(string arg)
    {
        Destroy(gameObject);
    }
}

using UnityEngine;
using System.Collections;
using System.Text;

[BoltGlobalBehaviour]
public class StreamCallbacks : Bolt.GlobalEventListener {
  public static UdpKit.UdpChannelName TextureChannel;
  public static Texture2D Logo;

  public override void RegisterStreamChannels() {
    TextureChannel = BoltNetwork.CreateStreamChannel("Texture", UdpKit.UdpChannelMode.Reliable, 1);
  }

  public override void StreamDataReceived(BoltConnection connection, UdpKit.UdpStreamData data) {
    BoltLog.Info("received texture");

    Logo = new Texture2D(4096, 4096);
    Logo.LoadImage(data.Data);
  }

  public override void Connected(BoltConnection connection) {
    Texture2D logo = (Texture2D)Resources.Load("body_diff_brown", typeof(Texture2D));
    connection.StreamBytes(TextureChannel, logo.EncodeToPNG());
  }

  void OnGUI() {
    if (Logo != null) {
      GUI.DrawTexture(new Rect(0, 0, 256, 256), Logo);
    }
  }
}

using UnityEngine;
using System.Collections;
using System.Text;

[BoltGlobalBehaviour]
public class StreamCallbacks : Bolt.GlobalEventListener {
  public static UdpKit.UdpChannelName TextChannel;

  public override void BoltStarted() {
    TextChannel = BoltNetwork.CreateStreamChannel("Text", UdpKit.UdpChannelMode.Reliable, 1);
  }

  public override void StreamDataReceived(BoltConnection connection, UdpKit.UdpStreamData data) {
    BoltLog.Info("{0}: {1}", data.Channel, Encoding.UTF8.GetString(data.Data));
  }

  public override void Connected(BoltConnection connection) {
    connection.StreamBytes(TextChannel, Encoding.UTF8.GetBytes("TEST"));
  }
}

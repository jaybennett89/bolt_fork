using UnityEngine;
using System.Collections;

public class TestToken : Bolt.IProtocolToken {
  static int NumberCounter;
  public int Number = 0;

  public TestToken() {
    Number = ++NumberCounter;
  }

  void Bolt.IProtocolToken.Read(UdpKit.UdpPacket packet) {
    Number = packet.ReadInt();
  }

  void Bolt.IProtocolToken.Write(UdpKit.UdpPacket packet) {
    packet.WriteInt(Number);
  }

  public override string ToString() {
    return string.Format("[TestToken {0}]", Number);
  }
}


public class CharacterCustomization : Bolt.IProtocolToken {
  public int SkinId;
  public int HatId;

  public void Write(UdpKit.UdpPacket packet) {
    packet.WriteInt(SkinId);
    packet.WriteInt(HatId);
  }

  public void Read(UdpKit.UdpPacket packet) {
    SkinId = packet.ReadInt();
    HatId = packet.ReadInt();
  }
}

public class NetworkCallbacks : Bolt.GlobalEventListener {
  public override void BoltStarted() {
    BoltNetwork.RegisterTokenClass<CharacterCustomization>();
  }
}

public class MyEntityBehaviour : Bolt.EntityBehaviour<ICharacterState> {
  public override void Attached(Bolt.IProtocolToken token) {
    var customization = (CharacterCustomization)token;
    // ... use the customization data here
  }
}
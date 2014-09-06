using UnityEngine;
using System.Collections;

public class ElevatorSerializer : BoltEntitySerializer {

  public override void Attached () {
    throw new System.NotImplementedException();
  }

  public override void Pack (BoltEntityUpdateInfo info, UdpKit.UdpStream stream, ref Bits mask) {

  }

  public override void Read (BoltEntityUpdateInfo info, UdpKit.UdpStream stream) {

  }
}

using UnityEngine;
using System.Collections;

public class CarPlayerSerializer : BoltEntitySerializer<ICarPlayerState> {
  public override void Attached (ICarPlayerState state) {
    if (boltEntity.boltIsOwner) {
      state.isActiveInWorld = true;
    }
  }
}

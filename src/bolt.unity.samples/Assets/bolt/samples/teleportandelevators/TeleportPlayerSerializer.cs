using UnityEngine;
using System.Collections;

public class TeleportPlayerSerializer : BoltEntitySerializer<ITeleportPlayerState> {
  public override void Attached (ITeleportPlayerState state) {
    state.mecanim[0] = 1f;
    state.mecanim[1] = 0f;
  }
}

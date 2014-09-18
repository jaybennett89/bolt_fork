using UnityEngine;
using System.Collections;

public class PlayerSerializer : BoltEntitySerializer<IPlayerState> {
  public override void Attached(IPlayerState state) {
    // force correct layerweights due to mecanim quirks
    state.mecanim.animator.SetLayerWeight(0, 1);
    state.mecanim.animator.SetLayerWeight(1, 1);
  }

  public void ApplyDamage(byte damage) {
    if (!state.dead) {
      state.health -= damage;

      if (state.health > 100) {
        state.health = 0;
      }

      if (state.health == 0) {
        entity.controllingConnection.GetPlayer().Kill();
      }
    }
  }
}

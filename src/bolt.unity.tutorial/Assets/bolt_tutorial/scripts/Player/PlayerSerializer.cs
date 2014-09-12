using UnityEngine;
using System.Collections;

public class PlayerSerializer : BoltEntitySerializer<IPlayerState> {
  public override void Attached(IPlayerState state) {
    // force correct layerweights due to mecanim quirks
    state.mecanim.animator.SetLayerWeight(0, 1);
    state.mecanim.animator.SetLayerWeight(1, 1);


  }
}

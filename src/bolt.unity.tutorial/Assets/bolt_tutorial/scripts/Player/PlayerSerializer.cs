using UnityEngine;
using System.Collections;

public class PlayerSerializer : BoltEntitySerializer<IPlayerState> {
  public override void Attached(IPlayerState state) {
    // force correct layerweights due to mecanim quirks
    state.mecanim.animator.SetLayerWeight(0, 1);
    state.mecanim.animator.SetLayerWeight(1, 1);

    // team callback
    state.teamChanged += TeamChanged;
  }

  void TeamChanged() {
    var smr = GetComponentInChildren<SkinnedMeshRenderer>();

    switch (state.team) {
      case Player.TEAM_RED:
        smr.material.color = Color.red;
        break;

      case Player.TEAM_BLUE:
        smr.material.color = Color.blue;
        break;
    }
  }
}

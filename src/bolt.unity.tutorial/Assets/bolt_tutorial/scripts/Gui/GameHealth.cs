using UnityEngine;
using System.Collections;

public class GameHealth : BoltCallbacks {
  BoltEntity me;
  IPlayerState meState;

  [SerializeField]
  TypogenicText text;

  public override void ControlOfEntityGained(BoltEntity arg) {
    if (arg.boltSerializer is PlayerSerializer) {
      me = arg;
      meState = me.GetBoltState<IPlayerState>();
      meState.healthChanged = HealthChanged;

      HealthChanged();
    }
  }

  public override void ControlOfEntityLost(BoltEntity arg) {
    if (arg.boltSerializer is PlayerSerializer) {
      meState.healthChanged -= HealthChanged;

      me = null;
      meState = null;
    }
  }

  void Update() {
    text.transform.position = new Vector3(
      -(Screen.width / 2) + 4,
      -(Screen.height / 2) + 40,
      250f
    );
  }

  void HealthChanged() {
    Color c;

    if (meState.health <= 25) { c = Color.red; }
    else if (meState.health <= 50) { c = new Color(1f, 0.5f, 36f / 255f); }
    else if (meState.health <= 75) { c = Color.yellow; }
    else { c = Color.green; }

    text.ColorBottomLeft =
    text.ColorBottomRight =
    text.ColorTopLeft =
    text.ColorTopRight =
      c;

    text.Set("HP " + meState.health);
  }
}

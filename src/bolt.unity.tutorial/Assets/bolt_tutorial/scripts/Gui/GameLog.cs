using UnityEngine;
using System.Collections;

public class GameLog : BoltCallbacks {
  [SerializeField]
  TypogenicText text;


  void Update() {
    text.transform.position = new Vector3(
      -(Screen.width / 2) + 4,
      +(Screen.height / 2) - 4,
      0
    );
  }

  public override void OnEvent(ILogEvent evnt, BoltConnection cn) {

  }
}

using UnityEngine;

internal class BoltEventGlobalReceiverInternal : MonoBehaviour {
  protected void OnEnable () {
    BoltCore._globalEventDispatcher.Add(this);
  }

  protected void OnDisable () {
    BoltCore._globalEventDispatcher.Remove(this);
  }
}

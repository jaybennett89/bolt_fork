using UnityEngine;

internal class BoltEventGlobalReceiverInternal : MonoBehaviour {
  protected void OnEnable () {
    BoltCore._eventDispatcher.Register(this);
  }

  protected void OnDisable () {
    BoltCore._eventDispatcher.Remove(this);
  }
}

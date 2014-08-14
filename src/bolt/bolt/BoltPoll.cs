using UnityEngine;

[BoltExecutionOrder(-10000)]
public class BoltPoll : MonoBehaviour {
  protected void Awake () {
    DontDestroyOnLoad(gameObject);
  }

  protected void FixedUpdate () {
    BoltCore.FixedUpdate();
  }

  protected void OnDisable () {
    BoltCore.Shutdown();
  }
  
  protected void OnDestroy () {
    BoltCore.Shutdown();
  }

  protected void OnApplicationQuit () {
    BoltCore.Shutdown();
  }
}

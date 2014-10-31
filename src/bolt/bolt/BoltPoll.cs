using UnityEngine;

[BoltExecutionOrder(-10000)]
public class BoltPoll : MonoBehaviour {
  protected void Awake () {
    DontDestroyOnLoad(gameObject);
  }

  protected void FixedUpdate () {
    BoltCore._timer.Stop();
    BoltCore._timer.Reset();
    BoltCore._timer.Start();

    BoltCore.Poll();

    BoltCore._timer.Stop();

    Bolt.DebugInfo.PollTime = (int)BoltCore._timer.ElapsedMilliseconds;
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

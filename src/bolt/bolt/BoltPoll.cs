using UnityEngine;

[BoltExecutionOrder(-10000)]
public class BoltPoll : MonoBehaviour {
  protected void Awake() {
    DontDestroyOnLoad(gameObject);
  }

  protected void Update() {
    if (Time.timeScale != 1f) {
      // log this error
      BoltLog.Error("Time.timeScale value is incorrect: {0}f", Time.timeScale);

      // force this
      Time.timeScale = 1f;

      // log that we forced timescale to 1
      BoltLog.Error("Time.timeScale has been set to 1.0f by Bolt");
    }
  }

  protected void FixedUpdate() {
    BoltCore._timer.Stop();
    BoltCore._timer.Reset();
    BoltCore._timer.Start();

    BoltCore.Poll();

    BoltCore._timer.Stop();

    Bolt.DebugInfo.PollTime = (int)BoltCore._timer.ElapsedMilliseconds;
  }

  protected void OnDisable() {
    BoltCore.Shutdown();
  }

  protected void OnDestroy() {
    BoltCore.Shutdown();
  }

  protected void OnApplicationQuit() {
    BoltCore.Shutdown();
  }
}

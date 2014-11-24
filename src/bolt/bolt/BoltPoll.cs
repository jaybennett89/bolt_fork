using System.Diagnostics;
using UnityEngine;

[BoltExecutionOrder(-10000)]
public class BoltPoll : MonoBehaviour {
  public static Stopwatch Timer = new Stopwatch();

  protected void Awake() {
    DontDestroyOnLoad(gameObject);
  }

  protected void Update() {
    if (Time.timeScale != 1f) {
      // log this error
      BoltLog.Error("Time.timeScale value is incorrect: {0}", Time.timeScale);
      BoltLog.Error("Time.timeScale has been set to 1.0f by Bolt");

      // force this
      Time.timeScale = 1f;
    }
  }

  protected void FixedUpdate() {
    Timer.Stop();
    Timer.Reset();

    BoltCore._timer.Stop();
    BoltCore._timer.Reset();
    BoltCore._timer.Start();

    BoltCore.Poll();

    BoltCore._timer.Stop();

    Bolt.DebugInfo.PollTime = (int)BoltCore._timer.ElapsedMilliseconds;

    Timer.Stop();

    //BoltLog.Info("TIMER: " + Timer.Elapsed);
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

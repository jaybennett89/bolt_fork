using UnityEngine;

/// <summary>
/// Bolt behaviour to poll the network and step entities in the simulation
/// </summary>
/// <example>
/// *Example:* if bolt is missing the ```BoltPoll`` behaviour then the simulation is broken and we should shut down.
/// 
/// ```csharp
/// void CheckBoltHealth() {
///   if(BoltNetwork.globalObject.GetComponent&ltBoltPoll&gt() == null) {
///     Debug.Log("BoltPoll is missing!);
///     GameApplication.Shutdown(ErrorCode.Bolt);
///   }
/// }
/// ```
/// </example>
[BoltExecutionOrder(-10000)]
public class BoltPoll : MonoBehaviour {
  internal bool AllowImmediateShutdown = true;

  protected void Awake() {
    Application.runInBackground  = true;
    DontDestroyOnLoad(gameObject);
  }

  protected void Update() {
    try {
      if ((Time.timeScale != 1f) && BoltRuntimeSettings.instance.overrideTimeScale) {
        // log this error
        BoltLog.Error("Time.timeScale value is incorrect: {0}f", Time.timeScale);

        // force this
        Time.timeScale = 1f;

        // log that we forced timescale to 1
        BoltLog.Error("Time.timeScale has been set to 1.0f by Bolt");
      }
    }
    finally {
      BoltCore.Update();
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
    if (Application.isEditor && AllowImmediateShutdown) {
      BoltCore.ShutdownImmediate();
    }
  }

  protected void OnDestroy() {
    if (Application.isEditor && AllowImmediateShutdown) {
      BoltCore.ShutdownImmediate();
    }
  }

  protected void OnApplicationQuit() {
    if (AllowImmediateShutdown) {
      BoltCore.ShutdownImmediate();
    }
  }
}

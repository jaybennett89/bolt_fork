using System;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// <example>
/// *Example:* if bolt is missing the ```BoltSend`` behaviour then the simulation is broken and we should shut down.
/// 
/// ```csharp
/// void CheckBoltHealth() {
///   if(BoltNetwork.globalObject.GetComponent&ltBoltSend&gt() == null) {
///     Debug.Log("BoltSend is missing!);
///     GameApplication.Shutdown(ErrorCode.Bolt);
///   }
/// }
/// ```
/// </example>
[BoltExecutionOrder(10000)]
public class BoltSend : MonoBehaviour {
  void Awake () {
    DontDestroyOnLoad(gameObject);
  }

  void FixedUpdate () {
    BoltCore._timer.Stop();
    BoltCore._timer.Reset();
    BoltCore._timer.Start();

    BoltCore.Send();

    BoltCore._timer.Stop();

    Bolt.DebugInfo.SendTime = (int)BoltCore._timer.ElapsedMilliseconds;
  }
}

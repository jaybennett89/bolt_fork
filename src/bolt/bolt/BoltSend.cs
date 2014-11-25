using System;
using UnityEngine;

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

using System;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

[BoltExecutionOrder(10000)]
public class BoltSend : MonoBehaviour {
  TimeSpan ts;

#if DEBUG
  Stopwatch sw = new Stopwatch();
#endif

  void Awake () {
    DontDestroyOnLoad(gameObject);
  }

  void FixedUpdate () {
#if DEBUG
    sw.Reset();
    sw.Start();
#endif

    BoltCore.Send();

#if DEBUG
    sw.Stop();
    ts = sw.Elapsed;
#endif
  }

#if DEBUG
  void OnDrawGizmos () {
    BoltPhysics.DrawSnapshot();
  }
#endif
}

using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {
  void Start() {
    BoltLauncher.StartServer();
    BoltLauncher.Shutdown();
    BoltLauncher.StartServer();
  }
}
 
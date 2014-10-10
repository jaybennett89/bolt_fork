using UnityEngine;
using System.Collections;

public class Tests : MonoBehaviour {
  bool set = false;

  // Use this for initialization
  void Start() {

  }

  // Update is called once per frame
  void Update() {

  }

  void FixedUpdate() {
    Animator an = GetComponent<Animator>();

    if (Time.time > 5 && !set) {
      set = true;
      an.SetTrigger("Fire");
    }

    Debug.Log(an.GetBool("Fire"));
  }
}

using UnityEngine;
using System.Collections;
using System;
using System.Threading;

public class BoltMethodRefs : MonoBehaviour {
	public static int integer;
	public static Action action;
	public static object obj;
	
	void Start() {
		Interlocked.CompareExchange (ref integer, 1, 1);
		Interlocked.CompareExchange (ref obj, null, null);
		Interlocked.CompareExchange <Action>(ref action, null, null);
	}
}
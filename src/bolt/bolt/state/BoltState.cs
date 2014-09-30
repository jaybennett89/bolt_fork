//using System;
//using UdpKit;
//using UnityEngine;

///// <summary>
///// Base interface for all bolt state objects
///// </summary>

public interface IBoltState {
  void PropertyChanged(IBoltStateProperty property);
}

//public abstract class BoltState : IBoltState, IBoltEntityCallbacks {
//  internal BoltEntity _entity = null;

//  public BoltEntity entity {
//    get { return _entity; }
//  }

//  public abstract Bits proxyMask { get; }
//  public abstract Bits controllerMask { get; }

//  public abstract bool Pack (BoltEntityUpdateInfo info, UdpStream stream, ref Bits mask);
//  public abstract void Read (BoltEntityUpdateInfo info, UdpStream stream);

//  public abstract void BeforeStep ();
//  public abstract void AfterStep ();
//  public abstract void Teleported ();
//  public abstract void UpdateRender ();
//  public abstract void OriginChanging (Transform old, Transform @new);

//  public abstract void Initialize ();
//  public abstract void PropertyChanged (IBoltStateProperty property);

//  protected static void TriggerChangedEvent (Action callback) {
//    if (callback != null) {
//      try {
//        callback();
//      } catch (Exception exn) {
//        BoltLog.Exception(exn);
//      }
//    }
//  }


//  protected static float FrameSmoothed (float from, float to, int fromFrame, int toFrame, int actualFrame) {
//    actualFrame = Mathf.Clamp(actualFrame, fromFrame, toFrame);

//    float d = toFrame - fromFrame;
//    float a = actualFrame - fromFrame;

//    return Mathf.Lerp(from, to, a / d);
//  }

//  protected static Vector2 FrameSmoothed (Vector2 from, Vector2 to, int fromFrame, int toFrame, int actualFrame) {
//    actualFrame = Mathf.Clamp(actualFrame, fromFrame, toFrame);

//    float d = toFrame - fromFrame;
//    float a = actualFrame - fromFrame;

//    return Vector2.Lerp(from, to, a / d);
//  }

//  protected static Vector3 FrameSmoothed (Vector3 from, Vector3 to, int fromFrame, int toFrame, int actualFrame) {
//    actualFrame = Mathf.Clamp(actualFrame, fromFrame, toFrame);

//    float d = toFrame - fromFrame;
//    float a = actualFrame - fromFrame;

//    return Vector3.Lerp(from, to, a / d);
//  }

//  protected static Vector4 FrameSmoothed (Vector4 from, Vector4 to, int fromFrame, int toFrame, int actualFrame) {
//    actualFrame = Mathf.Clamp(actualFrame, fromFrame, toFrame);

//    float d = toFrame - fromFrame;
//    float a = actualFrame - fromFrame;

//    return Vector4.Lerp(from, to, a / d);
//  }

//  protected static Quaternion FrameSmoothed (Quaternion from, Quaternion to, int fromFrame, int toFrame, int actualFrame) {
//    actualFrame = Mathf.Clamp(actualFrame, fromFrame, toFrame);

//    float d = toFrame - fromFrame;
//    float a = actualFrame - fromFrame;

//    return Quaternion.Lerp(from, to, a / d);
//  }
//}

//public abstract class BoltState<T> : BoltState where T : BoltStateFrame, new() {
//  protected T _recv;
//  protected BoltDoubleList<T> _buffer;

//  public BoltState () {
//    _buffer = new BoltDoubleList<T>();
//    _recv = new T();
//  }
//}

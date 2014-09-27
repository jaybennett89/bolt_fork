//using System;
//using UdpKit;

//public abstract class BoltEntitySerializer<T> : BoltEntitySerializer where T : class, IBoltState {
//  BoltState _state;

//  public override Bits proxyMask {
//    get { return _state.proxyMask; }
//  }

//  public override Bits controllerMask {
//    get { return _state.controllerMask; }
//  }

//  [Obsolete("Use BoltEntitySerializer<T>.state instead")]
//  public T boltState {
//    get { return state; }
//  }


//  public T state {
//    get { return (T) (object) _state; }
//  }

//  public sealed override void Attached () {
//#if DEBUG
//    if (typeof(T).IsInterface == false) {
//      throw new BoltException("type parameter to BoltEntitySerializerState<T> must be an interface");
//    }
//#endif

//    _state = (BoltState) (object) BoltFactory.NewState<T>();
//    _state._entity = entity;
//    _state.Initialize();

//    Attached((T) (object) _state);
//  }

//  public sealed override void BeforeStep () {
//    _state.BeforeStep();
//  }

//  public sealed override void AfterStep () {
//    _state.AfterStep();
//  }

//  public override void UpdateRender () {
//    _state.UpdateRender();
//  }

//  public override void Teleported () {
//    _state.Teleported();
//  }

//  public override void OriginChanging (UnityEngine.Transform old, UnityEngine.Transform @new) {
//    _state.OriginChanging(old, @new);
//  }

//  public sealed override void Pack (BoltEntityUpdateInfo info, UdpStream stream, ref Bits mask) {
//    _state.Pack(info, stream, ref mask);
//  }

//  public sealed override void Read (BoltEntityUpdateInfo info, UdpStream stream) {
//    _state.Read(info, stream);
//  }

//  public virtual void Attached (T state) {

//  }
//}

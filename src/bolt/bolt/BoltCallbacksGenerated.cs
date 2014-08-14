 
using UdpKit;
using UnityEngine;

partial class BoltCallbacksBase {
   
  public virtual void MapLoadBegin(string arg) { }
   
  public virtual void MapLoadDone(string arg) { }
   
  public virtual void Connected(BoltConnection arg) { }
   
  public virtual void Disconnected(BoltConnection arg) { }
   
  public virtual void ConnectRequest(UdpEndPoint arg) { }
   
  public virtual void ConnectFailed(UdpEndPoint arg) { }
   
  public virtual void ConnectRefused(UdpEndPoint arg) { }
   
  public virtual void ConnectedToServer(BoltConnection arg) { }
   
  public virtual void DisconnectedFromServer(BoltConnection arg) { }
   
  public virtual void ClientConnected(BoltConnection arg) { }
   
  public virtual void ClientDisconnected(BoltConnection arg) { }
   
  public virtual void EntityAttached(BoltEntity arg) { }
   
  public virtual void EntityDetached(BoltEntity arg) { }
   
  public virtual void ControlOfEntityGained(BoltEntity arg) { }
   
  public virtual void ControlOfEntityLost(BoltEntity arg) { }
   
  public virtual void ClientLoadedMap(BoltConnection arg) { }
   
  public virtual void ServerLoadedMap(BoltConnection arg) { }
   
  public virtual void StartBegin() { }
   
  public virtual void StartDone() { }
   
  public virtual void ShutdownBegin() { }
   

   
  internal static void MapLoadBeginInvoke(string arg) { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.MapLoadBegin(arg);
    }
  }
   
  internal static void MapLoadDoneInvoke(string arg) { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.MapLoadDone(arg);
    }
  }
   
  internal static void ConnectedInvoke(BoltConnection arg) { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.Connected(arg);
    }
  }
   
  internal static void DisconnectedInvoke(BoltConnection arg) { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.Disconnected(arg);
    }
  }
   
  internal static void ConnectRequestInvoke(UdpEndPoint arg) { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.ConnectRequest(arg);
    }
  }
   
  internal static void ConnectFailedInvoke(UdpEndPoint arg) { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.ConnectFailed(arg);
    }
  }
   
  internal static void ConnectRefusedInvoke(UdpEndPoint arg) { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.ConnectRefused(arg);
    }
  }
   
  internal static void ConnectedToServerInvoke(BoltConnection arg) { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.ConnectedToServer(arg);
    }
  }
   
  internal static void DisconnectedFromServerInvoke(BoltConnection arg) { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.DisconnectedFromServer(arg);
    }
  }
   
  internal static void ClientConnectedInvoke(BoltConnection arg) { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.ClientConnected(arg);
    }
  }
   
  internal static void ClientDisconnectedInvoke(BoltConnection arg) { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.ClientDisconnected(arg);
    }
  }
   
  internal static void EntityAttachedInvoke(BoltEntity arg) { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.EntityAttached(arg);
    }
  }
   
  internal static void EntityDetachedInvoke(BoltEntity arg) { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.EntityDetached(arg);
    }
  }
   
  internal static void ControlOfEntityGainedInvoke(BoltEntity arg) { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.ControlOfEntityGained(arg);
    }
  }
   
  internal static void ControlOfEntityLostInvoke(BoltEntity arg) { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.ControlOfEntityLost(arg);
    }
  }
   
  internal static void ClientLoadedMapInvoke(BoltConnection arg) { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.ClientLoadedMap(arg);
    }
  }
   
  internal static void ServerLoadedMapInvoke(BoltConnection arg) { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.ServerLoadedMap(arg);
    }
  }
   
  internal static void StartBeginInvoke() { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.StartBegin();
    }
  }
   
  internal static void StartDoneInvoke() { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.StartDone();
    }
  }
   
  internal static void ShutdownBeginInvoke() { 
    foreach (BoltCallbacksBase cb in callbacks) {
      cb.ShutdownBegin();
    }
  }
   
}

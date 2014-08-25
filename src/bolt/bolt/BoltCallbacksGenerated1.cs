 
using UdpKit;
using UnityEngine;

partial class BoltCallbacksBase {
   
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
   

  
  public virtual void MapLoadLocalBegin(string map) { 
  }

  internal static void MapLoadLocalBeginInvoke(string map) { 
    foreach (BoltCallbacksBase cb in callbacks) {
        cb.MapLoadLocalBegin(map);
    }
  }

  
  public virtual void MapLoadLocalDone(string map) { 
  }

  internal static void MapLoadLocalDoneInvoke(string map) { 
    foreach (BoltCallbacksBase cb in callbacks) {
        cb.MapLoadLocalDone(map);
    }
  }

  
  public virtual void MapLoadRemoteDone(string map, BoltConnection connection) { 
  }

  internal static void MapLoadRemoteDoneInvoke(string map, BoltConnection connection) { 
    foreach (BoltCallbacksBase cb in callbacks) {
        cb.MapLoadRemoteDone(map, connection);
    }
  }

  }

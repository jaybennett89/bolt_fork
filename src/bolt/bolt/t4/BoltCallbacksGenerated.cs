 
using UdpKit;
using UnityEngine;

partial class BoltGlobalEventListenerBase {
   
  public virtual void ConnectFailed(UdpEndPoint arg) { }
   
  public virtual void ConnectRefused(UdpEndPoint arg) { }
   
  public virtual void EntityAttached(BoltEntity arg) { }
   
  public virtual void EntityDetached(BoltEntity arg) { }
   
  public virtual void ControlOfEntityGained(BoltEntity arg) { }
   
  public virtual void ControlOfEntityLost(BoltEntity arg) { }
   

   
  internal static void ConnectFailedInvoke(UdpEndPoint arg) { 
    BoltLog.Debug("Invoking callback ConnectFailed");
    foreach (BoltGlobalEventListenerBase cb in callbacks) {
      cb.ConnectFailed(arg);
    }
  }
   
  internal static void ConnectRefusedInvoke(UdpEndPoint arg) { 
    BoltLog.Debug("Invoking callback ConnectRefused");
    foreach (BoltGlobalEventListenerBase cb in callbacks) {
      cb.ConnectRefused(arg);
    }
  }
   
  internal static void EntityAttachedInvoke(BoltEntity arg) { 
    BoltLog.Debug("Invoking callback EntityAttached");
    foreach (BoltGlobalEventListenerBase cb in callbacks) {
      cb.EntityAttached(arg);
    }
  }
   
  internal static void EntityDetachedInvoke(BoltEntity arg) { 
    BoltLog.Debug("Invoking callback EntityDetached");
    foreach (BoltGlobalEventListenerBase cb in callbacks) {
      cb.EntityDetached(arg);
    }
  }
   
  internal static void ControlOfEntityGainedInvoke(BoltEntity arg) { 
    BoltLog.Debug("Invoking callback ControlOfEntityGained");
    foreach (BoltGlobalEventListenerBase cb in callbacks) {
      cb.ControlOfEntityGained(arg);
    }
  }
   
  internal static void ControlOfEntityLostInvoke(BoltEntity arg) { 
    BoltLog.Debug("Invoking callback ControlOfEntityLost");
    foreach (BoltGlobalEventListenerBase cb in callbacks) {
      cb.ControlOfEntityLost(arg);
    }
  }
   

  
  public virtual void SceneLoadLocalBegin(string map) { 
  }

  internal static void SceneLoadLocalBeginInvoke(string map) { 
    BoltLog.Debug("Invoking callback SceneLoadLocalBegin");
    foreach (BoltGlobalEventListenerBase cb in callbacks) {
        cb.SceneLoadLocalBegin(map);
    }
  }

  
  public virtual void SceneLoadLocalDone(string map) { 
  }

  internal static void SceneLoadLocalDoneInvoke(string map) { 
    BoltLog.Debug("Invoking callback SceneLoadLocalDone");
    foreach (BoltGlobalEventListenerBase cb in callbacks) {
        cb.SceneLoadLocalDone(map);
    }
  }

  
  public virtual void SceneLoadRemoteDone(BoltConnection connection) { 
  }

  internal static void SceneLoadRemoteDoneInvoke(BoltConnection connection) { 
    BoltLog.Debug("Invoking callback SceneLoadRemoteDone");
    foreach (BoltGlobalEventListenerBase cb in callbacks) {
        cb.SceneLoadRemoteDone(connection);
    }
  }

  
  public virtual void Connected(BoltConnection connection) { 
  }

  internal static void ConnectedInvoke(BoltConnection connection) { 
    BoltLog.Debug("Invoking callback Connected");
    foreach (BoltGlobalEventListenerBase cb in callbacks) {
        cb.Connected(connection);
    }
  }

  
  public virtual void Disconnected(BoltConnection connection) { 
  }

  internal static void DisconnectedInvoke(BoltConnection connection) { 
    BoltLog.Debug("Invoking callback Disconnected");
    foreach (BoltGlobalEventListenerBase cb in callbacks) {
        cb.Disconnected(connection);
    }
  }

  
  public virtual void ConnectRequest(UdpEndPoint endpoint, byte[] token) { 
  }

  internal static void ConnectRequestInvoke(UdpEndPoint endpoint, byte[] token) { 
    BoltLog.Debug("Invoking callback ConnectRequest");
    foreach (BoltGlobalEventListenerBase cb in callbacks) {
        cb.ConnectRequest(endpoint, token);
    }
  }

  }

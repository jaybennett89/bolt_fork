 
using UdpKit;
using UnityEngine;

namespace BoltInternal {

partial class GlobalEventListenerBase {
   
  public virtual void ConnectFailed(UdpEndPoint arg) { }
   
  public virtual void EntityAttached(BoltEntity arg) { }
   
  public virtual void EntityDetached(BoltEntity arg) { }
   
  public virtual void ControlOfEntityGained(BoltEntity arg) { }
   
  public virtual void ControlOfEntityLost(BoltEntity arg) { }
   

   
  internal static void ConnectFailedInvoke(UdpEndPoint arg) { 
    BoltLog.Debug("Invoking callback ConnectFailed");
    foreach (GlobalEventListenerBase cb in callbacks) {
      cb.ConnectFailed(arg);
    }
  }
   
  internal static void EntityAttachedInvoke(BoltEntity arg) { 
    BoltLog.Debug("Invoking callback EntityAttached");
    foreach (GlobalEventListenerBase cb in callbacks) {
      cb.EntityAttached(arg);
    }
  }
   
  internal static void EntityDetachedInvoke(BoltEntity arg) { 
    BoltLog.Debug("Invoking callback EntityDetached");
    foreach (GlobalEventListenerBase cb in callbacks) {
      cb.EntityDetached(arg);
    }
  }
   
  internal static void ControlOfEntityGainedInvoke(BoltEntity arg) { 
    BoltLog.Debug("Invoking callback ControlOfEntityGained");
    foreach (GlobalEventListenerBase cb in callbacks) {
      cb.ControlOfEntityGained(arg);
    }
  }
   
  internal static void ControlOfEntityLostInvoke(BoltEntity arg) { 
    BoltLog.Debug("Invoking callback ControlOfEntityLost");
    foreach (GlobalEventListenerBase cb in callbacks) {
      cb.ControlOfEntityLost(arg);
    }
  }
   

  
  public virtual void BoltShutdown() { 
  }

  internal static void BoltShutdownInvoke() { 
    BoltLog.Debug("Invoking callback BoltShutdown");
    foreach (GlobalEventListenerBase cb in callbacks) {
        cb.BoltShutdown();
    }
  }

  
  public virtual void BoltStarted() { 
  }

  internal static void BoltStartedInvoke() { 
    BoltLog.Debug("Invoking callback BoltStarted");
    foreach (GlobalEventListenerBase cb in callbacks) {
        cb.BoltStarted();
    }
  }

  
  public virtual void SceneLoadLocalBegin(string map) { 
  }

  internal static void SceneLoadLocalBeginInvoke(string map) { 
    BoltLog.Debug("Invoking callback SceneLoadLocalBegin");
    foreach (GlobalEventListenerBase cb in callbacks) {
        cb.SceneLoadLocalBegin(map);
    }
  }

  
  public virtual void SceneLoadLocalDone(string map) { 
  }

  internal static void SceneLoadLocalDoneInvoke(string map) { 
    BoltLog.Debug("Invoking callback SceneLoadLocalDone");
    foreach (GlobalEventListenerBase cb in callbacks) {
        cb.SceneLoadLocalDone(map);
    }
  }

  
  public virtual void SceneLoadRemoteDone(BoltConnection connection) { 
  }

  internal static void SceneLoadRemoteDoneInvoke(BoltConnection connection) { 
    BoltLog.Debug("Invoking callback SceneLoadRemoteDone");
    foreach (GlobalEventListenerBase cb in callbacks) {
        cb.SceneLoadRemoteDone(connection);
    }
  }

  
  public virtual void ConnectAttempt(UdpKit.UdpEndPoint endpoint) { 
  }

  internal static void ConnectAttemptInvoke(UdpKit.UdpEndPoint endpoint) { 
    BoltLog.Debug("Invoking callback ConnectAttempt");
    foreach (GlobalEventListenerBase cb in callbacks) {
        cb.ConnectAttempt(endpoint);
    }
  }

  
  public virtual void Connected(BoltConnection connection, Bolt.IProtocolToken token) { 
  }

  internal static void ConnectedInvoke(BoltConnection connection, Bolt.IProtocolToken token) { 
    BoltLog.Debug("Invoking callback Connected");
    foreach (GlobalEventListenerBase cb in callbacks) {
        cb.Connected(connection, token);
    }
  }

  
  public virtual void Disconnected(BoltConnection connection) { 
  }

  internal static void DisconnectedInvoke(BoltConnection connection) { 
    BoltLog.Debug("Invoking callback Disconnected");
    foreach (GlobalEventListenerBase cb in callbacks) {
        cb.Disconnected(connection);
    }
  }

  
  public virtual void ConnectRequest(UdpEndPoint endpoint, Bolt.IProtocolToken token) { 
  }

  internal static void ConnectRequestInvoke(UdpEndPoint endpoint, Bolt.IProtocolToken token) { 
    BoltLog.Debug("Invoking callback ConnectRequest");
    foreach (GlobalEventListenerBase cb in callbacks) {
        cb.ConnectRequest(endpoint, token);
    }
  }

  
  public virtual void ConnectRefused(UdpEndPoint endpoint, Bolt.IProtocolToken token) { 
  }

  internal static void ConnectRefusedInvoke(UdpEndPoint endpoint, Bolt.IProtocolToken token) { 
    BoltLog.Debug("Invoking callback ConnectRefused");
    foreach (GlobalEventListenerBase cb in callbacks) {
        cb.ConnectRefused(endpoint, token);
    }
  }

  
  public virtual void EntityReceived(BoltEntity entity) { 
  }

  internal static void EntityReceivedInvoke(BoltEntity entity) { 
    BoltLog.Debug("Invoking callback EntityReceived");
    foreach (GlobalEventListenerBase cb in callbacks) {
        cb.EntityReceived(entity);
    }
  }

  
  public virtual void PortMappingChanged(Bolt.INatDevice device, Bolt.IPortMapping portMapping) { 
  }

  internal static void PortMappingChangedInvoke(Bolt.INatDevice device, Bolt.IPortMapping portMapping) { 
    BoltLog.Debug("Invoking callback PortMappingChanged");
    foreach (GlobalEventListenerBase cb in callbacks) {
        cb.PortMappingChanged(device, portMapping);
    }
  }

  }
}
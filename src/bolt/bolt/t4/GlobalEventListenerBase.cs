 
using UdpKit;
using UnityEngine;

namespace BoltInternal {

partial class GlobalEventListenerBase {
  
  public virtual void BoltShutdown() {  }

  internal static void BoltShutdownInvoke() { 
    BoltLog.Debug("Invoking callback BoltShutdown");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.BoltShutdown();
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void BoltStarted() {  }

  internal static void BoltStartedInvoke() { 
    BoltLog.Debug("Invoking callback BoltStarted");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.BoltStarted();
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void StreamDataReceived(BoltConnection connection, UdpStreamData data) {  }

  internal static void StreamDataReceivedInvoke(BoltConnection connection, UdpStreamData data) { 
    BoltLog.Debug("Invoking callback StreamDataReceived");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.StreamDataReceived(connection, data);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void PortMappingChanged(Bolt.INatDevice device, Bolt.IPortMapping portMapping) {  }

  internal static void PortMappingChangedInvoke(Bolt.INatDevice device, Bolt.IPortMapping portMapping) { 
    BoltLog.Debug("Invoking callback PortMappingChanged");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.PortMappingChanged(device, portMapping);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void SceneLoadLocalBegin(string map) {  }

  internal static void SceneLoadLocalBeginInvoke(string map) { 
    BoltLog.Debug("Invoking callback SceneLoadLocalBegin");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.SceneLoadLocalBegin(map);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void SceneLoadLocalDone(string map) {  }

  internal static void SceneLoadLocalDoneInvoke(string map) { 
    BoltLog.Debug("Invoking callback SceneLoadLocalDone");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.SceneLoadLocalDone(map);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void SceneLoadRemoteDone(BoltConnection connection) {  }

  internal static void SceneLoadRemoteDoneInvoke(BoltConnection connection) { 
    BoltLog.Debug("Invoking callback SceneLoadRemoteDone");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.SceneLoadRemoteDone(connection);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void ConnectAttempt(UdpKit.UdpEndPoint endpoint) {  }

  internal static void ConnectAttemptInvoke(UdpKit.UdpEndPoint endpoint) { 
    BoltLog.Debug("Invoking callback ConnectAttempt");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ConnectAttempt(endpoint);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void Connected(BoltConnection connection) {  }

  internal static void ConnectedInvoke(BoltConnection connection) { 
    BoltLog.Debug("Invoking callback Connected");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.Connected(connection);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void Connected(BoltConnection connection, Bolt.IProtocolToken acceptToken) {  }

  internal static void ConnectedInvoke(BoltConnection connection, Bolt.IProtocolToken acceptToken) { 
    BoltLog.Debug("Invoking callback Connected");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.Connected(connection, acceptToken);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void Connected(BoltConnection connection, Bolt.IProtocolToken acceptToken, Bolt.IProtocolToken connectToken) {  }

  internal static void ConnectedInvoke(BoltConnection connection, Bolt.IProtocolToken acceptToken, Bolt.IProtocolToken connectToken) { 
    BoltLog.Debug("Invoking callback Connected");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.Connected(connection, acceptToken, connectToken);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void ConnectFailed(UdpEndPoint endpoint) {  }

  internal static void ConnectFailedInvoke(UdpEndPoint endpoint) { 
    BoltLog.Debug("Invoking callback ConnectFailed");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ConnectFailed(endpoint);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void ConnectRequest(UdpEndPoint endpoint) {  }

  internal static void ConnectRequestInvoke(UdpEndPoint endpoint) { 
    BoltLog.Debug("Invoking callback ConnectRequest");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ConnectRequest(endpoint);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void ConnectRequest(UdpEndPoint endpoint, Bolt.IProtocolToken token) {  }

  internal static void ConnectRequestInvoke(UdpEndPoint endpoint, Bolt.IProtocolToken token) { 
    BoltLog.Debug("Invoking callback ConnectRequest");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ConnectRequest(endpoint, token);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void ConnectRefused(UdpEndPoint endpoint) {  }

  internal static void ConnectRefusedInvoke(UdpEndPoint endpoint) { 
    BoltLog.Debug("Invoking callback ConnectRefused");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ConnectRefused(endpoint);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void ConnectRefused(UdpEndPoint endpoint, Bolt.IProtocolToken token) {  }

  internal static void ConnectRefusedInvoke(UdpEndPoint endpoint, Bolt.IProtocolToken token) { 
    BoltLog.Debug("Invoking callback ConnectRefused");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ConnectRefused(endpoint, token);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void Disconnected(BoltConnection connection) {  }

  internal static void DisconnectedInvoke(BoltConnection connection) { 
    BoltLog.Debug("Invoking callback Disconnected");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.Disconnected(connection);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void Disconnected(BoltConnection connection, Bolt.IProtocolToken token) {  }

  internal static void DisconnectedInvoke(BoltConnection connection, Bolt.IProtocolToken token) { 
    BoltLog.Debug("Invoking callback Disconnected");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.Disconnected(connection, token);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void ControlOfEntityLost(BoltEntity entity) {  }

  internal static void ControlOfEntityLostInvoke(BoltEntity entity) { 
    BoltLog.Debug("Invoking callback ControlOfEntityLost");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ControlOfEntityLost(entity);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void ControlOfEntityGained(BoltEntity entity) {  }

  internal static void ControlOfEntityGainedInvoke(BoltEntity entity) { 
    BoltLog.Debug("Invoking callback ControlOfEntityGained");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ControlOfEntityGained(entity);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void ControlOfEntityLost(BoltEntity entity, Bolt.IProtocolToken token) {  }

  internal static void ControlOfEntityLostInvoke(BoltEntity entity, Bolt.IProtocolToken token) { 
    BoltLog.Debug("Invoking callback ControlOfEntityLost");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ControlOfEntityLost(entity, token);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void ControlOfEntityGained(BoltEntity entity, Bolt.IProtocolToken token) {  }

  internal static void ControlOfEntityGainedInvoke(BoltEntity entity, Bolt.IProtocolToken token) { 
    BoltLog.Debug("Invoking callback ControlOfEntityGained");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ControlOfEntityGained(entity, token);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void EntityAttached(BoltEntity entity) {  }

  internal static void EntityAttachedInvoke(BoltEntity entity) { 
    BoltLog.Debug("Invoking callback EntityAttached");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.EntityAttached(entity);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void EntityDetached(BoltEntity entity) {  }

  internal static void EntityDetachedInvoke(BoltEntity entity) { 
    BoltLog.Debug("Invoking callback EntityDetached");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.EntityDetached(entity);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void EntityAttached(BoltEntity entity, Bolt.IProtocolToken token) {  }

  internal static void EntityAttachedInvoke(BoltEntity entity, Bolt.IProtocolToken token) { 
    BoltLog.Debug("Invoking callback EntityAttached");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.EntityAttached(entity, token);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void EntityDetached(BoltEntity entity, Bolt.IProtocolToken token) {  }

  internal static void EntityDetachedInvoke(BoltEntity entity, Bolt.IProtocolToken token) { 
    BoltLog.Debug("Invoking callback EntityDetached");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.EntityDetached(entity, token);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  
  public virtual void EntityReceived(BoltEntity entity) {  }

  internal static void EntityReceivedInvoke(BoltEntity entity) { 
    BoltLog.Debug("Invoking callback EntityReceived");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.EntityReceived(entity);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  }
}
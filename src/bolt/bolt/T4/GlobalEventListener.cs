 

using UdpKit;
using UnityEngine;

namespace BoltInternal {

partial class GlobalEventListenerBase {
  
  /// <summary>
  /// Callback triggered when the bolt simulation is shutting down.
  /// </summary>
  /// <example>
  /// *Example:* Showing a message to clients when the server has shut down unexpectedly.
  /// 
  /// ```csharp
  /// public override void BoltShutdown() {
  ///   Message.Show("Error", "Server Shut Down!");
  /// }
  /// ```
  /// </example>
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

  /// <summary>
  /// Callback triggered when the bolt simulation is starting.
  /// </summary>
  /// <example>
  /// *Example:* Showing a meessage when the bolt simulation is starting and initializing some NPC data.
  /// ```csharp
  /// public override void BoltStarted() {
  ///   PrecalcNpcPaths();
  ///   Message.Show("Starting Game...");
  /// }
  /// ```
  /// </example>
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

  /// <summary>
  /// Callback triggered when binary stream data is received 
  /// </summary>
  /// <param name="connection">The sender connection</param>
  /// <param name="data">The binary stream data</param>
  /// <example>
  /// *Example:* Receiving a custom player icon.
  /// 
  /// ```csharp
  /// public override void StreamDataReceived(BoltConnection connnection, UdpStreamData data) { 
  ///   Texture2D icon = new Texture2D(4, 4);
  ///   icon.LoadImage(data.Data);
  ///   
  ///   PlayerData playerData = (PlayerData)connection.userToken;
  ///   playerData.SetIcon(icon);
  /// }
  /// ```
  /// </example>
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

  /// <summary>
  /// Registering binary stream channels may only be done in this callback
  /// </summary>
  /// <example>
  /// *Example:* Creating an unreliable stream channel for voice and a reliable stream channel for sending custom player icons.
  /// 
  /// ```csharp
  /// public static UdpKit.UdpChannelName Voice;
  /// public static UdpKit.UdpChannelName CustomPlayerIcon;
  /// 
  /// public override void RegisterStreamChannels() {
  ///   Voice = BoltNetwork.CreateStreamChannel("Voice", UdpKit.UdpChannelMode.Unreliable, 1});
  ///   CustomPlayerIcon = BoltNetwork.CreateStreamChannel("CustomPlayerIcon", UdpKit.UdpChannelMode.Reliable, 4});
  /// }
  /// ```
  /// </example>
  public virtual void RegisterStreamChannels() {  }

  internal static void RegisterStreamChannelsInvoke() { 
    BoltLog.Debug("Invoking callback RegisterStreamChannels");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.RegisterStreamChannels();
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

  /// <summary>
  /// Callback triggered before the new local scene is loaded
  /// </summary>
  /// <param name="map">Name of scene being loaded</param>
  /// <example>
  /// *Example:* Showing a splash screen when clients are loading the game scene.
  /// 
  /// ```csharp
  /// public override void SceneLoadLocalBegin(string map) {
  ///   if(BoltNetwork.isClient && map.Equals("GameScene") {
  ///     SplashScreen.Show(SplashScreens.GameLoad);
  ///   }
  /// }
  /// ```
  /// </example>
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

  /// <summary>
  /// Callback triggered before the new local scene has been completely loaded
  /// </summary>
  /// <param name="map">Name of scene that has loaded</param>
  /// <example>
  /// *Example:* Hiding a splash screen that was shown during loading.
  /// 
  /// ```csharp
  /// public override void SceneLoadLocalBegin(string map) {
  ///   if(BoltNetwork.isClient && map.Equals("GameScene") {
  ///     SplashScreen.Hide();
  ///   }
  /// }
  /// ```
  /// </example>
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

  /// <summary>
  /// Callback triggered when a remote connection has completely loaded the current scene
  /// </summary>
  /// <param name="connection">The remote connection</param>
  /// <example>
  /// *Example:* Instantiating and configuring a player entity on the server and then assigning control to the client.
  /// 
  /// ```csharp
  /// public override void SceneLoadRemoteDone(BoltConnection connection) {
  ///   var player = BoltNetwork.Instantiate(BoltPrefabs.Player);
  ///   player.transform.position = spawnPoint.transform.position;
  ///   
  ///   var initData = prototype.GetNewPlayer(GameLogic.PlayableClass.Mercenary);
  ///   Configure(player, initData);
  ///   
  ///   player.AssignControl(connection);
  /// }
  /// ```
  /// </example>
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

  /// <summary>
  /// Callback triggered when a client has become connected to this instance
  /// </summary>
  /// <param name="connection">The connected client endpoint</param>
  /// <example>
  /// *Example:* Instantiating and configuring a player entity when a client connects to the server.
  /// 
  /// ```csharp
  /// public override void Connected(BoltConnection connection) {
  ///   var player = BoltNetwork.Instantiate(BoltPrefabs.Player);
  ///   player.transform.position = spawnPoint.transform.position;
  ///   
  ///   var initData = prototype.GetNewPlayer(GameLogic.PlayableClass.Mercenary);
  ///   Configure(player, initData);
  ///   
  ///   player.AssignControl(connection);
  /// }
  /// ```
  /// </example>
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

  /// <summary>
  /// Callback triggered when a client has become connected to this instance
  /// </summary>
  /// <param name="connection">The connected client endpoint</param>
  /// <param name="acceptToken">A data token sent in the accept method</param>
  /// <example>
  /// *Example:* Using a protocol token with the Connected callback to pass a spawnpoint position to to new player entity.
  /// 
  /// ```csharp
  /// public override void Connected(BoltConnection connection, Bolt.IProtocolToken acceptToken) {
  ///   var player = BoltNetwork.Instantiate(BoltPrefabs.Player);
  ///   
  ///   var spawnPoint = (SpawnPoint)acceptToken;
  ///   player.transform.position = spawnPoint.position;
  ///   
  ///   var initData = prototype.GetNewPlayer(GameLogic.PlayableClass.Mercenary);
  ///   Configure(player, initData);
  ///   
  ///   player.AssignControl(connection);
  /// }
  /// ```
  /// </example>
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

  /// <summary>
  /// Callback triggered when a connection to remote server has failed
  /// </summary>
  /// <param name="endpoint">The remote address</param>
  /// <example>
  /// *Example:* Showing an error message when the remote connection has failed.
  /// 
  /// ```csharp
  /// public override void ConnectFailed(UdpEndPoint endpoint) {
  ///   Message.Show("Error", string.Format("Connection To ({0}:{1}) has failed", endpoint.Address.ToString(), endpoint.ToString());
  /// }
  /// ```
  /// </example>
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

  /// <summary>
  /// Callback triggered when this instance receives an incoming client connection
  /// </summary>
  /// <param name="endpoint">The incoming client endpoint</param>
  /// <example>
  /// Accepting an incoming connection.
  /// 
  /// ```csharp
  /// public override void ConnectRequest(UdpEndPoint endpoint) {
  ///   BoltNetwork.Accept(remoteEndPoint);
  /// }
  /// ```
  /// </example> 
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

  /// <summary>
  /// Callback triggered when this instance receives an incoming client connection
  /// </summary>
  /// <param name="endpoint">The incoming client endpoint</param>
  /// <param name="token">A data token sent from the incoming client</param>
  /// <example>
  /// *Example:* Accepting an incoming connection with user credentials in the data token.
  /// 
  /// ```csharp
  /// public override void ConnectRequest(UdpEndPoint endpoint, Bolt.IProtocolToken token) {
  ///   UserCredentials creds = (UserCredentials)token);
  ///   if(Authenticate(creds.username, creds.password)) {
  ///     BoltNetwork.Accept(connection.remoteEndPoint);
  ///   }
  /// }
  /// ```
  /// </example> 
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

  /// <summary>
  /// Callback triggered when the connection to a remote server has been refused.
  /// </summary>
  /// <param name="endpoint">The remote server endpoint</param>
  /// <example>
  /// *Example:* Showing an error message when the remote connection has been refused.
  /// 
  /// ```csharp
  /// public override void ConnectRefused(UdpEndPoint endpoint) {
  ///   Message.Show("Error", string.Format("Connection To ({0}:{1}) has been refused", endpoint.Address.ToString(), endpoint.Port.ToString());
  /// }
  /// ```
  /// </example>
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

  /// <summary>
  /// Callback triggered when the connection to a remote server has been refused.
  /// </summary>
  /// <param name="endpoint">The remote server endpoint</param>
  /// <param name="token">Data token sent by refusing server</param>
  /// <example>
  /// *Example:* Showing an error message when the remote connection has been refused using an error message token from the server.
  /// 
  /// ```csharp
  /// public override void ConnectRefused(UdpEndPoint endpoint, Bolt.IProtocolToken token) {
  ///   ServerMessage.message = (ServerMessage)token;
  ///   Message.Show(serverMessage.error, string.Format("Connection To ({0}:{1}) has been refused. Reason was {3}", 
  ///     endpoint.Address.ToString(), endpoint.ToString(), serverMessage.errorDescription);
  /// }
  /// ```
  /// </example> 
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

  /// <summary>
  /// Callback triggered when trying to connect to a remote endpoint
  /// </summary>
  /// <param name="endpoint">The remote server address</param>
  /// <example>
  /// *Example:* Displaying a message to clients when initializing a connection to server.
  /// 
  /// ```csharp
  /// public override void ConnectAttempt((UdpEndPoint endpoint) {
  ///   Message.Show("Connecting ...", string.Format("To Remote Server At ({0}:{1}), endpoint.Address, endpoint.Port);
  /// }
  /// ```
  /// </example>
  public virtual void ConnectAttempt(UdpEndPoint endpoint) {  }

  internal static void ConnectAttemptInvoke(UdpEndPoint endpoint) { 
    BoltLog.Debug("Invoking callback ConnectAttempt");
    foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ConnectAttempt(endpoint);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
    }
  }

  /// <summary>
  /// Callback triggered when disconnected from remote server
  /// </summary>
  /// <param name="connection">The remote server endpoint</param>
  /// <example>
  /// *Example:* Displaying a disconnect message to the client and returning to the main menu scene.
  /// 
  /// ```csharp
  /// public override void Disconnected(BoltConnection connection) {
  ///   Message.Show("Disconnected", "Returning to main menu...");
  ///   Application.LoadLevel("MainMenu");
  /// }
  /// ```
  /// </example>
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

  /// <summary>
  /// Callback triggered when disconnected from remote server
  /// </summary>
  /// <param name="connection">The remote server endpoint</param>
  /// <param name="token">Data token sent by disconnecting server</param>
  /// <example>
  /// *Example:* Displaying a custom disconnect message to the client.
  /// 
  /// ```csharp
  /// public override void Disconnected(BoltConnection connection, Bolt.IProtocolToken token) {
  ///   ServerMessage msg = (ServerMessage)token;  /// 
  ///   Message.Show(msg.error, msg.description);
  ///   Application.LoadLevel("MainMenu");
  /// }
  /// ```
  /// </example>
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

  /// <summary>
  /// Callback triggered when this instance of bolt loses control of a bolt entity
  /// </summary>
  /// <param name="entity">The controlled entity</param>
  /// <example>
  /// *Example:* Setting up game components to no longer control an entity.
  /// 
  /// ```csharp
  /// public override void ControlOfEntityLost(BoltEntity entity) {
  ///   GameInput.instance.RemoveControlledEntity(entity);
  ///   MiniMap.instance.RemoveControlledEntity(entity);
  /// }
  /// ```
  /// </example>
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

  /// <summary>
  /// Callback triggered when this instance of bolt loses control of a bolt entity
  /// </summary>
  /// <param name="entity">The controlled entity</param>
  /// <param name="token">Data token sent by the owner</param>
  /// <example>
  /// *Example:* Removing the player entity from being controlled by game components and showing a message explaining 
  /// the reason for losing control.
  /// 
  /// ```csharp
  /// public override void ControlOfEntityLost(BoltEntity entity, Bolt.IProtocolToken token) {
  ///   GameInput.instance.RemoveControlledEntity(entity);
  ///   MiniMap.instance.RemoveControlledEntity(entity);
  ///   
  ///   ServerMessage msg = (ServerMessage)token;
  ///   Message.Show(msg.error, msg.description);
  /// }
  /// ```
  /// </example>
  public virtual void ControlOfEntityLost(BoltEntity entity, Bolt.IProtocolToken token) { }

  internal static void ControlOfEntityLostInvoke(BoltEntity entity, Bolt.IProtocolToken token) {
    BoltLog.Debug("Invoking callback ControlOfEntityLost");
    foreach (GlobalEventListenerBase cb in callbacks) {
      try {
        cb.ControlOfEntityLost(entity, token);
      }
      catch (System.Exception exn) {
        BoltLog.Exception(exn);
      }
    }
  }

  /// <summary>
  /// Callback triggered when this instance of bolt receieves control of a bolt entity
  /// </summary>
  /// <param name="entity">The controlled entity</param>
  /// <example>
  /// *Example:* Setting up the game minimap and other components to use a specific entity as the player's controlled entity.
  /// 
  /// ```csharp
  /// public override void ControlOfEntityGained(BoltEntity entity) {
  ///   GameInput.instance.SetControlledEntity(entity);
  ///   MiniMap.instance.SetControlledEntity(entity);
  /// }
  /// ```
  /// </example>
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

  /// <summary>
  /// Callback triggered when this instance of bolt receieves control of a bolt entity
  /// </summary>
  /// <param name="entity">The controlled entity</param>
  /// <param name="token">Data token sent by the owner</param>
  /// <example>
  /// *Example:* Setting up the game minimap and other components to use a specific entity as the player's controlled entity and using a data token to 
  /// transmit the correct team color.
  /// 
  /// ```csharp
  /// public override void ControlOfEntityGained(BoltEntity entity, IProtocolToken token) {
  ///   GameInput.instance.SetControlledEntity(entity);
  ///   
  ///   PlayerInfo info = (PlayerInfo)token;  
  ///   MiniMap.instance.SetControlledEntity(entity, info.teamColor);
  /// }
  /// ```
  /// </example>
  public virtual void ControlOfEntityGained(BoltEntity entity, Bolt.IProtocolToken token) { }

  internal static void ControlOfEntityGainedInvoke(BoltEntity entity, Bolt.IProtocolToken token) {
    BoltLog.Debug("Invoking callback ControlOfEntityGained");
    foreach (GlobalEventListenerBase cb in callbacks) {
      try {
        cb.ControlOfEntityGained(entity, token);
      }
      catch (System.Exception exn) {
        BoltLog.Exception(exn);
      }
    }
  }

  /// <summary>
  /// Callback triggered when a new entity is attached to the bolt simulation
  /// </summary>
  /// <param name="entity">The attached entity</param>
  /// <example>
  /// *Example:* Setting up the game minimap to show a newly attached entity.
  /// 
  /// ```csharp
  /// public override void EntityAttached(BoltEntity entity) {
  ///   MiniMap.instance.SetKnownEntity(entity);
  /// }
  /// ```
  /// </example> 
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

  /// <summary>
  /// Callback triggered when a new entity is attached to the bolt simulation
  /// </summary>
  /// <param name="entity">The attached entity</param>
  /// <param name="token">Data token sent by the owner</param>
  /// <example>
  /// *Example:* Initializing a newly attached entity with loadout data for weapon, armor, and abilities.
  /// 
  /// ```csharp
  /// public override void EntityAttached(BoltEntity entity, Bolt.IProtocolToken token) {
  ///   MiniMap.instance.SetKnownEntity(entity);
  ///   
  ///   EntityLoadout loadout = (EntityLoadout)token;
  ///   ConfigureEntity(entity, loadout.weaponId, loadout.armorId, loadout.abilities);
  /// }
  /// ```
  /// </example>
  public virtual void EntityAttached(BoltEntity entity, Bolt.IProtocolToken token) { }

  internal static void EntityAttachedInvoke(BoltEntity entity, Bolt.IProtocolToken token) {
    BoltLog.Debug("Invoking callback EntityAttached");
    foreach (GlobalEventListenerBase cb in callbacks) {
      try {
        cb.EntityAttached(entity, token);
      }
      catch (System.Exception exn) {
        BoltLog.Exception(exn);
      }
    }
  }

  /// <summary>
  /// Callback triggered when a new entity is detached from the bolt simulation
  /// </summary>
  /// <param name="entity">The detached entity</param>
  /// <example>
  /// *Example:* Removing the newly detached entity from the game minimap.
  /// 
  /// ```csharp
  /// public override void EntityDetached(BoltEntity entity) {
  ///   MiniMap.instance.RemoveKnownEntity(entity);
  /// }
  /// ```
  /// </example>
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

  /// <summary>
  /// Callback triggered when a new entity is detached from the bolt simulation
  /// </summary>
  /// <param name="entity">The detached entity</param>
  /// <param name="token">Data token sent by the owner</param>
  /// <example>
  /// *Example:* Cleaning up a newly detached entity and writing a detailed description of its death to a game console.
  /// ```csharp
  /// public override void EntityDetached(BoltEntity entity, Bolt.IProtocolToken token) {
  ///   MiniMap.instance.RemoveKnownEntity(entity);
  ///   
  ///   DeathRecap recap = (DeathRecap)token;
  ///   GameConsole.Log(recap.killer, recap.damageEvents);
  /// }
  /// ```
  /// </example>
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
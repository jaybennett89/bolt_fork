using Bolt;
using UnityEngine;


namespace BoltInternal {
  /// <summary>
  /// Base class for all BoltCallbacks objects
  /// </summary>
  /// <example>
  /// *Example:* Accepting an incoming remote connection.
  /// 
  /// ```csharp
  /// public override void ConnectRequest(BoltConnection connection) {
  ///   BoltNetwork.Accept(connection.remoteEndPoint);
  /// }
  /// ```
  /// </example> 
  [DocumentationAttribute(Alias = "Bolt.GlobalEventListener")]
  public abstract partial class GlobalEventListenerBase : MonoBehaviour, IBoltListNode {
    static readonly BoltDoubleList<GlobalEventListenerBase> callbacks = new BoltDoubleList<GlobalEventListenerBase>();

    object IBoltListNode.prev { get; set; }
    object IBoltListNode.next { get; set; }
    object IBoltListNode.list { get; set; }

    protected void OnEnable() {
      BoltCore._globalEventDispatcher.Add(this);
      callbacks.AddLast(this);
    }

    protected void OnDisable() {
      BoltCore._globalEventDispatcher.Remove(this);
      callbacks.Remove(this);
    }

    /// <summary>
    /// Override this method and return true if you want the event listener to keep being attached to Bolt even when Bolt shuts down and starts again.
    /// </summary>
    /// <returns>True/False</returns>
    /// <example>
    /// *Example:* Configuring the persistence behaviour to keep this listener alive between startup and shutdown.
    /// 
    /// ```csharp
    /// public override bool PersistBetweenStartupAndShutdown() {
    ///   return true;
    /// }
    /// ```
    /// </example>
    public virtual bool PersistBetweenStartupAndShutdown() {
      return false;
    }
  }
}
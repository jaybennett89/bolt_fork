namespace Bolt {

  /// <summary>
  /// Interface that can be implemented on Bolt.GlobalEventListener, Bolt.EntityEventListener and Bolt.EntityEventListener<T> 
  /// to modify its invoke condition settings
  /// </summary>
  /// <example>
  /// *Example:* A custom server callback class that invokes events even when disabled.
  /// 
  /// ```csharp
  /// [BoltGlobalBehaviour(BoltNetworkModes.Server)]
  /// public class BoltServerCallbacks : Bolt.GlobalEventListener, Bolt.IEventListener {
  ///   public bool InvokeIfDisabled { return true; }
  ///   public bool InvokeIfGameObjectIsInactive { return true; }
  ///   
  ///   // event callback overrides below
  /// } 
  /// ```
  /// </example>
  public interface IEventListener {
    bool InvokeIfDisabled { get; }
    bool InvokeIfGameObjectIsInactive { get; }
  }
}
namespace Bolt {
  /// <summary>
  /// Interface that can be implemented to create custom event filtering rules
  /// </summary>
  /// <example>
  /// *Example:* An event filter that blocks chat events from a chat restricted player
  /// 
  /// ```csharp
  /// public class ChatEventFilter : IEventFilter {
  ///   public bool EventReceived(Event evt) {
  ///     if(chatRestrictedPlayerList.ContainsKey(evt.RaisedBy)) {
  ///       return false;
  ///     }
  ///     else return true;
  ///   }
  /// }
  /// ```
  /// </example>
  public interface IEventFilter {
    /// <summary>
    /// Called when a new event is recieved
    /// </summary>
    /// <param name="ev">The event data</param>
    /// <returns>Whether to accept or reject the event</returns>
    bool EventReceived(Event ev);
  }

  /// <summary>
  /// Default implementation of Bolt.IEventFilter that lets everything through
  /// </summary>
  public class DefaultEventFilter : IEventFilter {
    bool IEventFilter.EventReceived(Event ev) {
      return true;
    }
  }
}

using System;

/// <summary>
/// Interface all event factories must implement
/// </summary>
public interface IBoltEventFactory {
  /// <summary>
  /// The event type this factory creates. Must be unique across the whole application.
  /// </summary>
  Type eventType { get; }

  /// <summary>
  /// The event id of the event this factory creates. Must e unique across the whojle application
  /// </summary>
  ushort eventId { get; }

  /// <summary>
  /// Create a new event of the type returned by IBoltEventFactory.eventType
  /// </summary>
  /// <returns></returns>
  object Create ();

  /// <summary>
  /// Dispatch an event of the type returned from IBoltEventFactory.eventType on the target object
  /// </summary>
  /// <param name="evnt">The event to be dispatched</param>
  /// <param name="target">The target that receives the event</param>
  void Dispatch (object evnt, object target);
}
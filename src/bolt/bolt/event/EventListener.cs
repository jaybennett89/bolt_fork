namespace Bolt {
  public interface IEventListener {
    bool InvokeIfDisabled { get; }
    bool InvokeIfGameObjectIsInactive { get; }
  }
}
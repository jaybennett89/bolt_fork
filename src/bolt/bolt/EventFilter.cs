namespace Bolt {
  public interface IEventFilter {
    bool EventReceived(NetworkEvent ev);
  }

  public class DefaultEventFilter : IEventFilter {
    bool IEventFilter.EventReceived(NetworkEvent ev) {
      return true;
    }
  }
}

/// <summary>
/// Base class for most objects in Bolt
/// </summary>
public class BoltObject : IBoltListNode {
#if DEBUG
  internal bool _pooled = true;
#endif

  object IBoltListNode.prev { get; set; }
  object IBoltListNode.next { get; set; }
  object IBoltListNode.list { get; set; }

  public static implicit operator bool (BoltObject obj) {
    return obj != null;
  }
}

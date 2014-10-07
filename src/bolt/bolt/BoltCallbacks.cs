using UnityEngine;

/// <summary>
/// Base class for all BoltCallbacks objects
/// </summary>
public abstract partial class BoltCallbacksBase : MonoBehaviour, IBoltListNode
{
    static readonly BoltDoubleList<BoltCallbacksBase> callbacks = new BoltDoubleList<BoltCallbacksBase>();

    object IBoltListNode.prev { get; set; }
    object IBoltListNode.next { get; set; }
    object IBoltListNode.list { get; set; }

    protected void OnEnable()
    {
        BoltCore._globalEventDispatcher.Add(this);
        callbacks.AddLast(this);
    }

    protected void OnDisable()
    {
        BoltCore._globalEventDispatcher.Remove(this);
        callbacks.Remove(this);
    }

    public virtual bool PersistBetweenStartupAndShutdown()
    {
        return false;
    }
}
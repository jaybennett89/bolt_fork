using UnityEngine;

public abstract class BoltCompilableAsset : ScriptableObject {
  public bool compile = true;

  public bool nameIsCSharpId {
    get { return true; }
  }
}

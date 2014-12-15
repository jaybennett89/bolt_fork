using UnityEngine;

namespace Bolt {
  public struct CachedObject<T> where T : Component {
    T component;
    float lastCheck;

    public bool Exists {
      get { return Component; }
    }

    public T Component {
      get {
        if (component) {
          return component;
        }
        else {
          if (lastCheck + 0.5f < Time.realtimeSinceStartup) {
            // update last check time
            lastCheck = Time.realtimeSinceStartup;

            // grab component
            component = GameObject.FindObjectOfType<T>();

            if (!component) {
              BoltLog.Error("Could not find object of type {0}", typeof(T).FullName);
            }

            return component;
          }

          return null;
        }
      }
    }
  }

  public struct CachedComponent<T> where T : Component {
    T component;

    readonly GameObject go;
    readonly bool children;

    public CachedComponent(GameObject gameObject, bool scanChildren) {
      go = gameObject;
      children = scanChildren;
      component = default(T);
    }

    public bool Exists {
      get { return Component; }
    }

    public T Component {
      get {
        if (go) {
          if (component) {
            return component;
          }
          else {
            if (children) {
              component = go.GetComponentInChildren<T>();
            }
            else {
              component = go.GetComponent<T>();
            }

            if (!component) {
              BoltLog.Error("Could not find {0} attached to {1}", typeof(T).FullName, go.name);
            }

            return component;
          }
        }
        else {
          return null;
        }
      }
    }
  }
}
using UnityEngine;

namespace Bolt {

  /// <summary>
  /// Utility struct for caching a unity component globally as a field on a class
  /// </summary>
  /// <typeparam name="T">The component type</typeparam>
  public struct CachedObject<T> where T : Component {
    T component;
    float lastCheck;

    /// <summary>
    /// Returns false if the component is currently null
    /// </summary>
    public bool Exists {
      get { return Component; }
    }

    /// <summary>
    /// Returns the cached component
    /// </summary>
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

  /// <summary>
  /// Utility struct for caching a unity component on the same object (or children) field on a class
  /// </summary>
  /// <typeparam name="T">The component type</typeparam>
  public struct CachedComponent<T> where T : Component {
    T component;

    readonly GameObject go;
    readonly bool children;

    public CachedComponent(GameObject gameObject, bool scanChildren) {
      go = gameObject;
      children = scanChildren;
      component = default(T);
    }

    /// <summary>
    /// Returns false if the component is currently null
    /// </summary>
    public bool Exists {
      get { return Component; }
    }

    /// <summary>
    /// Returns the cached component
    /// </summary>
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
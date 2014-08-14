using UnityEngine;

public abstract class BoltSingletonPrefab<T> : MonoBehaviour where T : MonoBehaviour {
  static T _instance;

  public static T instance {
    get {
      if (!_instance) {
        Object obj = FindObjectOfType(typeof(T));

        if (obj) {
          _instance = (T) obj;
        } else {
          obj = GameObject.Instantiate(Resources.Load(typeof(T).Name, typeof(GameObject)));

          if (obj) {
            _instance = ((GameObject) obj).GetComponent<T>();
          } else {
            BoltLog.Error("could not load auto instance of {0}", typeof(T));
          }
        }
      }

      return _instance;
    }
  }
}

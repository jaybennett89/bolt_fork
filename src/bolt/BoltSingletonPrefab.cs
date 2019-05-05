using Bolt;
using UnityEngine;

/// <summary>
/// Base class for monobehaviours that can be accessed as a singleton. The singleton is instantiated from the resources folder and should have the same name as the class type.
/// </summary>
/// <typeparam name="T">The name of the type and name of the prefab inside resources folder</typeparam>
/// <example>
/// *Example:* Using as a base class for PlayerCamera script.
/// 
/// ```csharp
/// public class PlayerCamera : BoltSingletonPrefab&ltPlayerCamera&gt {
///   Transform _target;
///   
///   public new Camera camera { 
///     get { return _camera; }
///   }
/// }
/// ```
/// </example>
[DocumentationAttribute]
public abstract class BoltSingletonPrefab<T> : MonoBehaviour where T : MonoBehaviour {
  static T _instance;
  static protected string _resourcePath;

  /// <summary>
  /// Returns the singleton instance of this type
  /// </summary>
  /// <example>
  /// *Example:* Using the player camera singleton
  /// 
  /// ```csharp
  /// public override void ControlOfEntityGained(BoltEntity entity {
  ///   PlayerCamera.instance.SetTarget(entity);
  /// }
  /// ```
  /// </example>
  public static T instance {
    get {
      Instantiate();
      return _instance;
    }
  }

  /// <summary>
  /// Create an instance of the singleton prefab
  /// </summary>
  /// <example>
  /// *Example:* Instantiate a player camera and game hud instance when the local scene is loaded. These will be cloned from
  /// from prefabs inside a Resources folder that have the same name as the type ("PlayerCamera" and "GameHUD").
  /// 
  /// ```csharp
  /// public override void SceneLoadLocalDone(string map) {
  ///   if(map.Equals("Game")) {
  ///     GameHUD.Instantiate();
  ///     PlayerCamera.Instantiate();
  ///   }
  /// }
  /// ```
  /// </example>
  public static void Instantiate() {
    if (!_instance) {
      Object obj = FindObjectOfType(typeof(T));

      if (obj) {
        _instance = (T)obj;
      }
      else {
        obj = GameObject.Instantiate(Resources.Load(_resourcePath == null ? typeof(T).Name : _resourcePath, typeof(GameObject)));

        if (obj) {
          _instance = ((GameObject)obj).GetComponent<T>();
        }
        else {
          BoltLog.Error("could not load auto instance of {0}", typeof(T));
        }
      }
    }
  }
}

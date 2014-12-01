using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UE = UnityEngine;

namespace Bolt {
  /// <summary>
  /// Base interface for all states
  /// </summary>
  [Documentation]
  public interface IState : IDisposable {
    UE.Animator Animator {
      get;
    }

    IEnumerable<UE.Animator> AllAnimators {
      get;
    }

    /// <summary>
    /// Set the animator object this state should use for reading/writing mecanim parameters
    /// </summary>
    /// <param name="animator">The animator object to use</param>
    void SetAnimator(UE.Animator animator);
    void AddAnimator(UE.Animator animator);

    /// <summary>
    /// Allows you to hook up a callback to a specific property
    /// </summary>
    /// <param name="path">The path of the property</param>
    /// <param name="callback">The callback delegate</param>
    void AddCallback(string path, PropertyCallback callback);

    /// <summary>
    /// Allows you to hook up a callback to a specific property
    /// </summary>
    /// <param name="path">The path of the property</param>
    /// <param name="callback">The callback delegate</param>
    void AddCallback(string path, PropertyCallbackSimple callback);

    /// <summary>
    /// Removes a callback from a property
    /// </summary>
    /// <param name="path">The path of the property</param>
    /// <param name="callback">The callback delegate to remove</param>
    void RemoveCallback(string path, PropertyCallback callback);

    /// <summary>
    /// Removes a callback from a property
    /// </summary>
    /// <param name="path">The path of the property</param>
    /// <param name="callback">The callback delegate to remove</param>
    void RemoveCallback(string path, PropertyCallbackSimple callback);

    /// <summary>
    /// Set a property dynamically by string name
    /// </summary>
    /// <param name="property"></param>
    /// <param name="value"></param>
    void SetDynamic(string property, object value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    object GetDynamic(string property);
  }
}

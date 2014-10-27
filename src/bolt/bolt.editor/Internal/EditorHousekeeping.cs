using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UE = UnityEngine;
using UED = UnityEditor;

namespace BoltEditor.Internal {
  [UED.InitializeOnLoad]
  public static class EditorHousekeeping {
    static DateTime saveSceneTime;
    static volatile Queue<Action> invokeQueue = new Queue<Action>();

    public static void Invoke(Action action) {
      lock (invokeQueue) {
        invokeQueue.Enqueue(action);
      }
    }

    public static void AskToSaveSceneAt(DateTime time) {
      saveSceneTime = time;
    }

    static EditorHousekeeping() {
      // so we dont auto save on load
      saveSceneTime = DateTime.MaxValue;

      // we want constant updates
      UED.EditorApplication.update += Update;
    }

    static void Update() {
      SaveScene();
      SaveEntities();
      InvokeCallbacks();
    }

    static void SaveScene() {
      if (saveSceneTime < DateTime.Now) {
        saveSceneTime = DateTime.MaxValue;

        // do this
        UED.EditorApplication.SaveCurrentSceneIfUserWantsTo();
      }
    }

    static void SaveEntities() {
      for (int i = 0; i < BoltInternal.BoltCoreInternal.ChangedEditorEntities.Count; ++i) {
        var entity = BoltInternal.BoltCoreInternal.ChangedEditorEntities[i];
        var entityPrefabType = UED.EditorUtility.GetPrefabType(entity);

        switch (entityPrefabType) {
          case UED.PrefabType.Prefab:
          case UED.PrefabType.PrefabInstance:
            UE.Debug.Log(string.Format("Saving Entity {0}", entity), entity);
            UED.EditorUtility.SetDirty(entity);
            UED.EditorUtility.SetDirty(entity.gameObject);
            break;
        }
      }

      BoltInternal.BoltCoreInternal.ChangedEditorEntities.Clear();
    }

    static void InvokeCallbacks() {
      lock (invokeQueue) {
        while (invokeQueue.Count > 0) {
          invokeQueue.Dequeue()();
        }
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UE = UnityEngine;
using UED = UnityEditor;

namespace Bolt.Editor.Internal {
  [UED.InitializeOnLoad]
  public static class EditorHousekeeping {
    public static DateTime AskToSaveSceneAt;

    static EditorHousekeeping() {
      // so we dont auto save on load
      AskToSaveSceneAt = DateTime.MaxValue;

      // we want constant updates
      UED.EditorApplication.update += Update;
    }

    static void Update() {
      if (AskToSaveSceneAt < DateTime.Now) {
        AskToSaveSceneAt = DateTime.MaxValue;

        // do this
        UED.EditorApplication.SaveCurrentSceneIfUserWantsTo();
      }

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
  }
}

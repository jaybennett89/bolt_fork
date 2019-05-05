using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UE = UnityEngine;
using UED = UnityEditor;

namespace BoltEditor {
  public static class EditorUtility {
    public static void UpdatePrefabDatabase() {
      BoltCompiler.UpdatePrefabsDatabase();
    }

    public static void CompileAssembly() {
      BoltUserAssemblyCompiler.Run().WaitOne();
    }

    public static void AssignSceneId(UE.GameObject gameObject) {
      BoltEntity entity = gameObject.GetComponent<BoltEntity>();

      if (!entity) {
        throw new ArgumentException(string.Format("Could not find a BoltEntity component attached to {0}", gameObject.name));
      }

      entity.ModifySettings().sceneId = Bolt.UniqueId.New();

      UED.EditorUtility.SetDirty(entity);
      UED.EditorUtility.SetDirty(gameObject);
    }

    public static void AssignSceneIdAll() {
      BoltMenuItems.GenerateSceneObjectGuids();
    }

    public static void InstallBolt() {
      BoltInstaller.Run();
    }

    public static void InvokeOnMainThread(Action action) {
      BoltEditor.Internal.EditorHousekeeping.Invoke(action);
    }
  }
}

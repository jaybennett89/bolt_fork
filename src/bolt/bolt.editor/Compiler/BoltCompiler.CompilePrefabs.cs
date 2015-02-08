using Bolt;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

partial class BoltCompiler {
  struct BoltPrefab {
    public int id;
    public string name;
    public GameObject go;
  }

  static IEnumerable<BoltPrefab> FindPrefabs() {
    var id = 1;
    var files = Directory.GetFiles(@"Assets", "*.prefab", SearchOption.AllDirectories);
    var settings = BoltRuntimeSettings.instance;

    for (int i = 0; i < files.Length; ++i) {
      BoltEntity entity = AssetDatabase.LoadAssetAtPath(files[i], typeof(BoltEntity)) as BoltEntity;

      if (entity) {
        entity._prefabId = id;
        entity._sceneGuid = null;

        if (settings.clientCanInstantiateAll) {
          entity._allowInstantiateOnClient = true;
        }

        EditorUtility.SetDirty(entity.gameObject);
        EditorUtility.SetDirty(entity);

        yield return new BoltPrefab { go = entity.gameObject, id = id, name = entity.gameObject.name.CSharpIdentifier() };

        id += 1;
      }

      EditorUtility.DisplayProgressBar("Updating Bolt Prefab Database", "Scanning for prefabs ...", Mathf.Clamp01((float)i / (float)files.Length));
    }
  }

  public static void UpdatePrefabsDatabase() {
    try {
      // get all prefabs
      IEnumerable<BoltPrefab> prefabs = FindPrefabs();

      // create new array
      PrefabDatabase.Instance.Prefabs = new GameObject[prefabs.Count() + 1];

      // update array
      foreach (BoltPrefab prefab in prefabs) {
        if (PrefabDatabase.Instance.Prefabs[prefab.id]) {
          throw new BoltException("Duplicate Prefab ID {0}", prefab.id);
        }

        // assign prefab
        PrefabDatabase.Instance.Prefabs[prefab.id] = prefab.go;

        // log this to the user
        Debug.Log(string.Format("Assigned {0} to '{1}'", new PrefabId(prefab.id), AssetDatabase.GetAssetPath(prefab.go)));
      }

      // save it!
      EditorUtility.SetDirty(PrefabDatabase.Instance);
    }
    finally {
      EditorUtility.ClearProgressBar();
    }
  }

  static void CompilePrefabs(BoltCompilerOperation op) {
    if (PrefabDatabase.Instance.DatabaseMode == PrefabDatabaseMode.AutomaticScan) {
      UpdatePrefabsDatabase();
    }

    using (BoltSourceFile file = new BoltSourceFile(op.prefabsFilePath)) {
      file.EmitScope("public static class BoltPrefabs", () => {
        for (int i = 1; i < PrefabDatabase.Instance.Prefabs.Length; ++i) {
          GameObject prefab = PrefabDatabase.Instance.Prefabs[i];

          if (prefab) {
            file.EmitLine("public static readonly Bolt.PrefabId {0} = new Bolt.PrefabId({1});", BoltEditorUtilsInternal.CSharpIdentifier(prefab.name), prefab.GetComponent<BoltEntity>()._prefabId);
          }
        }
      });
    }
  }
}

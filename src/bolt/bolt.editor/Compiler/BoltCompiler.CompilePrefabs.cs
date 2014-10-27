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
    int id = 1;

    foreach (var file in Directory.GetFiles(@"Assets", "*.prefab", SearchOption.AllDirectories)) {
      BoltEntity entity = AssetDatabase.LoadAssetAtPath(file, typeof(BoltEntity)) as BoltEntity;

      if (entity) {
        entity._prefabId = id;

        EditorUtility.SetDirty(entity.gameObject);
        EditorUtility.SetDirty(entity);

        yield return new BoltPrefab { go = entity.gameObject, id = id, name = entity.gameObject.name.CSharpIdentifier() };

        id += 1;
      }
    }
  }

  public static void UpdatePrefabsDatabase() {
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

  static void CompilePrefabs(BoltCompilerOperation op) {
    if (PrefabDatabase.Instance.ManualMode == false) {
      UpdatePrefabsDatabase();
    }

    using (BoltSourceFile file = new BoltSourceFile(op.prefabsFilePath)) {
      file.EmitScope("public static class BoltPrefabs", () => {
        for (int i = 1; i < PrefabDatabase.Instance.Prefabs.Length; ++i) {
          GameObject prefab = PrefabDatabase.Instance.Prefabs[i];

          if (prefab) {
            file.EmitLine("public static readonly Bolt.PrefabId {0} = new Bolt.PrefabId({1});", BoltEditorUtils.CSharpIdentifier(prefab.name), prefab.GetComponent<BoltEntity>()._prefabId);
          }
        }
      });
    }
  }
}

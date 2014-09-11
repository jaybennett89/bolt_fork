using UnityEditor;
using UnityEngine;

class BoltPrefab {
  public int id;
  public string name;
  public GameObject go;
}

partial class BoltCompiler {
  static void CompilePrefabs (BoltCompilerOperation op) {
    mode = BoltCompilerMode.Prefabs;

    // update prefabs lookup table
    GameObject[] prefabsArray = new GameObject[op.prefabs.Count];

    for (int i = 0; i < op.prefabs.Count; ++i) {
      prefabsArray[op.prefabs[i].id] = op.prefabs[i].go;
    }

    // grab asset
    BoltRuntimeSettings table = BoltEditorUtils.GetSingletonAsset<BoltRuntimeSettings>();

    // update asset
    table._prefabs = prefabsArray;

    // mark as dirty
    EditorUtility.SetDirty(table);

    // create prefabs enumeration
    using (BoltSourceFile file = new BoltSourceFile(op.prefabsFilePath)) {
      file.EmitScope("public static class BoltPrefabs", () => {
        foreach (BoltPrefab prefab in op.prefabs) {
          file.EmitLine("public const string {0} = \"{0}\";", BoltEditorUtils.CSharpIdentifier(prefab.name), prefab.id);
        }
      });
    }
  }
}

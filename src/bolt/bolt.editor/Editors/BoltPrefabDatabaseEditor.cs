using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Bolt.PrefabDatabase))]
public class BoltPrefabDatabaseEditor : Editor {
  public override void OnInspectorGUI() {
    Bolt.PrefabDatabase db = (Bolt.PrefabDatabase)target;

    EditorGUILayout.BeginVertical();

    for (int i = 1; i < db.Prefabs.Length; ++i) {
      if (db.Prefabs[i]) {
        GUIStyle style;

        style = new GUIStyle(EditorStyles.miniButton);
        style.alignment = TextAnchor.MiddleLeft;

        string id = db.Prefabs[i].GetComponent<BoltEntity>().prefabId.ToString();
        string path = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(db.Prefabs[i]));

        if (GUILayout.Button(id + " " + path, style)) {
          Selection.activeGameObject = db.Prefabs[i];
        }
      }
    }

    EditorGUILayout.EndVertical();
  }
}

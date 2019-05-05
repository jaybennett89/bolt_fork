using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoltHitboxBody))]
public class BoltHitboxBodyEditor : Editor {
  public override void OnInspectorGUI () {
    base.OnInspectorGUI();

    if (GUILayout.Button("Find Hitboxes", EditorStyles.miniButton)) {
      BoltHitboxBody hbtarget = (BoltHitboxBody) target;

      hbtarget._hitboxes = hbtarget.GetComponentsInChildren<BoltHitbox>().Where(x => x._type != BoltHitboxType.Proximity).ToArray();
      hbtarget._proximity = hbtarget.GetComponentsInChildren<BoltHitbox>().FirstOrDefault(x => x._type == BoltHitboxType.Proximity);

      EditorUtility.SetDirty(hbtarget);
    }
  }
}

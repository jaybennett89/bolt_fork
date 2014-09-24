using UnityEngine;
using UnityEditor;
using System.Collections;

public static class BoltEditorGUI {
  public static Color ColorStateHeader {
    get { return new Color(0 / 255f, 162f / 255f, 232f / 255f); }
  }

  public static GUIStyle SelectedStyle {
    get {

      GUIStyle s = new GUIStyle();

      s.normal.background = ((GUIStyle)"LODSliderRangeSelected").normal.background;

      return s;

    }
  }

  public static int ClickCount {
    get { return Event.current.clickCount; }
  }

  public static bool IsLeftClick {
    get { return Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.clickCount > 0; }
  }

  public static bool IsRightClick {
    get { return Event.current.type == EventType.MouseDown && Event.current.button == 1 && Event.current.clickCount > 0; }
  }

  public static bool WasKeyPressed(KeyCode key) {
    return Event.current.type == EventType.KeyDown && Event.current.keyCode == key;
  }

  public static void UseEvent() {
    if (Event.current != null) {
      Event.current.Use();
    }
  }

  public static void Icon(string name) {
    Icon(name, new RectOffset());
  }

  public static void Icon(string name, RectOffset offset) {
    GUIStyle s;

    s = new GUIStyle(GUIStyle.none);
    s.margin = offset;

    GUILayout.Box(LoadIcon(name), s, GUILayout.Width(16), GUILayout.Height(16));
  }

  public static void IconClickable(string name, System.Action onClick) {
    IconClickable(name, new RectOffset(), onClick);
  }

  public static void IconClickable(string name, RectOffset offset, System.Action onClick) {
    Icon(name, offset);
    MakeClickable(onClick);
  }

  public static void LabelClickable(string label, System.Action onClick) {
    LabelClickable(label, GUI.skin.label, onClick);
  }

  public static void LabelClickable(string label, GUIStyle style, System.Action onClick) {
    GUILayout.Label(label, style);
    MakeClickable(onClick);
  }

  static void MakeClickable(System.Action onClick) {
    Rect r = GUILayoutUtility.GetLastRect();

    if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition)) {
      onClick();
    }
  }

  public static Texture2D LoadIcon(string name) {
    return Resources.Load("icons/" + name, typeof(Texture2D)) as Texture2D;
  }
}

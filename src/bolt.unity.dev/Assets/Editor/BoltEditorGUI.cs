using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Linq;
using Bolt.Compiler;
using System.Collections.Generic;

public static class BoltEditorGUI {
  public static readonly Color Blue = new Color(0 / 255f, 162f / 255f, 232f / 255f);
  public static readonly Color LightBlue = new Color(0f / 255f, 232f / 255f, 226f / 255f);
  public static readonly Color Orange = new Color(255f / 255f, 127f / 255f, 39f / 255f);
  public static readonly Color LightGreen = new Color(105f / 255f, 251f / 255f, 9f / 255f);
  public static readonly Color DarkGreen = new Color(34f / 255f, 177f / 255f, 76f / 255f);
  public static readonly Color LightOrange = new Color(255f / 255f, 201f / 255f, 12f / 255f);

  public static GUIStyle ImageButtonStyle {
    get {
      GUIStyle style;
      style = new GUIStyle();
      style.margin = new RectOffset(4, 4, 2, 0);
      return style;
    }
  }

  public static GUIStyle ParameterBackgroundStyle {
    get {
      GUIStyle style;
      style = new GUIStyle("ObjectFieldThumb");
      style.padding = new RectOffset(5, 5, 5, 5);
      style.margin = new RectOffset(5, 5, 0, 5);
      return style;
    }
  }

  public static GUIStyle StateHeaderStyle {
    get {
      GUIStyle style;
      style = NodeStyle(2);
      style.padding = new RectOffset(0, 0, 0, 3);
      style.margin = new RectOffset(5, 5, 5, 5);
      return style;
    }
  }

  public static Color StateHeaderColor {
    get { return Blue; }
  }


  public static GUIStyle InheritanceSeparatorStyle {
    get {
      GUIStyle style;

      style = new GUIStyle(EditorStyles.label);
      style.contentOffset = new Vector2(2, -1);
      style.normal.textColor = Color.white;

      return style;
    }
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

  public static GUIStyle NodeStyle(int n) {
    GUIStyle s = new GUIStyle("flow node " + n);
    s.padding = new RectOffset();
    s.margin = new RectOffset();
    return s;
  }

  public static void Icon(string name) {
    Icon(name, new RectOffset());
  }

  public static void Icon(string name, RectOffset offset) {
    Icon(name, offset, GUILayout.Width(16), GUILayout.Height(16));
  }

  public static void Icon(string name, RectOffset offset, params GUILayoutOption[] layout) {
    GUIStyle s;

    s = new GUIStyle(GUIStyle.none);
    s.margin = offset;

    GUILayout.Box(LoadIcon(name), s, layout);
  }

  public static void IconClickable(string name, System.Action onClick) {
    IconClickable(name, new RectOffset(), onClick);
  }

  public static void IconClickable(string name, RectOffset offset, System.Action onClick) {
    IconClickable(name, offset, onClick, 16);
  }

  public static void IconClickable(string name, RectOffset offset, System.Action onClick, int w) {
    Icon(name, offset, GUILayout.Width(w), GUILayout.Height(16));
    MakeClickable(onClick);
  }

  public static void LabelClickable(string label, System.Action onClick) {
    LabelClickable(label, GUI.skin.label, onClick);
  }

  public static void LabelClickable(string label, GUIStyle style, System.Action onClick) {
    GUILayout.Label(label, style);
    MakeClickable(onClick);
  }

  public static Texture2D LoadIcon(string name) {
    return Resources.Load("icons/" + name, typeof(Texture2D)) as Texture2D;
  }

  public static Guid AssetPopup(IEnumerable<AssetDefinition> assets, Guid current, IEnumerable<Guid> exclude) {
    var filtered = assets.Where(x => !exclude.Contains(x.Guid)).ToArray();
    var options = (new string[] { "-" }).Concat(filtered.Select(x => x.Name)).ToArray();
    var selected = Array.FindIndex(filtered, x => x.Guid == current) + 1;

    selected = EditorGUILayout.Popup(selected, options);

    if (selected == 0) {
      return Guid.Empty;
    }

    return filtered[selected - 1].Guid;
  }

  public static void PropertyTypePopup(AssetDefinition asset, PropertyDefinition definition) {
    if (!asset.AllowedPropertyTypes.Contains(definition.PropertyType.GetType())) {
      definition.PropertyType = new PropertyTypeFloat();
    }

    var types = asset.AllowedPropertyTypes.ToArray();
    var typesNames = types.Select(x => x.Name.Replace("PropertyType", "")).ToArray();
    var selected = Array.IndexOf(types, definition.PropertyType.GetType());
    var selectedNew = EditorGUILayout.Popup(selected, typesNames);

    if (selected != selectedNew) {
      definition.PropertyType = (PropertyType)Activator.CreateInstance(types[selectedNew]);
    }
  }

  public static void AddButton(string text, List<PropertyDefinition> list, Func<PropertyDefinitionAssetSettings> newSettings) {
    GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
    labelStyle.margin = new RectOffset();
    labelStyle.padding = new RectOffset(5, 0, 1, 0);
    labelStyle.contentOffset = new Vector2();
    labelStyle.normal.textColor = Color.white;

    GUIStyle buttonStyle = new GUIStyle();
    buttonStyle.margin = new RectOffset();
    buttonStyle.padding = new RectOffset();
    buttonStyle.contentOffset = new Vector2();

    EditorGUILayout.BeginHorizontal();
    bool btn0 = GUILayout.Button(text, labelStyle, GUILayout.ExpandWidth(false));
    bool btn1 = GUILayout.Button(LoadIcon("boltico_add") as Texture, buttonStyle, GUILayout.Width(16), GUILayout.Height(16));
    EditorGUILayout.EndHorizontal();

    if (btn0 || btn1) {
      list.Add(
        new PropertyDefinition {
          Name = "NewProperty",
          Comment = "",
          Deleted = false,
          Enabled = true,
          Expanded = true,
          PropertyType = new PropertyTypeFloat { Compression = new FloatCompression { Bits = 32 } },
          AssetSettings = newSettings()
        }
      );
    }
  }

  public static bool IconButton(string icon) {
    return IconButton(icon, Color.white);
  }

  public static bool IconButton(string icon, bool enabled) {
    return IconButton(icon, new Color(1, 1, 1, enabled ? 1f : 0.25f));
  }

  public static bool OnOffButton(string on, string off, bool enabled) {
    return GUILayout.Button(LoadIcon(enabled ? on : off) as Texture, ImageButtonStyle, GUILayout.Width(16), GUILayout.Height(16));
  }

  public static bool IconButton(string icon, float opacity) {
    return IconButton(icon, new Color(1, 1, 1, Mathf.Clamp01(opacity)));
  }

  public static bool IconButton(string icon, Color color) {
    bool result = false;

    WithColor(color, () => {
      result = GUILayout.Button(LoadIcon(icon) as Texture, ImageButtonStyle, GUILayout.Width(16), GUILayout.Height(16));
    });

    return result;
  }

  public static void WithColor(Color color, Action gui) {
    GUI.color = color;

    try {
      gui();
    }
    finally {
      GUI.color = Color.white;
    }
  }

  static void MakeClickable(System.Action onClick) {
    Rect r = GUILayoutUtility.GetLastRect();

    if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition)) {
      onClick();
    }
  }
}

using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Linq;
using Bolt.Compiler;
using System.Collections.Generic;
using System.Reflection;

public static class BoltEditorGUI {
  public static string Tooltip = "";

  public static readonly Color Blue = new Color(0 / 255f, 162f / 255f, 232f / 255f);
  public static readonly Color LightBlue = new Color(0f / 255f, 232f / 255f, 226f / 255f);
  public static readonly Color Orange = new Color(255f / 255f, 127f / 255f, 39f / 255f);
  public static readonly Color LightGreen = new Color(105f / 255f, 251f / 255f, 9f / 255f);
  public static readonly Color DarkGreen = new Color(34f / 255f, 177f / 255f, 76f / 255f);
  public static readonly Color LightOrange = new Color(255f / 255f, 201f / 255f, 12f / 255f);

  public static void SetWindowTitle(this EditorWindow editor, string title, Texture icon) {
    typeof(EditorWindow).GetField("m_CachedTitleContent", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(editor, new GUIContent(title, icon));
  }

  public static void SetTooltip(string tooltip) {
    Rect r = GUILayoutUtility.GetLastRect();

    if (r.Contains(Event.current.mousePosition)) {
      Tooltip = tooltip;
    }
  }

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

  public static GUIStyle StructHeaderStyle {
    get {
      return StateHeaderStyle;

      //GUIStyle style;
      //style = NodeStyle(2);
      //style.padding = new RectOffset(0, 0, 0, 3);
      //style.margin = new RectOffset(5, 5, 5, 5);
      //return style;
    }
  }

  public static GUIStyle StateHeaderStyle {
    get {
      GUIStyle style;
      style = NodeStyle(1);
      style.padding = new RectOffset(0, 0, 0, 3);
      style.margin = new RectOffset(5, 5, 5, 5);
      return style;
    }
  }

  public static GUIStyle EventHeaderStyle {
    get {
      return CommandHeaderStyle;
      //GUIStyle style;
      //style = NodeStyle(4);
      //style.padding = new RectOffset(0, 0, 0, 3);
      //style.margin = new RectOffset(5, 5, 5, 5);
      //return style;
    }
  }


  public static GUIStyle CommandHeaderStyle {
    get {
      GUIStyle style;
      style = NodeStyle(2);
      style.padding = new RectOffset(0, 0, 0, 3);
      style.margin = new RectOffset(5, 5, 5, 5);
      return style;
    }
  }

  public static GUIStyle WhiteTextureBackgroundStyle {
    get {
      GUIStyle bg;
      bg = new GUIStyle(GUIStyle.none);
      bg.normal.background = EditorGUIUtility.whiteTexture;
      return bg;
    }
  }

  public static Color ToUnityColor(this Color4 c) {
    return new Color(c.R, c.G, c.B, c.A);
  }

  public static Color4 ToBoltColor(this Color c) {
    return new Color4(c.r, c.g, c.b, c.a);
  }

  public static Color ColorInt(int r, int g, int b) {
    return new Color(r / 255f, g / 255f, b / 255f);
  }

  public static Color ColorOpacity(float opacity) {
    return new Color(1, 1, 1, opacity);
  }

  public static Color StateHeaderColor {
    get { return ColorInt(225, 255, 225); }
  }

  public static Color StructHeaderColor {
    get { return StateHeaderColor; }
    //get { return ColorInt(225, 255, 225); }
  }
   
  public static Color EventHeaderColor {
    get { return CommandHeaderColor; }
    //get { return ColorInt(255, 75, 75); }
  }

  public static Color CommandHeaderColor {
    get { return LightOrange; }
  }

  public static GUIStyle SmallWhiteText {
    get {
      GUIStyle s = new GUIStyle(EditorStyles.miniLabel);
      s.normal.textColor = Color.white;
      return s;
    }
  }

  public static void Disabled(Action gui) {
    EditorGUI.BeginDisabledGroup(true);
    gui();
    EditorGUI.EndDisabledGroup();
  }

  public static GUIStyle PropertiesAddTextStyle {
    get {
      GUIStyle s = new GUIStyle(SmallWhiteText);
      return s;
    }
  }

  public static GUIStyle InheritanceSeparatorStyle {
    get {
      GUIStyle s;
      s = new GUIStyle(EditorStyles.label);
      s.padding = new RectOffset(4, 2, 0, 0);
      s.contentOffset = new Vector2(0, 0);
      s.normal.textColor = Color.white;
      return s;
    }
  }

  public static GUIStyle MiniLabelButtonStyle {
    get {
      GUIStyle s;
      s = new GUIStyle(EditorStyles.miniLabel);
      s.normal.textColor = Color.white;
      s.alignment = TextAnchor.MiddleRight;
      return s;
    }
  }

  public static GUIStyle SelectedStyle {
    get {
      GUIStyle s = new GUIStyle();
      s.normal.background = ((GUIStyle)"LODSliderRangeSelected").normal.background;
      return s;
    }
  }

  public static GUIStyle MiniLabelWithColor(Color c) {
    GUIStyle s;
    s = new GUIStyle(EditorStyles.miniLabel);
    s.normal.textColor = c;
    return s;
  }

  public static int ClickCount {
    get { return Event.current.clickCount; }
  }

  public static bool IsLeftClick {
    get { return Event.current.type == EventType.MouseDown && Event.current.button == 0; }
  }

  public static bool IsRightClick {
    get { return Event.current.type == EventType.MouseDown && Event.current.button == 1; }
  }

  public static bool WasKeyPressed(KeyCode key) {
    return Event.current.type == EventType.KeyDown && Event.current.keyCode == key;
  }

  public static void UseEvent() {
    if (Event.current != null) {
      Event.current.Use();
    }
  }

  public static string EditComment(string comment) {
    GUILayout.BeginHorizontal();
    GUILayout.Label("//", BoltEditorGUI.InheritanceSeparatorStyle, GUILayout.Width(15));
    comment = EditorGUILayout.TextArea(comment);
    GUILayout.EndHorizontal();

    return comment;
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

  public static void Icon(string name, Rect r) {
    GUI.DrawTexture(r, LoadIcon(name));
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

  public static void WithLabel(string label, Action gui) {
    GUILayout.BeginHorizontal();
    EditorGUILayout.LabelField(label, GUILayout.Width(200));

    gui();

    GUILayout.EndHorizontal();
  }

  public static void PropertyTypePopup(AssetDefinition asset, PropertyDefinition definition, params GUILayoutOption[] options) {
    if (!asset.AllowedPropertyTypes.Contains(definition.PropertyType.GetType())) {
      definition.PropertyType = new PropertyTypeFloat();
    }

    definition.PropertyType = PropertyTypePopup(asset.AllowedPropertyTypes, definition.PropertyType, options);
  }

  public static PropertyType PropertyTypePopup(IEnumerable<Type> allTypes, PropertyType current, params GUILayoutOption[] options) {
    if (current == null) {
      current = (PropertyType)Activator.CreateInstance(allTypes.First());
    }

    var types = allTypes.OrderBy(x => x.Name.Replace("PropertyType", "")).ToArray();
    var typesNames = types.Select(x => x.Name.Replace("PropertyType", "")).ToArray();
    var selected = Array.IndexOf(types, current.GetType());
    var selectedNew = EditorGUILayout.Popup(selected, typesNames, options);

    if (selected != selectedNew) {
      return (PropertyType)Activator.CreateInstance(types[selectedNew]);
    }

    return current;
  }

  public static void AddButton(string text, List<PropertyDefinition> list, Func<PropertyAssetSettings> newSettings) {


    EditorGUILayout.BeginHorizontal();
    bool btn0 = LabelButton(text, true, 1, GUILayout.ExpandWidth(false));
    bool btn1 = IconButton("boltico_plus".ToContent());
    EditorGUILayout.EndHorizontal();

    if (btn0 || btn1) {
      list.Add(
        new PropertyDefinition {
          Name = "NewProperty",
          Comment = "",
          Deleted = false,
          Enabled = true,
          Expanded = true,
          PropertyType = new PropertyTypeFloat { Compression = FloatCompression.Default() },
          AssetSettings = newSettings()
        }
      );
    }
  }

  public static GUIContent ToContent(this string text) {
    return ToContent(text, "");
  }

  public static GUIContent ToContent(this string text, string tooltip) {
    return new GUIContent(text, tooltip);
  }

  public static bool IconButton(GUIContent icon) {
    return IconButton(icon, Color.white);
  }

  public static bool IconButton(GUIContent icon, bool enabled) {
    return IconButton(icon, new Color(1, 1, 1, enabled ? 1f : 0.25f));
  }

  public static bool OnOffButton(GUIContent on, GUIContent off, bool enabled) {
    var texture = LoadIcon(enabled ? on.text : off.text) as Texture;
    var tooltip = enabled ? on.tooltip : off.tooltip;
    return GUILayout.Button(new GUIContent(texture, tooltip), ImageButtonStyle, GUILayout.Width(16), GUILayout.Height(16));
  }

  public static bool OnOffButton(GUIContent on, GUIContent off, bool enabled, float offOpacity) {
    return IconButton(enabled ? on : off, enabled ? 1f : offOpacity);
  }

  public static bool IconButton(GUIContent icon, float opacity) {
    return IconButton(icon, new Color(1, 1, 1, Mathf.Clamp01(opacity)));
  }

  public static bool IconButton(GUIContent icon, Color color) {
    bool result = false;

    WithColor(color, () => {
      var texture = LoadIcon(icon.text) as Texture;
      var tooltip = icon.tooltip;
      result = GUILayout.Button(new GUIContent(texture, tooltip), ImageButtonStyle, GUILayout.Width(16), GUILayout.Height(16));
    });

    return result;
  }

  public static int EditPriority(int priority, bool enabled) {
    if (enabled) {
      priority = Mathf.Clamp(EditorGUILayout.IntField(priority, GUILayout.Width(32)), 1, 999);
      BoltEditorGUI.SetTooltip("Priority. An integer between 1 and 999. Higher values means this is more likely to be sent.");
    }
    else {
      BoltEditorGUI.Disabled(() => {
        EditorGUILayout.TextField("---", GUILayout.Width(32));
      });
    }

    return priority;
  }

  public static bool LabelButton(string label, bool enabled, float opacity, params GUILayoutOption[] options) {
    bool result = false;

    WithColor(enabled ? Color.white : ColorOpacity(opacity), () => {
      result = GUILayout.Button(label, BoltEditorGUI.MiniLabelButtonStyle, options);
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

  public static float FloatFieldOverlay(float value, string overlay, params GUILayoutOption[] options) {
    value = EditorGUILayout.FloatField(value, options);

    GUIStyle s = new GUIStyle("Label");
    s.alignment = TextAnchor.MiddleRight;
    s.normal.textColor = Color.gray;

    GUI.Label(GUILayoutUtility.GetLastRect(), overlay, s);
    return value;
  }

  public static int IntFieldOverlay(int value, string overlay, params GUILayoutOption[] options) {
    value = EditorGUILayout.IntField(value, options);

    GUIStyle s = new GUIStyle(EditorStyles.miniLabel);
    s.alignment = TextAnchor.MiddleRight;
    s.contentOffset = new Vector2(-2, 0);
    s.normal.textColor = Color.gray;

    GUI.Label(GUILayoutUtility.GetLastRect(), overlay, s);
    return value;
  }

  public static FloatCompression EditFloatCompression(FloatCompression c) {
    return EditFloatCompression("Value Range", c);
  }

  public static FloatCompression EditFloatCompression(string label, FloatCompression c) {
    if (c == null) {
      c = FloatCompression.Default();
    }

    var bits = BoltMath.BitsRequired((c.MaxValue - c.MinValue) * c.Bits);
    var accuracy = (1f / c.Bits);

    WithLabel(label, () => {
      c.MinValue = Mathf.Min(IntFieldOverlay(c.MinValue, "Min"), c.MaxValue - 1);
      c.MaxValue = Mathf.Max(IntFieldOverlay(c.MaxValue, "Max"), c.MinValue + 1);
      c.Bits = Mathf.Clamp(IntFieldOverlay(c.Bits, "Bits"), 1, 32);

      if (IconButton("chart-pie-separate".ToContent())) {
        BoltFloatCompressionEditor.Target = c;
      }
    });

    return c;
  }

  public static void EditAxes(string label, Axis[] axis) {

    WithLabel(label, () => {
      EditorGUILayout.BeginVertical();

      for (int i = 0; i < axis.Length; ++i) {
        EditAxis(axis[i]);
      }

      EditorGUILayout.EndVertical();
    });

  }

  public static void EditAxis(Axis axis) {
    EditorGUILayout.BeginHorizontal();

    axis.Enabled = GUILayout.Toggle(axis.Enabled, " " + axis.Component.ToString(), GUILayout.Width(38));

    EditorGUILayout.BeginVertical();
    EditorGUI.BeginDisabledGroup(!axis.Enabled);
    EditFloatCompression(axis.Compression);
    EditorGUI.EndDisabledGroup();
    EditorGUILayout.EndVertical();

    EditorGUILayout.EndHorizontal();
  }

  static void MakeClickable(System.Action onClick) {
    Rect r = GUILayoutUtility.GetLastRect();

    if (IsLeftClick && r.Contains(Event.current.mousePosition)) {
      onClick();
      UseEvent();
    }
  }
}

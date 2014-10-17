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

  public static AxisSelections EditAxisSelection(AxisSelections value) {
    return EditAxisSelection(null, value);
  }

  public static AxisSelections EditAxisSelection(string prefix, AxisSelections value) {
    var values = Enum.GetValues(typeof(AxisSelections)).Cast<AxisSelections>().ToArray();
    var names = values.Select(x => x.ToString());
    AxisSelections selection = values[EditorGUILayout.Popup(Array.IndexOf(values, value), names.Select(x => (prefix ?? "") + x).ToArray())];

    if (prefix == null && selection == AxisSelections.Disabled) {
      Debug.LogError("Can't disable axis selection on standalone properties");
      selection = AxisSelections.XYZ;
    }

    return selection;
  }

  public static void EditAxes(FloatCompression[] compression, AxisSelections selection) {
    EditorGUILayout.BeginVertical();
    compression[Axis.X] = ExitAxis(compression[Axis.X], "X", (selection & AxisSelections.X) == AxisSelections.X);
    compression[Axis.Y] = ExitAxis(compression[Axis.Y], "Y", (selection & AxisSelections.Y) == AxisSelections.Y);
    compression[Axis.Z] = ExitAxis(compression[Axis.Z], "Z", (selection & AxisSelections.Z) == AxisSelections.Z);
    EditorGUILayout.EndVertical();
  }

  static FloatCompression ExitAxis(FloatCompression compression, string label, bool enabled) {
    if (enabled) {
      EditorGUILayout.BeginHorizontal();
      GUILayout.Label(label, GUILayout.Width(15));
      compression = BoltEditorGUI.EditFloatCompression(compression);
      EditorGUILayout.EndHorizontal();
    }

    return compression;
  }

  public static void EditSmoothingAlgorithm(AssetDefinition adef, PropertyDefinition pdef) {
    if (adef is StateDefinition) {
      BoltEditorGUI.WithLabel("Smoothing Algorithm", () => {
        pdef.StateAssetSettings.SmoothingAlgorithm = (SmoothingAlgorithms)EditorGUILayout.EnumPopup(pdef.StateAssetSettings.SmoothingAlgorithm);
      });

      if (pdef.StateAssetSettings.SmoothingAlgorithm == SmoothingAlgorithms.Extrapolation) {

        if (pdef.PropertyType is PropertyTypeTransform) {
          PropertyTypeTransform transform = (PropertyTypeTransform)pdef.PropertyType;

          BoltEditorGUI.WithLabel("Extrapolation Velocity", () => {
            transform.ExtrapolationVelocityMode = (ExtrapolationVelocityModes)EditorGUILayout.EnumPopup(transform.ExtrapolationVelocityMode);
          });
        }

        BoltEditorGUI.WithLabel("Extrapolation Settings", () => {
          pdef.StateAssetSettings.ExtrapolationMaxFrames = IntFieldOverlay(pdef.StateAssetSettings.ExtrapolationMaxFrames, "Max Frames");
          pdef.StateAssetSettings.ExtrapolationCorrectionFrames = IntFieldOverlay(pdef.StateAssetSettings.ExtrapolationCorrectionFrames, "Correction Frames");
          pdef.StateAssetSettings.ExtrapolationErrorTolerance = FloatFieldOverlay(pdef.StateAssetSettings.ExtrapolationErrorTolerance, "Error Tolerance");
        });
      }
    }
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
      style.margin = new RectOffset(0, 0, 0, 0);
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

  public static GUIStyle AccentText {
    get {
      GUIStyle s = new GUIStyle(EditorStyles.miniLabel);
      s.padding = new RectOffset();
      s.margin = new RectOffset(4, 4, 4, 0);
      s.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
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
      GUIStyle s = new GUIStyle(AccentText);
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

  public static Texture2D LoadIcon(string name) {
    return Resources.Load("icons/" + name, typeof(Texture2D)) as Texture2D;
  }

  public static void LabelClickable(string label, System.Action onClick) {
    LabelClickable(label, GUI.skin.label, onClick);
  }

  public static void LabelClickable(string label, GUIStyle style, System.Action onClick) {
    GUILayout.Label(label, style);
    MakeClickable(onClick);
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
    EditorGUILayout.LabelField(label, GUILayout.Width(220));

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

  static GUIStyle PaddingStyle(int left, int right, int top, int bottom) {
    GUIStyle s = new GUIStyle(GUIStyle.none);
    s.padding = new RectOffset(left, right, top, bottom);
    return s;
  }

  public static bool IconButton(string icon, Color color) {
    GUIStyle style;
    style = new GUIStyle(GUIStyle.none);
    style.padding = new RectOffset();
    style.margin = new RectOffset(0, 0, 0, 0);
    style.contentOffset = new Vector2(0, 0);

    GUI.color = color;
    bool result = GUILayout.Button(LoadIcon(icon), style, GUILayout.Width(16), GUILayout.Height(16));
    GUI.color = Color.white;

    return result;
  }

  public static bool Button(string icon) {
    return IconButton(icon, BoltRuntimeSettings.instance.highlightColor);
  }

  public static bool Toggle(string on, string off, bool enabled) {
    return Button(enabled ? on : off);
  }

  public static bool Toggle(string icon, bool enabled) {
    Color c;

    c = BoltRuntimeSettings.instance.highlightColor;
    c.a = enabled ? 1f : 0.25f;

    if (IconButton(icon, c)) {
      return !enabled;
    }

    return enabled;
  }

  public static void AddButton(string text, List<PropertyDefinition> list, Func<PropertyAssetSettings> newSettings) {
    EditorGUILayout.BeginHorizontal(PaddingStyle(3, 0, 0, 5));

    if (Button("mc_plus")) {
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

    GUIStyle s = new GUIStyle(EditorStyles.boldLabel);
    s.margin.top = 0;
    GUILayout.Label(text, s);

    EditorGUILayout.EndHorizontal();
  }

  public static void HeaderButton(string text, string icon, Action clicked) {
    EditorGUILayout.BeginHorizontal(PaddingStyle(5, 0, 0, 0));

    if (Button(icon)) {
      clicked();
    }

    GUIStyle s = new GUIStyle(EditorStyles.boldLabel);
    s.margin.top = 0;
    GUILayout.Label(text, s);

    EditorGUILayout.EndHorizontal();
  }

  public static int EditPriority(int priority, bool enabled) {
    if (enabled) {
      priority = Mathf.Clamp(IntFieldOverlay(priority, "Priority", GUILayout.Width(75)), 1, 999);
    }
    else {
      BoltEditorGUI.Disabled(() => { EditorGUILayout.TextField("N/A", GUILayout.Width(75)); });
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
    GUIStyle f = new GUIStyle("TextField");
    value = EditorGUILayout.FloatField(value, f, options);

    GUIStyle s = new GUIStyle(EditorStyles.miniLabel);
    s.alignment = TextAnchor.MiddleRight;
    s.contentOffset = new Vector2(-2, 0);
    s.normal.textColor = Color.gray;

    GUI.Label(GUILayoutUtility.GetLastRect(), overlay, s);
    return value;
  }

  public static string TextFieldOverlay(string value, string overlay, params GUILayoutOption[] options) {
    GUIStyle f = new GUIStyle("TextField");
    value = EditorGUILayout.TextField(value, f, options);

    GUIStyle l = new GUIStyle(EditorStyles.miniLabel);
    l.alignment = TextAnchor.MiddleRight;
    l.contentOffset = new Vector2(-2, 0);
    l.normal.textColor = Color.gray;

    GUI.Label(GUILayoutUtility.GetLastRect(), overlay, l);
    return value;
  }

  public static int IntFieldOverlay(int value, string overlay, params GUILayoutOption[] options) {
    GUIStyle f = new GUIStyle("TextField");
    value = EditorGUILayout.IntField(value, f, options);

    GUIStyle l = new GUIStyle(EditorStyles.miniLabel);
    l.alignment = TextAnchor.MiddleRight;
    l.contentOffset = new Vector2(-2, 0);
    l.normal.textColor = Color.gray;

    GUI.Label(GUILayoutUtility.GetLastRect(), overlay, l);
    return value;
  }

  static GUIStyle SettingSectionStyle {
    get {
      GUIStyle s = new GUIStyle(GUIStyle.none);
      s.padding = new RectOffset();
      s.margin = new RectOffset(0, 0, 3, 0);

      return s;
    }
  }

  static void ToggleDisabled() {
    EditorGUI.BeginDisabledGroup(true);
    Toggle(true);
    EditorGUI.EndDisabledGroup();
  }

  public static bool ToggleDropdown(string on, string off, bool enabled) {
    return EditorGUILayout.Popup(enabled ? 0 : 1, new[] { on, off }) == 0;
  }

  public static bool Toggle(bool value) {
    GUIStyle toggle = new GUIStyle("Toggle");
    toggle.margin = new RectOffset();
    toggle.padding = new RectOffset();
    return EditorGUILayout.Toggle(value, toggle, GUILayout.Width(15));
  }

  public static void SettingsSection(string label, Action gui) {
    EditorGUILayout.BeginHorizontal(SettingSectionStyle);

    GUILayout.Label(label, BoltEditorGUI.AccentText);
    EditorGUILayout.EndHorizontal();
    gui();
  }

  public static void SettingsSectionDouble(string left, string right, Action gui) {
    EditorGUILayout.BeginHorizontal(SettingSectionStyle);
    GUILayout.Label(left, BoltEditorGUI.AccentText, GUILayout.ExpandWidth(false));
    GUILayout.FlexibleSpace();
    GUILayout.Label(right, BoltEditorGUI.AccentText, GUILayout.ExpandWidth(false));
    EditorGUILayout.EndHorizontal();

    gui();
  }

  public static void SettingsSectionToggle(string label, ref bool enabled, Action gui, params GUILayoutOption[] options) {
    EditorGUILayout.BeginHorizontal(SettingSectionStyle);

    GUILayout.Label(label, BoltEditorGUI.AccentText, options);

    enabled = Toggle(enabled);

    EditorGUILayout.EndHorizontal();

    EditorGUI.BeginDisabledGroup(!enabled);
    gui();
    EditorGUI.EndDisabledGroup();
  }

  public static FloatCompression EditFloatCompression(FloatCompression c) {
    if (c == null) {
      c = FloatCompression.Default();
    }

    c.Enabled = BoltEditorGUI.Toggle(c.Enabled);

    EditorGUI.BeginDisabledGroup(!c.Enabled);
    c.MinValue = Mathf.Min(IntFieldOverlay(c.MinValue, "Min"), c.MaxValue - 1);
    c.MaxValue = Mathf.Max(IntFieldOverlay(c.MaxValue, "Max"), c.MinValue + 1);
    c.Accuracy = Mathf.Max(FloatFieldOverlay(c.Accuracy, "Accuracy"), 0.001f);
    EditorGUI.EndDisabledGroup();

    return c;
  }

  static void MakeClickable(System.Action onClick) {
    Rect r = GUILayoutUtility.GetLastRect();

    if (IsLeftClick && r.Contains(Event.current.mousePosition)) {
      onClick();
      UseEvent();
    }
  }
}

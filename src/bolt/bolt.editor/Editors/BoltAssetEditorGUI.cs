using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class BoltAssetEditorGUI {
  public const int WIDTH = 100;

  public static BoltAssetPropertyEditMode mode = BoltAssetPropertyEditMode.State;
  public static readonly Color blue = new Color(0 / 255f, 162f / 255f, 232f / 255f);
  public static readonly Color lightBlue = new Color(81f / 255f, 203f / 255f, 255f / 255f);
  public static readonly Color orange = new Color(255f / 255f, 127f / 255f, 39f / 255f);
  public static readonly Color lightGreen = new Color(105f / 255f, 251f / 255f, 9f / 255f);
  public static readonly Color darkGreen = new Color(34f / 255f, 177f / 255f, 76f / 255f);
  public static readonly Color lightOrange = new Color(255f / 255f, 201f / 255f, 12f / 255f);

  public static void Header (string icon, string text) {
    HeaderBackground(() => {
      DrawIcon(icon, new RectOffset(6, 0, 0, 0));

      GUIStyle label;
      label = new GUIStyle(EditorStyles.label);
      label.fontStyle = FontStyle.Bold;
      label.alignment = TextAnchor.UpperLeft;
      label.contentOffset = new Vector2(0, 0);
      GUILayout.Label(text, label, GUILayout.ExpandWidth(false));
    }, 2, 4);
  }

  public static void HeaderBackground (Action contents, int topSpace, int bottomSpace) {
    BeginHeaderBackground(topSpace);
    contents();
    EndHeaderBackground(bottomSpace);
  }

  public static void BeginHeaderBackground (int topSpace) {
    GUILayout.Space(topSpace);
    GUI.color = new Color(.15f, .15f, .15f);
    GUIStyle bg = new GUIStyle();
    bg.normal.background = EditorGUIUtility.whiteTexture;
    bg.padding = new RectOffset(0, 0, 2, 2);
    GUILayout.BeginHorizontal(bg);
    GUI.color = Color.white;

  }

  public static void EndHeaderBackground (int bottomSpace) {
    GUILayout.EndHorizontal();
    GUILayout.Space(bottomSpace);
  }


  public static void HeaderPropertyList (string icon, string text, ref BoltAssetProperty[] properties) {
    BeginHeaderBackground(2);

    BoltAssetEditorGUI.DrawIcon(icon, new RectOffset(6, 0, 0, 0));

    GUIStyle label;
    label = new GUIStyle(EditorStyles.label);
    label.fontStyle = FontStyle.Bold;
    label.alignment = TextAnchor.UpperLeft;
    label.contentOffset = new Vector2(0, 0);
    GUILayout.Label(text, label);

    if (mode != BoltAssetPropertyEditMode.Mecanim) {
      if (BoltAssetEditorGUI.IconButton("properties_add", new RectOffset(0, 6, 0, 0))) {
        ArrayUtility.Add(ref properties, BoltAssetEditorGUI.CreateProperty());
      }
    }

    EndHeaderBackground(2);
  }

  public static void DrawIcon (string name) {
    DrawIcon(name, new RectOffset());
  }

  public static void DrawIcon (string name, RectOffset offset) {
    GUIStyle s;

    s = new GUIStyle(GUIStyle.none);
    s.margin = offset;

    GUILayout.Box(Icon(name), s, GUILayout.Width(16), GUILayout.Height(16));
  }

  public static bool IconButton (string name) {
    return IconButton(name, new RectOffset());
  }

  public static bool IconButton (string name, RectOffset offset) {
    GUIStyle s;

    s = new GUIStyle(GUIStyle.none);
    s.margin = offset;

    return GUILayout.Button(Icon(name), s, GUILayout.Width(16), GUILayout.Height(16));
  }

  public static GUIStyle BoxStyle (int n) {
    GUIStyle s = new GUIStyle("flow node " + n);
    s.padding = new RectOffset();
    s.margin = new RectOffset();
    return s;
  }

  public static Texture2D Gradient (string name) {
    return Resources.Load("gradients/" + name, typeof(Texture2D)) as Texture2D;
  }

  public static Texture2D Icon (string name) {
    return Resources.Load("icons/" + name, typeof(Texture2D)) as Texture2D;
  }

  public static BoltAssetFloatCompression FloatCompressionPopupNoLabel (BoltAssetFloatCompression value) {
    string[] names = Enum.GetNames(typeof(BoltAssetFloatCompression));

    for (int i = 0; i < names.Length; ++i) {
      if (names[i] == BoltAssetFloatCompression.ByteZeroOne.ToString()) {
        names[i] = "Byte [0, 1]";
      }

      if (names[i] == BoltAssetFloatCompression.ByteNegOneOne.ToString()) {
        names[i] = "Byte [-1, 1]";
      }

      if (names[i] == BoltAssetFloatCompression.ByteAngle.ToString()) {
        names[i] = "Byte [0, 360]";
      }

      if (names[i] == BoltAssetFloatCompression.ByteAngle180.ToString()) {
        names[i] = "Byte [-180, 180]";
      }
    }

    return (BoltAssetFloatCompression) EditorGUILayout.Popup((int) value, names);
  }

  public static BoltAssetFloatCompression FloatCompressionPopup (BoltAssetFloatCompression value) {
    return Label("Compression", () => FloatCompressionPopupNoLabel(value));
  }

  public static T ToggleRow<T> (T mask) where T : struct {
    if (!typeof(T).IsEnum) throw new InvalidOperationException("T must be an enumeration");
    return (T) (ValueType) ToggleRow((int) (ValueType) mask, Enum.GetNames(typeof(T)));
  }

  public static T ToggleRow<T> (T mask, params int[] disabled) where T : struct {
    if (!typeof(T).IsEnum) throw new InvalidOperationException("T must be an enumeration");
    return (T) (ValueType) ToggleRow((int) (ValueType) mask, disabled, Enum.GetNames(typeof(T)));
  }

  public static T ToggleRow<T> (int mask) where T : struct {
    if (!typeof(T).IsEnum) throw new InvalidOperationException("T must be an enumeration");
    return (T) (ValueType) ToggleRow(mask, Enum.GetNames(typeof(T)));
  }

  public static T ToggleRow<T> (int mask, params int[] disabled) where T : struct {
    if (!typeof(T).IsEnum) throw new InvalidOperationException("T must be an enumeration");
    return (T) (ValueType) ToggleRow(mask, disabled, Enum.GetNames(typeof(T)));
  }

  public static int ToggleRow (int mask, params string[] flags) {
    return ToggleRow(mask, new int[0], flags);
  }

  public static int ToggleRow (int mask, int[] disabled, params string[] flags) {
    for (int i = 0; i < flags.Length; ++i) {
      flags[i] = Regex.Replace(flags[i], "([A-Z]+)", " $1").Trim();
    }

    for (int i = 0; i < flags.Length; ++i) {
      int flag = 1 << i;
      bool set = (mask & flag) == flag;

      // set color
      GUIStyle style = new GUIStyle(EditorStyles.miniButton);

      if (flags.Length > 1) {
        if (i == 0) {
          style = new GUIStyle(EditorStyles.miniButtonLeft);
        } else if (i + 1 == flags.Length) {
          style = new GUIStyle(EditorStyles.miniButtonRight);
        } else {
          style = new GUIStyle(EditorStyles.miniButtonMid);
        }
      }

      if (EditorGUIUtility.isProSkin) {
        style.normal.textColor = set ? lightBlue : style.normal.textColor;
        style.active.textColor = set ? lightBlue : style.active.textColor;
      } else {
        if (set) {
          GUI.color = lightOrange;
        }
      }

      if (GUILayout.Button(flags[i], style, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20))) {
        if (set) {
          mask &= ~flag;
        } else {
          mask |= flag;
        }
      }

      GUI.color = Color.white;
    }

    GUI.color = Color.white;
    return mask;
  }

  public static void CompileButton (BoltCompilableAsset asset) {
    if (GUI.changed) {
      if (asset.compile == false) {
        EditorPrefs.SetInt("BOLT_UNCOMPILED_COUNT", EditorPrefs.GetInt("BOLT_UNCOMPILED_COUNT", 0) + 1);
      }

      asset.compile = true;

      EditorPrefs.SetBool(BoltScenesWindow.COMPILE_SETTING, true);
      EditorUtility.SetDirty(asset);
    }

    if (asset.compile) {
      //if (GUILayout.Button("Compile", EditorStyles.miniButton)) {
      //BoltUserAssemblyCompiler.Run();
      //}
    }
  }

  public static bool DeleteButton () {
    if (GUILayout.Button("Delete", EditorStyles.miniButton, GUILayout.ExpandWidth(false))) {
      return EditorUtility.DisplayDialog("Confirm", "Do you want to delete this item?", "Yes", "No");
    }

    return false;
  }

  public static BoltAssetProperty[] RemoveDeletedProperties (BoltAssetProperty[] ps) {
    for (int i = 0; i < ps.Length; ++i) {
      if (ps[i].delete) {
        ArrayUtility.RemoveAt(ref ps, i);
        i -= 1;
      }
    }

    return ps;
  }

  public static BoltAssetProperty CreateProperty () {
    BoltAssetProperty p = new BoltAssetProperty();
    p.name = "new_property";
    p.type = BoltAssetPropertyType.Float;
    p.stringSettings = new BoltAssetProperty.StringSettings();
    p.vectorSettings = new BoltAssetProperty.VectorSettings();
    p.quaternionSettings = new BoltAssetProperty.QuaternionSettings();
    p.floatSettings = new BoltAssetProperty.FloatSettings();
    p.intSettings = new BoltAssetProperty.IntSettings();
    return p;
  }

  public static void EditBox (GUIStyle style, params Action[] rows) {
    GUILayout.BeginVertical(style);

    for (int i = 0; i < rows.Length; ++i) {
      GUILayout.BeginHorizontal();
      rows[i]();
      GUILayout.EndHorizontal();
    }

    GUILayout.EndVertical();
    GUILayout.Space(4);
  }

  public static void EditPropertyName (BoltAssetProperty p, bool disabled) {
    EditorGUI.BeginDisabledGroup(disabled);
    GUIStyle s = new GUIStyle("TextField");
    s.normal.textColor = Color.white;

    p.name = EditorGUILayout.TextField(p.name, s);
    EditorGUI.EndDisabledGroup();
  }

  public static void EditPropertySyncMode (BoltAssetProperty p) {
    Label("Sync When", () => p.syncMode = (BoltAssetSyncMode) EditorGUILayout.EnumPopup(p.syncMode));
  }

  public static void EditPropertySyncTarget (BoltAssetProperty p) {
    Label("Target", () => p.syncTarget = ToggleRow<BoltAssetSyncTarget>((int) p.syncTarget));
  }

  static bool DeleteDialog () {
    return EditorUtility.DisplayDialog("Confirm", "Do you want to delete this item?", "Yes", "No");
  }

  public static void EditPropertyDeleteButton (BoltAssetProperty p, bool disabled) {
    EditorGUI.BeginDisabledGroup(disabled);

    if (IconButton("minus", new RectOffset(0, 6, 2, 0))) {
      p.delete = DeleteDialog();
    }

    EditorGUI.EndDisabledGroup();
  }

  public static void EditPropertyOptions (BoltAssetProperty p, bool disabled) {
    if (p.isReference || p.isDefault) {
      return;
    }

    EditorGUI.BeginDisabledGroup(disabled);

    Label("Changed Callback", () => {
      if (EditorGUILayout.Toggle((p.options & BoltAssetPropertyOptions.Notify) == BoltAssetPropertyOptions.Notify)) {
        p.options |= BoltAssetPropertyOptions.Notify;
      } else {
        p.options &= ~BoltAssetPropertyOptions.Notify;
      }
    });

    EditorGUI.EndDisabledGroup();
  }

  public static void EditPropertyEnabled (BoltAssetProperty p) {
    if (IconButton(p.enabled ? "status" : "status-offline", new RectOffset(6, 0, 2, 0))) {
      p.enabled = !p.enabled;
    }
  }

  public static void EditPropertyFoldout (BoltAssetProperty p) {
    var noFoldout = (p.type == BoltAssetPropertyType.Bool) || (p.type == BoltAssetPropertyType.Trigger);

    EditorGUI.BeginDisabledGroup(noFoldout);

    if (IconButton((p.foldout && !noFoldout) ? "arrow_down" : "arrow_right", new RectOffset(0, 0, 2, 0))) {
      p.foldout = !p.foldout;
    }

    EditorGUI.EndDisabledGroup();
  }

  public static bool ArrowButton (string icon) {
    if (EditorGUIUtility.isProSkin == false) {
      icon += "_Dark";
    }

    GUIStyle s = new GUIStyle(EditorStyles.miniButton);
    s.padding = new RectOffset(2, 2, 2, 2);
    Texture2D t = Resources.Load(icon) as Texture2D;
    return GUILayout.Button(t, s, GUILayout.Height(15), GUILayout.Width(20));
  }

  public static void EditPropertyType (BoltAssetProperty p, bool disabled) {
    EditorGUI.BeginDisabledGroup(disabled);

    //Label("Type", () => {
    BoltAssetPropertyType type = p.type;
    p.type = (BoltAssetPropertyType) EditorGUILayout.EnumPopup(p.type, GUILayout.Width(80));

    switch (p.type) {
      case BoltAssetPropertyType.ByteArray:
        if (mode != BoltAssetPropertyEditMode.Event) {
          p.type = type;
        }
        break;

      case BoltAssetPropertyType.Transform:
      case BoltAssetPropertyType.Mecanim:
        p.type = type;
        break;

      case BoltAssetPropertyType.Trigger:
        if (mode != BoltAssetPropertyEditMode.Mecanim) {
          p.type = type;
        }
        break;

      case BoltAssetPropertyType.Custom:
        if (mode != BoltAssetPropertyEditMode.State) {
          p.type = type;
        }
        break;
    }
    //});

    EditorGUI.EndDisabledGroup();
  }

  public static void EditPropertySettings (BoltAssetProperty p, bool disabled) {
    EditorGUI.BeginDisabledGroup(disabled);

    switch (p.type) {
      case BoltAssetPropertyType.Int:
      case BoltAssetPropertyType.Byte:
      case BoltAssetPropertyType.Long:
      case BoltAssetPropertyType.UShort:
        p.intSettings = EditSettings(p, p.intSettings);
        break;

      case BoltAssetPropertyType.Float:
        p.floatSettings = EditSettings(p.floatSettings);

        if (mode == BoltAssetPropertyEditMode.Mecanim) {
          p.assetSettingsMecanim = EditSettings(p.assetSettingsMecanim);
        }
        break;

      case BoltAssetPropertyType.String:
        p.stringSettings = EditSettings(p.stringSettings);
        break;

      case BoltAssetPropertyType.Vector2:
      case BoltAssetPropertyType.Vector3:
      case BoltAssetPropertyType.Vector4:
        p.vectorSettings = EditSettings(p.vectorSettings, p.type);
        break;

      case BoltAssetPropertyType.Quaternion:
        p.quaternionSettings = EditSettings(p.quaternionSettings);
        break;

      case BoltAssetPropertyType.Mecanim:
        p.mecanimSettings = EditSettings(p.mecanimSettings);
        break;

      case BoltAssetPropertyType.Transform:
        p.transformSettings = EditSettings(p.transformSettings);
        break;
    }

    EditorGUI.EndDisabledGroup();
  }

  static BoltAssetProperty.TransformSettings EditSettings (BoltAssetProperty.TransformSettings s) {
    Label("Implementation", () => s.mode = (BoltAssetTransformModes) EditorGUILayout.EnumPopup(s.mode));

    s.posAxes = EditAxes(s.posAxes, BoltAssetPropertyType.Vector3, "Position", () => s.posCompression = FloatCompressionPopupNoLabel(s.posCompression));
    s.rotAxes = EditAxes(s.rotAxes, BoltAssetPropertyType.Vector3, "Rotation", () => s.rotCompression = FloatCompressionPopupNoLabel(s.rotCompression));

    if (s.mode == BoltAssetTransformModes.DeadReckoning) {
      s.velAxes = EditAxes(s.velAxes, BoltAssetPropertyType.Vector3, "Velocity", () => s.velCompression = FloatCompressionPopupNoLabel(s.velCompression));

      Label(" ", () => {
        s.velMode = (BoltAssetTransformVelocityMode) EditorGUILayout.EnumPopup(s.velMode);
        s.velZeroTolerance = FloatFieldOverlay(s.velZeroTolerance, "Tolerance");
      });

      Label("Acceleration", () => {
        s.accMode = (BoltAssetTransformAccelerationMode) EditorGUILayout.EnumPopup(s.accMode);
        EditorGUI.BeginDisabledGroup(s.accMode == BoltAssetTransformAccelerationMode.DontUse);
        s.accZeroTolerance = FloatFieldOverlay(s.accZeroTolerance, "Tolerance");
        EditorGUI.EndDisabledGroup();
      });

      Label(" ", () => {
        EditorGUI.BeginDisabledGroup(s.accMode == BoltAssetTransformAccelerationMode.DontUse);
        s.accCompression = FloatCompressionPopupNoLabel(s.accCompression);
        EditorGUI.EndDisabledGroup();
      });

      s.maxForwardExtrapolation = Label("Max Extrapolation", () => s.maxForwardExtrapolation = IntFieldOverlay(s.maxForwardExtrapolation, "Packets"));
      s.maxInterpTime = Label("Smooth Time", () => s.maxInterpTime = IntFieldOverlay(s.maxInterpTime, "Packets"));
    }

    return s;
  }

  static BoltAssetProperty.AssetSettingsMecanim EditSettings (BoltAssetProperty.AssetSettingsMecanim s) {
    Label("Damping", () => s.interpolationTime = EditorGUILayout.FloatField(s.interpolationTime));
    return s;
  }

  static BoltAssetProperty.StringSettings EditSettings (BoltAssetProperty.StringSettings s) {
    Label("Encoding", () => s.encoding = (BoltAssetStringEncoding) EditorGUILayout.EnumPopup(s.encoding));
    Label("Max Length", () => s.maxLength = EditorGUILayout.IntField(s.maxLength));
    return s;
  }

  static BoltAssetProperty.IntSettings EditSettings (BoltAssetProperty p, BoltAssetProperty.IntSettings s) {
    Label("Bits", () => {
      switch (p.type) {
        case BoltAssetPropertyType.Byte: s.byteBits = Mathf.Clamp(EditorGUILayout.IntField(s.byteBits), 1, 8); break;
        case BoltAssetPropertyType.UShort: s.shortBits = Mathf.Clamp(EditorGUILayout.IntField(s.shortBits), 1, 16); break;
        case BoltAssetPropertyType.Int: s.intBits = Mathf.Clamp(EditorGUILayout.IntField(s.intBits), 1, 32); break;
        case BoltAssetPropertyType.Long: s.longBits = Mathf.Clamp(EditorGUILayout.IntField(s.longBits), 1, 64); break;
      }
    });

    return s;
  }

  static BoltAssetProperty.FloatSettings EditSettings (BoltAssetProperty.FloatSettings s) {
    s.compression = BoltAssetEditorGUI.FloatCompressionPopup(s.compression);

    if (mode == BoltAssetPropertyEditMode.State)
      Label("Interpolate", () => s.interpolate = EditorGUILayout.Toggle(s.interpolate));

    return s;
  }

  static BoltAssetProperty.VectorSettings EditSettings (BoltAssetProperty.VectorSettings s, BoltAssetPropertyType type) {
    s.compression = FloatCompressionPopup(s.compression);
    s.axes = EditAxes(s.axes, type);

    if (mode == BoltAssetPropertyEditMode.State)
      Label("Interpolate", () => s.interpolate = EditorGUILayout.Toggle(s.interpolate));

    return s;
  }

  static BoltAssetProperty.QuaternionSettings EditSettings (BoltAssetProperty.QuaternionSettings s) {
    s.compression = FloatCompressionPopup(s.compression);

    //s.axes = EditAxes(s.axes, BoltAssetPropertyType.Quaternion);

    if (mode == BoltAssetPropertyEditMode.State) {
      Label("Interpolate", () => s.interpolate = EditorGUILayout.Toggle(s.interpolate));
    }

    return s;
  }

  static BoltAssetProperty.MecanimSettings EditSettings (BoltAssetProperty.MecanimSettings s) {
    Label("Mecanim Asset", () => s.mecanimAsset = (BoltMecanimAsset) EditorGUILayout.ObjectField(s.mecanimAsset, typeof(BoltMecanimAsset), false));
    return s;
  }

  internal static void EditSettings (BoltAssetProperty.AssetSettingsCommand s) {
    //Label("Synchronize", () => s.synchronize = EditorGUILayout.Toggle(s.synchronize));
  }

  public static BoltAssetProperty EditProperty (BoltAssetProperty p, BoltAssetPropertyEditMode mode, bool odd) {
    BoltAssetEditorGUI.mode = mode;
    EditorGUILayout.BeginVertical();

    GUIStyle s = new GUIStyle(GUIStyle.none);
    s.normal.background = EditorGUIUtility.whiteTexture;
    s.padding = new RectOffset(0, 0, 0, 2);

    float c = odd ? 0.25f : 0.3f;

    GUI.color = new Color(c, c, c);

    EditorGUILayout.BeginHorizontal(s);

    GUI.color = Color.white;

    BoltAssetEditorGUI.EditPropertyEnabled(p);
    BoltAssetEditorGUI.EditPropertyFoldout(p);

    EditorGUI.BeginDisabledGroup(p.enabled == false);
    BoltAssetEditorGUI.EditPropertyName(p, mode == BoltAssetPropertyEditMode.Mecanim || p.isDefault);
    BoltAssetEditorGUI.EditPropertyType(p, mode == BoltAssetPropertyEditMode.Mecanim || p.isDefault);

    EditorGUI.BeginDisabledGroup(p.isDefault);
    if (IconButton("move_down", new RectOffset(0, 4, 2, 0))) { p.shift = 1; }
    if (IconButton("move_up", new RectOffset(0, 4, 2, 0))) { p.shift = -1; }
    EditorGUI.EndDisabledGroup();

    BoltAssetEditorGUI.EditPropertyDeleteButton(p, mode == BoltAssetPropertyEditMode.Mecanim || p.isDefault);
    EditorGUILayout.EndHorizontal();

    if (p.foldout) {
      if (mode == BoltAssetPropertyEditMode.State) {
        BoltAssetEditorGUI.EditPropertySyncMode(p);
        BoltAssetEditorGUI.EditPropertySyncTarget(p);
      }

      if (mode == BoltAssetPropertyEditMode.Command) {
        EditSettings(p.assetSettingsCommand);
      }

      if (mode == BoltAssetPropertyEditMode.State) {
        BoltAssetEditorGUI.EditPropertyOptions(p, false);
      }

      BoltAssetEditorGUI.EditPropertySettings(p, false);
    }

    EditorGUI.EndDisabledGroup();
    EditorGUILayout.EndVertical();

    return p;
  }

  public static BoltAssetProperty[] EditPropertyArray (BoltAssetProperty[] ps, BoltAssetPropertyEditMode mode, bool group) {
    for (int i = 0; i < ps.Length; ++i) {
      ps[i] = EditProperty(ps[i], mode, (i & 1) == 1);

      if (ps[i].shift != 0) {
        int s = i + ps[i].shift;

        ps[i].shift = 0;

        if (s >= 0 && s < ps.Length) {
          var a = ps[i];
          var b = ps[s];

          ps[s] = a;
          ps[i] = b;
        }
      }
    }

    return BoltAssetEditorGUI.RemoveDeletedProperties(ps);
  }

  static BoltAssetAxes EditAxes (BoltAssetAxes axes, BoltAssetPropertyType type) {
    return EditAxes(axes, type, "Axes", null);
  }

  static BoltAssetAxes EditAxes (BoltAssetAxes axes, BoltAssetPropertyType type, string label, Action after) {
    List<string> axesNames = new List<string>() { "X", "Y" };

    switch (type) {
      case BoltAssetPropertyType.Vector3:
      case BoltAssetPropertyType.Quaternion: axesNames.Add("Z"); break;
      case BoltAssetPropertyType.Vector4: axesNames.Add("Z"); axesNames.Add("W"); break;
    }

    EditorGUILayout.BeginHorizontal();
    BoltAssetAxes result = Label(label, () => (BoltAssetAxes) ToggleRow((int) axes, axesNames.ToArray()));
    if (after != null) after();
    EditorGUILayout.EndHorizontal();
    return result;
  }

  public static void Label (string label, GUIStyle style, Action action) {
    EditorGUILayout.BeginHorizontal();
    GUILayout.Label(label, style);
    action();
    EditorGUILayout.EndHorizontal();
  }

  public static void Label (string label, Action action) {
    EditorGUILayout.BeginHorizontal();
    GUILayout.Label(label, GUILayout.Width(150));
    try {
      action();
    } catch { };

    EditorGUILayout.EndHorizontal();
  }

  public static void DarkLabel (string label, Action action) {
    EditorGUILayout.BeginHorizontal();
    GUIStyle s = new GUIStyle(EditorStyles.label);
    s.normal.textColor = new Color(0.125f, 0.125f, 0.125f);
    EditorGUILayout.PrefixLabel(label, "Button", s);
    action();
    EditorGUILayout.EndHorizontal();
  }

  static T Label<T> (string label, Func<T> action) {
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PrefixLabel(label);
    T result = action();
    EditorGUILayout.EndHorizontal();
    return result;
  }

  public static float FloatFieldOverlay (float value, string overlay) {
    value = EditorGUILayout.FloatField(value);

    GUIStyle s = new GUIStyle("Label");
    s.alignment = TextAnchor.MiddleRight;
    s.normal.textColor = Color.gray;

    GUI.Label(GUILayoutUtility.GetLastRect(), overlay, s);
    return value;
  }

  public static int IntFieldOverlay (int value, string overlay) {
    value = EditorGUILayout.IntField(value);

    GUIStyle s = new GUIStyle("Label");
    s.alignment = TextAnchor.MiddleRight;
    s.normal.textColor = Color.gray;

    GUI.Label(GUILayoutUtility.GetLastRect(), overlay, s);
    return value;
  }
}

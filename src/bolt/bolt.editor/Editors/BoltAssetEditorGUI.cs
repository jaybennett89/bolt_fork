using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;

public static class BoltAssetEditorGUI {
  public const int WIDTH = 100;

  public static readonly Color lightOrange = new Color(255f / 255f, 201f / 255f, 12f / 255f);


  public static void HeaderBackground(Action contents, int topSpace, int bottomSpace) {
    BeginHeaderBackground(topSpace);
    contents();
    EndHeaderBackground(bottomSpace);
  }

  public static void BeginHeaderBackground(int topSpace) {
    GUILayout.BeginHorizontal(GUIStyle.none);
  }

  public static void EndHeaderBackground(int bottomSpace) {
    GUILayout.EndHorizontal();
    GUILayout.Space(bottomSpace);
  }

  public static GUIStyle BoxStyle(int n) {
    GUIStyle s = new GUIStyle("flow node " + n);
    s.padding = new RectOffset();
    s.margin = new RectOffset();
    return s;
  }

  public static T ToggleRow<T>(T mask) where T : struct {
    if (!typeof(T).IsEnum) throw new InvalidOperationException("T must be an enumeration");
    return (T)(ValueType)ToggleRow((int)(ValueType)mask, Enum.GetNames(typeof(T)));
  }

  public static T ToggleRow<T>(T mask, params int[] disabled) where T : struct {
    if (!typeof(T).IsEnum) throw new InvalidOperationException("T must be an enumeration");
    return (T)(ValueType)ToggleRow((int)(ValueType)mask, disabled, Enum.GetNames(typeof(T)));
  }

  public static T ToggleRow<T>(int mask) where T : struct {
    if (!typeof(T).IsEnum) throw new InvalidOperationException("T must be an enumeration");
    return (T)(ValueType)ToggleRow(mask, Enum.GetNames(typeof(T)));
  }

  public static T ToggleRow<T>(int mask, params int[] disabled) where T : struct {
    if (!typeof(T).IsEnum) throw new InvalidOperationException("T must be an enumeration");
    return (T)(ValueType)ToggleRow(mask, disabled, Enum.GetNames(typeof(T)));
  }

  public static int ToggleRow(int mask, params string[] flags) {
    return ToggleRow(mask, new int[0], flags);
  }

  public static int ToggleRow(int mask, int[] disabled, params string[] flags) {
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
        }
        else if (i + 1 == flags.Length) {
          style = new GUIStyle(EditorStyles.miniButtonRight);
        }
        else {
          style = new GUIStyle(EditorStyles.miniButtonMid);
        }
      }

      if (EditorGUIUtility.isProSkin) {
        style.normal.textColor = set ? BoltEditorGUI.HighlightColor : style.normal.textColor;
        style.active.textColor = set ? BoltEditorGUI.HighlightColor : style.active.textColor;
      }
      else {
        GUI.color = set ? BoltEditorGUI.HighlightColor : Color.white;
      }

      if (GUILayout.Button(flags[i], style, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20))) {
        if (set) {
          mask &= ~flag;
        }
        else {
          mask |= flag;
        }
      }

      GUI.color = Color.white;
    }

    GUI.color = Color.white;
    return mask;
  }

  public static void EditBox(GUIStyle style, params Action[] rows) {
    GUILayout.BeginVertical(style);

    for (int i = 0; i < rows.Length; ++i) {
      GUILayout.BeginHorizontal();
      rows[i]();
      GUILayout.EndHorizontal();
    }

    GUILayout.EndVertical();
    GUILayout.Space(4);
  }

  public static void Label(string label, Action action) {
    EditorGUILayout.BeginHorizontal();
    GUILayout.Label(label, GUILayout.Width(150));
    try {
      action();
    }
    catch { };

    EditorGUILayout.EndHorizontal();
  }

  static T Label<T>(string label, Func<T> action) {
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PrefixLabel(label);
    T result = action();
    EditorGUILayout.EndHorizontal();
    return result;
  }
}

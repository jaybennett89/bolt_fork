using System;
using System.Collections;
using UnityEngine;

public static class BoltGUI {
  public static readonly Color blue = new Color(0 / 255f, 162f / 255f, 232f / 255f);
  public static readonly Color lightBlue = new Color(0f / 255f, 232f / 255f, 226f / 255f);
  public static readonly Color orange = new Color(255f / 255f, 127f / 255f, 39f / 255f);
  public static readonly Color lightGreen = new Color(105f / 255f, 251f / 255f, 9f / 255f);
  public static readonly Color darkGreen = new Color(34f / 255f, 177f / 255f, 76f / 255f);

  public static Texture2D whiteTexture {
    get { return Resources.Load("BoltConsoleWhiteTexture") as Texture2D; }
  }

  public struct Coll<T> {
    public string name;
    public Action<T> drawer;
  }

  public struct TableStyle {
    public GUIStyle tableStyle;
    public GUIStyle labelStyle;
  }

  public static Coll<T> Column<T> (string name, Func<T, object> selector) {
    return Column<T>(name, v => GUILayout.Label(selector(v).ToString()));
  }

  public static Coll<T> Column<T> (string name, Action<T> drawer) {
    return new Coll<T>() { name = name, drawer = drawer };
  }

  public static void Table<T> (IEnumerable objects, TableStyle style, params Coll<T>[] columns) {
    GUILayout.BeginHorizontal(style.tableStyle ?? GUI.skin.box);

    for (int i = 0; i < columns.Length; ++i) {
      GUILayout.BeginVertical();
      GUILayout.Label(columns[i].name, style.labelStyle ?? GUI.skin.label);

      foreach (object obj in objects) {
        columns[i].drawer((T) obj);
      }

      GUILayout.EndVertical();
    }

    GUILayout.EndHorizontal();
  }

  public static void Table<T> (IEnumerable objects, params Coll<T>[] columns) {
    Table<T>(objects, new TableStyle(), columns);
  }
}

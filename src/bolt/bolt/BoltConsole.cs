using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

/// <summary>
/// Renders a toggleable console at the top of the screen
/// </summary>
public class BoltConsole : MonoBehaviour {
  [Obsolete("This property is not used anymore")]
  public static bool writable {
    get;
    set;
  }

  struct Line {
    public Color color;
    public string text;
  }

  static volatile int _changed = 0;
  static readonly object _lock = new object();
  static readonly BoltRingBuffer<Line> _lines = new BoltRingBuffer<Line>(1024);
  static readonly BoltRingBuffer<Line> _linesRender = new BoltRingBuffer<Line>(1024);
  //static readonly Dictionary<string, Action<string>> _commands = new Dictionary<string, Action<string>>();

  static int height {
    get { return _linesRender.count * 20; }
  }

  /// <summary>
  /// Write one line to the console
  /// </summary>
  /// <param name="line">Text to write</param>
  /// <param name="color">Color of the text</param>
  public static void Write (string line, Color color) {
    lock (_lock) {
      if (line.Contains("\r") || line.Contains("\n")) {
        foreach (string l in Regex.Split(line, "[\r\n]+")) {
          WriteReal(l, color);
        }
      } else {
        WriteReal(line, color);
      }
    }

    // tell main thread we wrote stuff
#pragma warning disable 0420
    Interlocked.Increment(ref _changed);
#pragma warning restore 0420
  }

  /// <summary>
  /// Write one line to the console
  /// </summary>
  /// <param name="line">Text to write</param>
  public static void Write (string line) {
    Write(line, Color.white);
  }

  static void WriteReal (string line, Color color) {
    // free one slot up
    if (_lines.full) { _lines.Dequeue(); }

    // put line 
    _lines.Enqueue(new Line { text = line, color = color });
  }

  //static public void RegisterCommand (string cmd, Action<string> callback) {
  //  _commands.Add(cmd, callback);
  //}

  string input;
  GUIStyle text;
  Texture2D background;

  [SerializeField]
  float consoleHeight = 0.5f;

  [SerializeField]
  int lineHeight = 11;

  [SerializeField]
  bool visible = true;

  [SerializeField]
  float backgroundTransparency = 0.5f;

  [SerializeField]
  int padding = 0;

  [SerializeField]
  int fontSize = 10;

  [SerializeField]
  Color inputColor = Color.white;

  [SerializeField]
  KeyCode toggleKey = KeyCode.Tab;

  [SerializeField]
  int inset = 10;

  [SerializeField]
  bool autoFocus = true;

  void Awake () {
    writable = true;

    switch (Application.platform) {
      case RuntimePlatform.Android:
      case RuntimePlatform.IPhonePlayer:
        fontSize *= 2;
        lineHeight *= 2;
        break;
    }
  }

  void OnGUI () {
    if (Event.current.Equals(Event.KeyboardEvent(toggleKey.ToString()))) {
      visible = !visible;
    }

    if (visible == false) {
      return;
    }

    if (!background) {
      background = Resources.Load("BoltConsoleWhiteTexture", typeof(Texture2D)) as Texture2D;
    }

    if (text == null) {
      text = new GUIStyle();
      text.normal.textColor = Color.white;
      text.fontStyle = FontStyle.Bold;
      text.fontSize = fontSize;
      text.alignment = TextAnchor.UpperLeft;
    }

    // update if we have changed
    if (_changed > 0) {
      int c = _changed;

      do {
        c = _changed;

        lock (_lock) {
          _lines.CopyTo(_linesRender);
        }

#pragma warning disable 0420
      } while (Interlocked.Add(ref _changed, -c) > 0);
#pragma warning restore 0420
    }

    // how many lines to render at most
    int lines = Mathf.Max(1, ((int) (Screen.height * consoleHeight)) / lineHeight) + 1;

    // background
    GUI.color = Color.black * backgroundTransparency;
    GUI.DrawTexture(new Rect(inset, inset, Screen.width - (inset * 2), (lines * lineHeight) + (padding * 2)), background);
    GUI.color = Color.white;

    // draw lines
    for (int i = 0; i < (lines - 1); ++i) {
      int m = Mathf.Min(_linesRender.count, (lines - 1));

      if (i < _linesRender.count) {
        Line l = _linesRender[_linesRender.count - m + i];
        GUI.color = l.color;
        GUI.Label(GetRect(i), l.text, text);
        GUI.color = Color.white;
      }
    }

    // execute commands
    if (Event.current.Equals(Event.KeyboardEvent(KeyCode.Return.ToString()))) {
      if (input.Length > 0) {
        EvalCommand(input);
      }

      input = "";
    }

    // input box
    //if (autoFocus) { GUI.SetNextControlName("BoltConsoleInput"); }
    //GUI.color = inputColor;
    //input = GUI.TextField(GetRect(lines - 1), input ?? "", text);
    //input = Regex.Replace(input, "[\r\n]+", " ");
    //GUI.color = Color.white;
    //if (autoFocus) { GUI.FocusControl("BoltConsoleInput"); }
  }

  Rect GetRect (int line) {
    return new Rect(inset + padding, inset + padding + (line * lineHeight), Screen.width - (padding * 2) - (inset * 2), lineHeight);
  }

  void EvalCommand (string cmd) {
    //string[] parts = cmd.Trim().Split(new[] { ' ' }, 2);
    //Action<string> callback = null;

    //if (_commands.TryGetValue(parts[0], out callback)) {
    //  if (parts.Length == 1) { callback(""); }
    //  if (parts.Length == 2) { callback(parts[1]); }
    //}
  }
}

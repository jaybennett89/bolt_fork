using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

public class BoltConsole : MonoBehaviour {
  internal struct Line {
    public Color color;
    public string text;
  }

  static volatile int _changed = 0;
  static readonly object _mutex = new object();
  static readonly BoltRingBuffer<Line> _lines = new BoltRingBuffer<Line>(1024);
  static readonly BoltRingBuffer<Line> _linesRender = new BoltRingBuffer<Line>(1024);

  static internal int LinesCount {
    get { return _linesRender.count; }
  }

  static internal IEnumerable<Line> Lines {
    get { return _linesRender; }
  }

  /// <summary>
  /// Write one line to the console
  /// </summary>
  /// <param name="line">Text to write</param>
  /// <param name="color">Color of the text</param>
  public static void Write(string line, Color color) {
    lock (_mutex) {
      if (line.Contains("\r") || line.Contains("\n")) {
        foreach (string l in Regex.Split(line, "[\r\n]+")) {
          WriteReal(l, color);
        }
      }
      else {
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
  public static void Write(string line) {
    Write(line, Color.white);
  }

  public static void WriteReal(string line, Color color) {
    // free one slot up
    if (_lines.full) { _lines.Dequeue(); }

    // put line 
    _lines.Enqueue(new Line { text = line, color = color });
  }

  [SerializeField]
  float consoleHeight = 0.5f;

  [SerializeField]
  int lineHeight = 11;

  [SerializeField]
  internal bool visible = true;

  [SerializeField]
  internal KeyCode toggleKey = KeyCode.Tab;

  [SerializeField]
  float backgroundTransparency = 0.75f;

  [SerializeField]
  int padding = 10;

  [SerializeField]
  int fontSize = 10;

  [SerializeField]
  int inset = 10;

  void Awake() {
    switch (Application.platform) {
      case RuntimePlatform.Android:
      case RuntimePlatform.IPhonePlayer:
        fontSize *= 2;
        lineHeight *= 2;
        break;
    }
  }

  static internal void Clear() {
    lock (_mutex) {
      _lines.Clear();
      _linesRender.Clear();
    }
  }

  static internal void LinesRefresh() {
    // update if we have changed
    if (_changed > 0) {
      int c = _changed;

      do {
        c = _changed;

        lock (_mutex) {
          _lines.CopyTo(_linesRender);
        }

#pragma warning disable 0420
      } while (Interlocked.Add(ref _changed, -c) > 0);
#pragma warning restore 0420
    }
  }

  void OnGUI() {
    if ((Event.current.type == EventType.KeyDown) && (Event.current.keyCode == toggleKey)) {
      visible = !visible;
    }

    if (visible == false) {
      return;
    }
    LinesRefresh();

    // how many lines to render at most
    int lines = Mathf.Max(1, ((int)(Screen.height * consoleHeight)) / lineHeight);

    // background
    Bolt.DebugInfo.DrawBackground(new Rect(inset, inset, Screen.width - (inset * 2), ((lines - 1) * lineHeight) + (padding * 2)));

    // draw lines
    for (int i = 0; i < lines; ++i) {
      int m = Mathf.Min(_linesRender.count, (lines - 1));

      if (i < _linesRender.count) {
        Line l = _linesRender[_linesRender.count - m + i];
        GUI.Label(GetRect(i), l.text, Bolt.DebugInfo.LabelStyleColor(l.color));
      }
    }
  }

  Rect GetRect(int line) {
    return new Rect(inset + padding, inset + padding + (line * lineHeight), Screen.width - (inset * 2) - (padding * 2), lineHeight);
  }
}

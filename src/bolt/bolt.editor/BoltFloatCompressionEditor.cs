using Bolt.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

static class BoltFloatCompressionEditor {
  static List<float> sentValues = new List<float>();
  static List<float> localValues = new List<float>();

  static Material material;
  static FloatCompression target;

  static int simulateFrame = 0;
  static float secondsForFullTurn = 2f;

  static float timeStart;
  static float timeSimulated;

  static float timeElapsed {
    get { return Time.realtimeSinceStartup - timeStart; }
  }

  static public FloatCompression Target {
    get { return target; }
    set {
      target = value;

      timeStart = 0;
      timeSimulated = 0;

      simulateFrame = 0;

      sentValues.Clear();
      localValues.Clear();
    }
  }

  static public bool HasTarget {
    get { return Target != null; }
  }

  static Material Material() {
    if (material == null) {
      material = new Material("Shader \"Lines/Colored Blended\" {" +
                                  "SubShader { Pass { " +
                                  "    Blend SrcAlpha OneMinusSrcAlpha " +
                                  "    ZWrite Off Cull Off Fog { Mode Off } " +
                                  "    BindChannels {" +
                                  "      Bind \"vertex\", vertex Bind \"color\", color }" +
                                  "} } }");

      material.hideFlags = HideFlags.HideAndDontSave;
      material.shader.hideFlags = HideFlags.HideAndDontSave;
    }

    return material;
  }

  public static void Update(Action repaint) {
    if (HasTarget) {
      var cfg = BoltRuntimeSettings.instance.GetConfigCopy();

      if (timeStart == 0) {
        timeStart = Time.realtimeSinceStartup;
      }
      else {
        float fpsInv = 1f / cfg.framesPerSecond;

        while ((timeSimulated + fpsInv) < timeElapsed) {
          Simulate(cfg);
          timeSimulated += fpsInv;
        }

        repaint();
      }
    }
  }

  public static void Draw(Rect r) {
    // gui group
    GUI.BeginGroup(r);

    // setup GL
    GL.PushMatrix();
    Material().SetPass(0);
    GL.LoadPixelMatrix();

    // draw background overlay
    DrawBackground(r);

    // display
    float w2 = r.width / 2;
    float h2 = r.height / 2;
    float height = Mathf.Min(h2 / 1.25f, w2 / 1.25f);
    
    FloatCompressionSimulator fc = new FloatCompressionSimulator();
    fc.Bits = Target.Bits;
    fc.MinValue = Target.MinValue;
    fc.MaxValue = Target.MaxValue;

    localValues = new List<float>();

    for (int i = 0; i < (1 << Target.Bits); ++i) {
      localValues.Add(fc.Read(i));
    }

    DrawCircle(r, localValues, Color.white, -1, +1, -height / 1.05f, -height / 5);
    //DrawCircle(r, sentValues, BoltEditorGUI.LightGreen, -2, +2, -height, -height / 1.1f);

    // reset GL
    GL.PopMatrix();

    // end gui group
    GUI.EndGroup();

    GUILayout.BeginArea(new Rect(5, 5, r.width - 10, r.height - 10));
    GUILayout.BeginHorizontal();

    target.MinValue = BoltEditorGUI.IntFieldOverlay(target.MinValue, "Min Value");
    target.MaxValue = BoltEditorGUI.IntFieldOverlay(target.MaxValue, "Max Value");
    target.Accuracy = BoltEditorGUI.FloatFieldOverlay(target.Accuracy, "Accuracy");
    secondsForFullTurn = BoltEditorGUI.FloatFieldOverlay(secondsForFullTurn, "Seconds For Full 360* Turn");

    if (GUILayout.Button("Close", EditorStyles.miniButton)) {
      Target = null;
    }

    GUILayout.EndHorizontal();
    GUILayout.EndArea();
  }

  static void Simulate(BoltConfig cfg) {
    while (localValues.Count > cfg.framesPerSecond) {
      localValues.RemoveAt(0);
    }

    while (sentValues.Count > (cfg.framesPerSecond / cfg.serverSendRate)) {
      sentValues.RemoveAt(0);
    }

    var range = Target.MaxValue - Target.MinValue;
    var val = ((Time.realtimeSinceStartup * (360f / secondsForFullTurn)) % range) + Target.MinValue;

    if ((simulateFrame % cfg.serverSendRate) == 0) {
      sentValues.Add(val);
    }

    FloatCompressionSimulator fc = new FloatCompressionSimulator();
    fc.Bits = Target.Bits;
    fc.MinValue = Target.MinValue;
    fc.MaxValue = Target.MaxValue;

    localValues.Add(fc.Read(fc.Pack(val)));

    ++simulateFrame;
  }

  static void DrawBackground(Rect r) {
    GL.Begin(GL.TRIANGLES);
    GL.Color(new Color(0, 0, 0, 0.75f));

    GL.Vertex3(0, 0, 0);
    GL.Vertex3(0, r.height, 0);
    GL.Vertex3(r.width, r.height, 0);

    GL.Vertex3(r.width, r.height, 0);
    GL.Vertex3(r.width, 0, 0);
    GL.Vertex3(0, 0, 0);
    GL.End();
  }

  static void DrawCircle(Rect r, List<float> values, Color color, float left, float right, float top, float bottom) {
    for (int i = 0; i < values.Count; ++i) {
      GL.Begin(GL.TRIANGLES);
      GL.Color(color);

      float w2 = r.width / 2;
      float h2 = r.height / 2;
      float height = Mathf.Min(h2 / 2, w2 / 2);

      var center = new Vector3(w2, h2);
      var rotation = Quaternion.Euler(0, 0, values[i]);

      var topLeft = rotation * new Vector2(left, top);
      var topRight = rotation * new Vector2(right, top);

      var botLeft = rotation * new Vector2(left, bottom);
      var botRight = rotation * new Vector2(right, bottom);

      topLeft += center;
      topRight += center;
      botLeft += center;
      botRight += center;

      GL.Vertex(topLeft);
      GL.Vertex(topRight);
      GL.Vertex(botRight);

      GL.Vertex(botRight);
      GL.Vertex(botLeft);
      GL.Vertex(topLeft);

      GL.End();
    }
  }

  struct FloatCompressionSimulator {
    public int Bits;
    public int MinValue;
    public int MaxValue;

    public int Pack(float value) {
      var bitsMask = (1 << Bits) - 1;
      float adjust = (float)-MinValue;
      float bitsRange = bitsMask;
      float valueRange = MaxValue - MinValue;
      return ((int)((value + adjust) * (bitsRange / valueRange))) & bitsMask;
    }

    public float Read(int packed) {
      float adjust = (float)-MinValue;
      float bitsRange = (1 << Bits) - 1;
      float valueRange = MaxValue - MinValue;
      return (packed * (valueRange / bitsRange)) + MinValue;
    }
  }
}

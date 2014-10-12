using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class EditorTests : EditorWindow {

  struct FloatCompression {
    public int Bits;
    public int MinValue;
    public int MaxValue;

    public int Pack(float value) {
      var bitsMask = (1 << Bits) - 1;
      float adjust = (float)-MinValue;
      float bitsRange = 1 << Bits;
      float valueRange = MaxValue - MinValue;
      return ((int)((value + adjust) * (bitsRange / valueRange))) & bitsMask;
    }

    public float Read(int packed) {
      float adjust = (float)-MinValue;
      float bitsRange = 1 << Bits;
      float valueRange = MaxValue - MinValue;
      return (packed * (valueRange / bitsRange)) + MinValue;
    }
  }

  public static Material material = null;

  [MenuItem("Window/Test")]
  public static void Open() {
    EditorWindow.GetWindow<EditorTests>().Show();
  }

  public static void CreateMaterial() {
    if (material != null)
      return;

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

  static List<float> sentValues = new List<float>();
  static List<float> localValues = new List<float>();

  static int simulateFrame = 0;
  static FloatCompression fc = new FloatCompression();

  static float lastRender = 0f;
  static float secondsForFullTurn = 2f;

  static float timeStart;
  static float timeSimulated;

  float timeElapsed {
    get { return Time.realtimeSinceStartup - timeStart; }
  }

  void Update() {
    var cfg = BoltRuntimeSettings.instance.GetConfigCopy();

    if (timeStart == 0) {
      timeStart = Time.realtimeSinceStartup;
    }
    else {
      float fpsInv = 1f / cfg.framesPerSecond;

      while ((timeSimulated + fpsInv) < timeElapsed) {
        Simulate();
        timeSimulated += fpsInv;
      }

      Repaint();
    }
  }

  void Simulate() {
    var cfg = BoltRuntimeSettings.instance.GetConfigCopy();

    while (localValues.Count > cfg.framesPerSecond) {
      localValues.RemoveAt(0);
    }

    while (sentValues.Count > (cfg.framesPerSecond / cfg.serverSendRate)) {
      sentValues.RemoveAt(0);
    }

    fc.Bits = 4;
    fc.MinValue = -80;
    fc.MaxValue = +80;

    var range = fc.MaxValue - fc.MinValue;

    var val = ((Time.realtimeSinceStartup * (360f / secondsForFullTurn)) % range) + fc.MinValue;


    if ((simulateFrame % cfg.serverSendRate) == 0) {
      sentValues.Add(val);
    }

    localValues.Add(fc.Read(fc.Pack(val)));
    ++simulateFrame;
  }

  void OnGUI() {
    CreateMaterial();

    GUI.BeginGroup(new Rect(0, 0, position.width, position.height));

    GL.PushMatrix();

    material.SetPass(0);

    GL.LoadPixelMatrix();

    GL.Begin(GL.TRIANGLES);
    GL.Color(new Color(0, 0, 0, 0.5f));

    GL.Vertex3(0, 0, 0);
    GL.Vertex3(0, position.height, 0);
    GL.Vertex3(position.width, position.height, 0);

    GL.Vertex3(position.width, position.height, 0);
    GL.Vertex3(position.width, 0, 0);
    GL.Vertex3(0, 0, 0);
    GL.End();

    float w2 = position.width / 2;
    float h2 = position.height / 2;
    float height = Mathf.Min(h2 / 1.25f, w2 / 1.25f);

    DrawCircle(localValues, Color.white, -2, -1, -height / 1.05f, -height / 5);
    DrawCircle(sentValues, BoltEditorGUI.LightGreen, -2, -1, -height, -height / 1.1f);

    GL.PopMatrix();

    GUI.EndGroup();
  }

  void DrawCircle(List<float> values, Color color, float left, float right, float top, float bottom) {
    for (int i = 0; i < values.Count; ++i) {
      GL.Begin(GL.TRIANGLES);
      GL.Color(color);

      float w2 = position.width / 2;
      float h2 = position.height / 2;
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
}

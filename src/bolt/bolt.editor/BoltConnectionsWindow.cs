using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class BoltConnectionsWindow : EditorWindow {
  GUIStyle 
    boldLabel,
    detailsLabel;

  float _lastRepaint = 0f;
  Vector2 _scrollPosition;

  void Update () {
    if (_lastRepaint + 0.1f < Time.realtimeSinceStartup) {
      _lastRepaint = Time.realtimeSinceStartup;
      Repaint();
    }
  }

  void OnEnable () {
    title = name = "Bolt Remotes";
    _lastRepaint = 0f;
    _scrollPosition = Vector2.zero;
  }

  void OnGUI () {
    if (boldLabel == null) {
      boldLabel = new GUIStyle(GUI.skin.label);
      boldLabel.fontStyle = FontStyle.Bold;
    }

    if (detailsLabel == null) {
      detailsLabel = new GUIStyle(GUI.skin.label);
      detailsLabel.normal.textColor = new Color(0f / 255f, 232f / 255f, 226f / 255f);
    }

    _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

    Connections();

    GUILayout.EndScrollView();
  }

  void Connections() {
    GUILayout.Space(4);

    BoltEditorGUI.Header("Connections", "mc_connection");

    GUIStyle sceneStyle = "TE NodeBox";
    sceneStyle.padding = new RectOffset(5, 5, 5, 5);

    foreach (var c in BoltCore.connections) {
      GUILayout.BeginHorizontal(sceneStyle);
      GUILayout.Label(c.udpConnection.RemoteEndPoint.ToString(), GUI.skin.label, GUILayout.Width(150));

      BoltAssetEditorGUI.DrawIconColorized("mc_latency");
      GUILayout.Label(Mathf.FloorToInt(c.udpConnection.NetworkPing * 1000f).ToString() + " ms");

      BoltAssetEditorGUI.DrawIconColorized("mc_download");
      GUILayout.Label(((c.bitsPerSecondIn >> 3) / 1000f).ToString() + " kb/s");

      BoltAssetEditorGUI.DrawIconColorized("mc_upload");
      GUILayout.Label(((c.bitsPerSecondOut >> 3) / 1000f).ToString() + " kb/s");

      GUILayout.EndHorizontal();
    }
  }

  void ConnectionButtons (BoltConnection c) {
    GUILayout.BeginHorizontal();

    if (GUILayout.Button("Disconnect", GUILayout.ExpandWidth(false))) {
      c.Disconnect();
    }

    GUILayout.EndHorizontal();
  }
}

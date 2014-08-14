using System;
using System.Collections.Generic;
using System.Linq;
using UdpKit;
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
    title = name = "Connections";
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
      detailsLabel.normal.textColor = BoltGUI.lightBlue;
    }

    _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

    Connections();

    GUILayout.EndScrollView();
  }

  void Connections () {
    BoltGUI.Table<BoltConnection>(BoltCore.connections, new BoltGUI.TableStyle { labelStyle = boldLabel },
      BoltGUI.Column<BoltConnection>("Address", c => {
        var address = c.udpConnection.RemoteEndPoint.ToString();
        GUILayout.Label(address, GUI.skin.label);
      }),
      BoltGUI.Column<BoltConnection>("Ping (ms)", c => Mathf.FloorToInt(c.udpConnection.NetworkPing * 1000f)),
      BoltGUI.Column<BoltConnection>("OUT (kb/s)", c => (c.bitsPerSecondOut >> 3) / 1000f),
      BoltGUI.Column<BoltConnection>("IN (kb/s)", c => (c.bitsPerSecondIn >> 3) / 1000f),
      BoltGUI.Column<BoltConnection>("", c => ConnectionButtons(c))
    );
  }

  void ConnectionButtons (BoltConnection c) {
    GUILayout.BeginHorizontal();

    if (GUILayout.Button("Disconnect", GUILayout.ExpandWidth(false))) {
      c.Disconnect();
    }

    GUILayout.EndHorizontal();
  }
}

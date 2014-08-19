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
      detailsLabel.normal.textColor = BoltGUI.lightBlue;
    }

    _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

    Connections();

    GUILayout.EndScrollView();
  }

  void Connections () {
    GUILayout.Space(2);

    BoltAssetEditorGUI.Header("network", "Connections");

    GUIStyle sceneStyle = "TE NodeBox";
    sceneStyle.padding = new RectOffset(5, 5, 5, 5);

    foreach (var c in BoltCore.connections) {
      GUILayout.BeginHorizontal(sceneStyle);


      GUILayout.Label(c.udpConnection.RemoteEndPoint.ToString(), GUI.skin.label, GUILayout.Width(150));
      
      BoltAssetEditorGUI.DrawIcon("rtt");
      GUILayout.Label(Mathf.FloorToInt(c.udpConnection.NetworkPing * 1000f).ToString() + " ms");

      BoltAssetEditorGUI.DrawIcon("download");
      GUILayout.Label(((c.bitsPerSecondIn >> 3) / 1000f).ToString() + " kb/s");

      BoltAssetEditorGUI.DrawIcon("upload");
      GUILayout.Label(((c.bitsPerSecondOut >> 3) / 1000f).ToString() + " kb/s");

      GUILayout.EndHorizontal();
    }


    //BoltGUI.Table<BoltConnection>(BoltCore.connections, new BoltGUI.TableStyle { labelStyle = boldLabel, tableStyle = sceneStyle },
    //  BoltGUI.Column<BoltConnection>("Address", c => GUILayout.Label(c.udpConnection.RemoteEndPoint.ToString(), GUI.skin.label)),
    //  BoltGUI.Column<BoltConnection>("Ping (ms)", c => Mathf.FloorToInt(c.udpConnection.NetworkPing * 1000f)),
    //  BoltGUI.Column<BoltConnection>("OUT (kb/s)", c => (c.bitsPerSecondOut >> 3) / 1000f),
    //  BoltGUI.Column<BoltConnection>("IN (kb/s)", c => (c.bitsPerSecondIn >> 3) / 1000f),
    //  BoltGUI.Column<BoltConnection>("", c => ConnectionButtons(c))
    //);
  }

  void ConnectionButtons (BoltConnection c) {
    GUILayout.BeginHorizontal();

    if (GUILayout.Button("Disconnect", GUILayout.ExpandWidth(false))) {
      c.Disconnect();
    }

    GUILayout.EndHorizontal();
  }
}

using System;
using System.Linq;
using System.Collections;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class BoltConnectionsWindow : BoltWindow {
  Vector2 scroll;
  BoltConnection ConnectionDetails;

  void OnEnable() {
    title = name = "Bolt Remotes";
    scroll = Vector2.zero;
  }

  void Header(string icon, string text) {
    GUI.color = BoltEditorSkin.Selected.Variation.TintColor;
    GUILayout.BeginHorizontal(BoltEditorGUI.BoxStyle(BoltEditorSkin.Selected.Background), GUILayout.Height(22));
    GUI.color = Color.white;

    BoltEditorGUI.Button(icon);
    GUILayout.Label(text);

    GUILayout.EndHorizontal();
  }

  void Update() {
    if (Application.isPlaying) {
      Repaints = Mathf.Max(Repaints, 1);
    }

    base.Update();
  }

  void OnGUI() {
    base.OnGUI();

    scroll = GUILayout.BeginScrollView(scroll);

    Header("mc_connection", "Connections");
    Connections();

    if (ConnectionDetails != null) {
      Header("mc_wifi", "Packet details for " + ConnectionDetails.remoteEndPoint.ToString());
      Stats();
    }


    GUILayout.EndScrollView();
  }

  Action MakeHeader(string icon, string text) {
    return () => {
      GUILayout.BeginHorizontal();

      BoltEditorGUI.Button(icon);
      GUILayout.Label(text, EditorStyles.miniLabel);

      GUILayout.EndHorizontal();
    };
  }

  bool IsSelected(BoltConnection c) {
    return ReferenceEquals(ConnectionDetails, c);
  }

  void StatsButton(BoltConnection c, object text) {
    GUIStyle s = new GUIStyle("Label");
    s.padding = new RectOffset();
    s.margin = new RectOffset(0, 0, 0, 2);
    s.normal.textColor = IsSelected(c) ? BoltEditorSkin.Selected.IconColor : s.normal.textColor;

    if (GUILayout.Button(text.ToString(), s)) {
      ConnectionDetails = c;
    }
  }

  void StatsLabel(BoltConnection c, object text) {
    GUIStyle s = new GUIStyle("Label");
    s.padding = new RectOffset();
    s.margin = new RectOffset(0, 0, 0, 2);

    if (GUILayout.Button(text.ToString(), s)) {
    }
  }

  void Connections() {
    GUILayout.Space(2);

    GUIStyle s = new GUIStyle(GUIStyle.none);
    s.padding = new RectOffset(5, 5, 2, 2);
    GUILayout.BeginHorizontal(s);

    EachConnection(MakeHeader("mc_wifi", "Address"), c => StatsButton(c, c.udpConnection.RemoteEndPoint));
    EachConnection(MakeHeader("mc_latency", "Ping (Network)"), c => StatsButton(c, Mathf.FloorToInt(c.udpConnection.NetworkPing * 1000f) + " ms"));
    EachConnection(MakeHeader("mc_latency", "Ping (Aliased)"), c => StatsButton(c, Mathf.FloorToInt(c.udpConnection.AliasedPing * 1000f) + " ms"));
    EachConnection(MakeHeader("mc_download", "Download"), c => StatsButton(c, Math.Round((c.bitsPerSecondIn / 8f) / 1000f, 2) + " kb/s"));
    EachConnection(MakeHeader("mc_upload", "Upload"), c => StatsButton(c, Math.Round((c.bitsPerSecondOut / 8f) / 1000f, 2) + " kb/s"));

    GUILayout.EndHorizontal();
    GUILayout.Space(4);
  }

  void EachConnection(Action header, Action<BoltConnection> call) {
    EachConnection(BoltCore.connections, header, call);
  }

  void EachConnection(IEnumerable connections, Action header, Action<BoltConnection> call) {
    GUILayout.BeginVertical();
    header();

    foreach (BoltConnection c in connections) {
      call(c);
    }

    GUILayout.EndVertical();
  }

  void SelectedConnection(Action header, Action<BoltConnection> call) {
    EachConnection(new[] { ConnectionDetails }, header, call);
  }

  void Stats() {
    double statesOut = Math.Round(ConnectionDetails._packetStatsOut.Select(x => x.StateBits).Sum() / 8f / ConnectionDetails._packetStatsOut.count, 2);
    double eventsOut = Math.Round(ConnectionDetails._packetStatsOut.Select(x => x.EventBits).Sum() / 8f / ConnectionDetails._packetStatsOut.count, 2);
    double commandsOut = Math.Round(ConnectionDetails._packetStatsOut.Select(x => x.CommandBits).Sum() / 8f / ConnectionDetails._packetStatsOut.count, 2);

    double statesIn = Math.Round(ConnectionDetails._packetStatsIn.Select(x => x.StateBits).Sum() / 8f / ConnectionDetails._packetStatsIn.count, 2);
    double eventsIn = Math.Round(ConnectionDetails._packetStatsIn.Select(x => x.EventBits).Sum() / 8f / ConnectionDetails._packetStatsIn.count, 2);
    double commandsIn = Math.Round(ConnectionDetails._packetStatsIn.Select(x => x.CommandBits).Sum() / 8f / ConnectionDetails._packetStatsIn.count, 2);

    GUIStyle s = new GUIStyle(GUIStyle.none);
    s.padding = new RectOffset(5, 5, 2, 2);
    GUILayout.BeginHorizontal(s);

    SelectedConnection(MakeHeader("mc_state", "In: States"), c => StatsLabel(c, statesIn + " bytes"));
    SelectedConnection(MakeHeader("mc_state", "Out: States"), c => StatsLabel(c, statesOut + " bytes"));

    SelectedConnection(MakeHeader("mc_event", "In: Events"), c => StatsLabel(c, eventsIn + " bytes"));
    SelectedConnection(MakeHeader("mc_event", "Out: Events"), c => StatsLabel(c, eventsOut + " bytes"));

    SelectedConnection(MakeHeader("mc_command", "In: Commands"), c => StatsLabel(c, commandsIn + " bytes"));
    SelectedConnection(MakeHeader("mc_command", "Out: Commands"), c => StatsLabel(c, commandsOut + " bytes"));

    GUILayout.EndHorizontal();
    GUILayout.Space(4);
  }
}

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

  new void Update() {
    if (Application.isPlaying) {
      Repaints = Mathf.Max(Repaints, 1);
    }

    base.Update();
  }

  new void OnGUI() {
    base.OnGUI();

    if (!Application.isPlaying) {
      ConnectionDetails = null;
    }

    scroll = GUILayout.BeginScrollView(scroll);

    if (BoltNetwork.connections.Count() == 1) {
      ConnectionDetails = BoltNetwork.connections.First();
    }

    Header("mc_connection", "Connections");
    Connections();

    Header("mc_nat", "NAT Devices");
    NatDevices();

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

      GUIStyle s = new GUIStyle(EditorStyles.miniLabel);
      s.padding = new RectOffset();
      s.margin = new RectOffset(5, 0, 3, 0);

      GUILayout.Label(text, s);

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

  void StatsLabel(object text) {
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
    EachConnection(MakeHeader("mc_state2", "Ping (Network)"), c => StatsButton(c, Mathf.FloorToInt(c.udpConnection.NetworkPing * 1000f) + " ms"));
    EachConnection(MakeHeader("mc_state2", "Ping (Aliased)"), c => StatsButton(c, Mathf.FloorToInt(c.udpConnection.AliasedPing * 1000f) + " ms"));
    EachConnection(MakeHeader("mc_download", "Download"), c => StatsButton(c, Math.Round((c.bitsPerSecondIn / 8f) / 1000f, 2) + " kb/s"));
    EachConnection(MakeHeader("mc_upload", "Upload"), c => StatsButton(c, Math.Round((c.bitsPerSecondOut / 8f) / 1000f, 2) + " kb/s"));

    GUILayout.EndHorizontal();
    GUILayout.Space(4);
  }

  void NatDevices() {
    GUILayout.Space(2);
    GUIStyle s = new GUIStyle(GUIStyle.none);
    s.padding = new RectOffset(5, 5, 2, 2);
    GUILayout.BeginHorizontal(s);

    EachNatDevice(MakeHeader("mc_devicetype", "Device Type"), n => StatsLabel(n.DeviceType));
    EachNatDevice(MakeHeader("mc_ipaddress", "Public Address"), n => StatsLabel(n.PublicAddress));
    EachNatDevice(MakeHeader("mc_ipaddress", "Local Address"), n => StatsLabel(n.LocalAddress));

    GUILayout.EndHorizontal();
    GUILayout.Space(4);
  }

  void EachConnection(Action header, Action<BoltConnection> call) {
    EachConnection(BoltCore.connections, header, call);
  }

  void EachNatDevice(Action header, Action<Bolt.INatDevice> call) {
    if (BoltNetworkInternal.NatCommunicator != null) {
      EachConnection(BoltNetworkInternal.NatCommunicator.NatDevices, header, call);
    }
    else {
      EachConnection(new Bolt.INatDevice[0], header, call);
    }
  }

  void EachConnection<T>(IEnumerable connections, Action header, Action<T> call) {
    GUILayout.BeginVertical();
    header();

    foreach (T c in connections) {
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

    SelectedConnection(MakeHeader("mc_state", "In: States"), c => StatsLabel(statesIn + " bytes"));
    SelectedConnection(MakeHeader("mc_state", "Out: States"), c => StatsLabel(statesOut + " bytes"));

    SelectedConnection(MakeHeader("mc_event", "In: Events"), c => StatsLabel(eventsIn + " bytes"));
    SelectedConnection(MakeHeader("mc_event", "Out: Events"), c => StatsLabel(eventsOut + " bytes"));

    SelectedConnection(MakeHeader("mc_command", "In: Commands"), c => StatsLabel(commandsIn + " bytes"));
    SelectedConnection(MakeHeader("mc_command", "Out: Commands"), c => StatsLabel(commandsOut + " bytes"));

    GUILayout.EndHorizontal();
    GUILayout.Space(4);
  }
}

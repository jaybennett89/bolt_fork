using System;
using System.Linq;
using System.Collections;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class BoltConnectionsWindow : BoltWindow {
  Vector2 scroll;
  BoltConnection ConnectionDetails;
  Bolt.INatDevice NatDeviceDetails;

  void OnEnable() {
    title = name = "Bolt Remotes";
    scroll = Vector2.zero;
  }

  void Header(string icon, string text) {
    GUILayout.BeginHorizontal(BoltEditorGUI.HeaderBackgorund, GUILayout.Height(BoltEditorGUI.HEADER_HEIGHT));

    BoltEditorGUI.IconButton(icon);
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

    GUILayout.BeginArea(new Rect(BoltEditorGUI.GLOBAL_INSET, BoltEditorGUI.GLOBAL_INSET, position.width - (BoltEditorGUI.GLOBAL_INSET * 2), position.height - (BoltEditorGUI.GLOBAL_INSET * 2)));

    if (!Application.isPlaying) {
      NatDeviceDetails = null;
      ConnectionDetails = null;
    }

    scroll = GUILayout.BeginScrollView(scroll);

    if (BoltNetwork.connections.Count() == 1) {
      ConnectionDetails = BoltNetwork.connections.First();
    }

    Header("mc_connection", "Connections");
    Connections();

    if (ConnectionDetails != null) {
      Header("mc_connection", "Packet details for " + ConnectionDetails.remoteEndPoint);
      ConnectionDetailsView();
    }

    Header("mc_nat", "NAT Devices");
    NatDevices();

    if (NatDeviceDetails != null) {
      Header("mc_nat", "Port details for " + NatDeviceDetails);
      NatDeviceDetailsView();
    }

    Header("mc_server", "LAN Servers");
    LanServers();

    GUILayout.EndArea();
    GUILayout.EndScrollView();
  }

  void LanServers() {
    GUIStyle s = new GUIStyle(GUIStyle.none);
    s.padding = new RectOffset(5, 5, 2, 2);
    GUILayout.BeginHorizontal(s);

    var sessions = BoltNetwork.isRunning ? BoltNetwork.GetSessions() : new UdpKit.UdpSession[0];

    Each<UdpKit.UdpSession>(sessions, MakeHeader("mc_name", "Name"), c => StatsLabel(c.ServerName));
    Each<UdpKit.UdpSession>(sessions, MakeHeader("mc_ipaddress", "End Point"), c => StatsLabel(c.EndPoint));
    Each<UdpKit.UdpSession>(sessions, MakeHeader("mc_bubble", "User Data"), c => StatsLabel(c.UserData ?? ""));

    GUILayout.EndHorizontal();
    GUILayout.Space(4);
  }

  void NatDeviceDetailsView() {
    GUIStyle s = new GUIStyle(GUIStyle.none);
    s.padding = new RectOffset(5, 5, 2, 2);
    GUILayout.BeginHorizontal(s);

    Each<Bolt.IPortMapping>(NatDeviceDetails.Ports, MakeHeader("mc_port", "Status"), c => StatsLabel(c.Status));
    Each<Bolt.IPortMapping>(NatDeviceDetails.Ports, MakeHeader("mc_port_in", "External Port"), c => StatsLabel(c.External));
    Each<Bolt.IPortMapping>(NatDeviceDetails.Ports, MakeHeader("mc_port_out", "Internal Port"), c => StatsLabel(c.Internal));

    GUILayout.EndHorizontal();
    GUILayout.Space(4);
  }

  Action MakeHeader(string icon, string text) {
    return () => {
      GUILayout.BeginHorizontal();

      BoltEditorGUI.IconButton(icon);

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

  bool IsSelected(Bolt.INatDevice d) {
    return ReferenceEquals(NatDeviceDetails, d);
  }

  void StatsButton(BoltConnection c, object text) {
    StatsButton(() => ConnectionDetails = c, IsSelected(c), text);
  }

  void StatsButton(Bolt.INatDevice d, object text) {
    StatsButton(() => NatDeviceDetails = d, IsSelected(d), text);
  }

  void StatsButton(Action clicked, bool selected, object text) {
    GUIStyle s = new GUIStyle("Label");
    s.padding = new RectOffset();
    s.margin = new RectOffset(0, 0, 0, 2);
    s.normal.textColor = selected ? BoltEditorGUI.HighlightColor : s.normal.textColor;

    if (GUILayout.Button(text.ToString(), s)) {
      clicked();
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

    EachConnection(MakeHeader("mc_ipaddress", "Address"), c => StatsButton(c, c.udpConnection.RemoteEndPoint));
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

    EachNatDevice(MakeHeader("mc_ipaddress", "Local Address"), n => StatsButton(n, n.LocalAddress));
    EachNatDevice(MakeHeader("mc_ipaddress", "Public Address"), n => StatsButton(n, n.PublicAddress));
    EachNatDevice(MakeHeader("mc_devicetype", "Device Type"), n => StatsButton(n, n.DeviceType));

    GUILayout.EndHorizontal();
    GUILayout.Space(4);
  }

  void EachConnection(Action header, Action<BoltConnection> call) {
    Each(BoltCore.connections, header, call);
  }

  void EachNatDevice(Action header, Action<Bolt.INatDevice> call) {
    if (BoltNetworkInternal.NatCommunicator != null) {
      Each(BoltNetworkInternal.NatCommunicator.NatDevices, header, call);
    }
    else {
      Each(new Bolt.INatDevice[0], header, call);
    }
  }

  void Each<T>(IEnumerable items, Action header, Action<T> call) {
    GUILayout.BeginVertical();
    header();

    foreach (T c in items) {
      call(c);
    }

    GUILayout.EndVertical();
  }

  void SelectedConnection(Action header, Action<BoltConnection> call) {
    Each(new[] { ConnectionDetails }, header, call);
  }

  void ConnectionDetailsView() {
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

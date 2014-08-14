//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(BoltConfigAsset))]
//class BoltConfigAssetEditor : Editor {
//  void BeginVertical () {

//    GUI.color = BoltAssetEditorGUI.lightBlue;
//    GUIStyle style =  BoltAssetEditorGUI.BoxStyle(1);
//    GUI.color = Color.white;
//    EditorGUILayout.BeginVertical(style);

//  }
//  public override void OnInspectorGUI () {

//    BoltConfigAsset asset = (BoltConfigAsset) target;

//    // GLOBAL

//    GUILayout.Label("Global Settings", EditorStyles.boldLabel);
//    BeginVertical();
//    BoltAssetEditorGUI.Label("Frames Per Second", () => {
//      asset.config.framesPerSecond = BoltAssetEditorGUI.IntFieldOverlay(asset.config.framesPerSecond, "FixedUpdate Calls");
//    });

//    EditorGUILayout.EndVertical();

//    // SERVER


//    GUILayout.Label("Server Settings", EditorStyles.boldLabel);

//    BeginVertical();

//    BoltAssetEditorGUI.Label("Packet Send Rate", () => {
//      asset.config.serverSendRate = BoltAssetEditorGUI.IntFieldOverlay(asset.config.serverSendRate, "Frames");
//    });

//    EditDejitterDelay("Dejitter Delay",
//      ref asset.config.serverDejitterDelay,
//      ref asset.config.serverDejitterDelayMin,
//      ref asset.config.serverDejitterDelayMax
//    );


//    EditorGUILayout.EndVertical();

//    // CLIENT

//    GUILayout.Label("Client Settings", EditorStyles.boldLabel);

//    BeginVertical();
    
//    BoltAssetEditorGUI.Label("Packet Send Rate", () => {
//      asset.config.clientSendRate = BoltAssetEditorGUI.IntFieldOverlay(asset.config.clientSendRate, "Frames");
//    });

//    EditDejitterDelay("Dejitter Delay",
//      ref asset.config.clientDejitterDelay,
//      ref asset.config.clientDejitterDelayMin,
//      ref asset.config.clientDejitterDelayMax
//    );

//    EditorGUILayout.EndVertical();

//    // COMMAND

//    GUILayout.Label("Command Settings", EditorStyles.boldLabel);

//    BeginVertical();
//    BoltAssetEditorGUI.Label("Dejitter Delay (Max)", () => {
//      asset.config.commandDejitterDelay = BoltAssetEditorGUI.IntFieldOverlay(asset.config.commandDejitterDelay, "Frames");
//    });

//    BoltAssetEditorGUI.Label("Discard Delay", () => {
//      asset.config.commandPingMultiplier = BoltAssetEditorGUI.FloatFieldOverlay(asset.config.commandPingMultiplier, "x Ping");

//      GUILayout.Label("+");

//      asset.config.commandDelayAllowed = BoltAssetEditorGUI.IntFieldOverlay(asset.config.commandDelayAllowed, "Frames");
//    });

//    BoltAssetEditorGUI.Label("Outgoing Queue (Max)", () => {
//      asset.config.commandQueueSize = BoltAssetEditorGUI.IntFieldOverlay(asset.config.commandQueueSize, "Unsent Commands");
//    });

//    BoltAssetEditorGUI.Label("Send Redundancy", () => {
//      asset.config.commandRedundancy = BoltAssetEditorGUI.IntFieldOverlay(asset.config.commandRedundancy, "Commands Per Packet");
//    });

//    EditorGUILayout.EndVertical();
//    // NETWORK SIMULATION

//    GUILayout.Label("Network Simulation", EditorStyles.boldLabel);

//    BeginVertical();
//    EditorGUI.BeginDisabledGroup(BoltCore.isDebugMode == false);

//    BoltAssetEditorGUI.Label("Packet Loss", () => {
//      int loss;

//      loss = Mathf.Clamp(Mathf.RoundToInt(asset.config.simulatedLoss * 100), 0, 100);
//      loss = BoltAssetEditorGUI.IntFieldOverlay(loss, "Percent");

//      asset.config.simulatedLoss = Mathf.Clamp01(loss / 100f);
//    });

//    BoltAssetEditorGUI.Label("Ping", () => {
//      asset.config.simulatedPingMean = BoltAssetEditorGUI.IntFieldOverlay(asset.config.simulatedPingMean, "Mean");
//      asset.config.simulatedPingJitter = BoltAssetEditorGUI.IntFieldOverlay(asset.config.simulatedPingJitter, "Jitter");
//    });

//    BoltAssetEditorGUI.Label("Noise Function", () => {
//      asset.config.simulatedRandomFunction = (BoltRandomFunction) EditorGUILayout.EnumPopup(asset.config.simulatedRandomFunction);
//    });

//    EditorGUI.EndDisabledGroup();
//    EditorGUILayout.EndVertical();

//    if (GUI.changed) {
//      EditorUtility.SetDirty(target);
//    }
//  }

//  void EditDejitterDelay (string label, ref int delay, ref int min, ref int max) {
//    const int MAX_DIFF = 24;

//    GUILayout.Label(label + ": " + delay + " (Frames)");

//    delay = Mathf.Clamp(Mathf.RoundToInt(GUILayout.HorizontalSlider(delay, -MAX_DIFF, +MAX_DIFF)), 0, +MAX_DIFF);

//    float minf = min;
//    float maxf = max;

//    EditorGUILayout.MinMaxSlider(ref minf, ref maxf, -MAX_DIFF, +MAX_DIFF);
//    min = Mathf.Clamp(Mathf.RoundToInt(minf), -MAX_DIFF, delay);
//    max = Mathf.Clamp(Mathf.RoundToInt(maxf), delay, +MAX_DIFF);

//    EditorGUILayout.BeginHorizontal();
//    GUILayout.Label(min + " (Min)", GUILayout.ExpandWidth(false));
//    GUILayout.FlexibleSpace();
//    GUILayout.Label("(Max) " + max, GUILayout.ExpandWidth(false));
//    EditorGUILayout.EndHorizontal();
//  }
//}

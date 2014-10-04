using UnityEngine;
using System.Collections;
using UnityEditor;
using Bolt.Compiler;
using System.Linq;
using System;
using System.Collections.Generic;

public class BoltEditorWindow : BoltWindow {
  [MenuItem("Window/Bolt Editor")]
  public static void Open() {
    BoltEditorWindow w;

    w = EditorWindow.GetWindow<BoltEditorWindow>();
    w.title = "Bolt Editor";
    w.name = "Bolt Editor";
    w.minSize = new Vector2(300, 400);
    w.Show();
  }

  Vector2 scroll;

  new void OnGUI() {
    base.OnGUI();

    GUILayout.BeginArea(new Rect(0, 5, position.width, position.height - 25));

    scroll = GUILayout.BeginScrollView(scroll, false, false);

    if (HasProject) {
      Editor();
    }

    GUILayout.EndScrollView();
    GUILayout.EndArea();

    if (GUI.changed) {
      Save();
    }

    Rect r = new Rect(0, position.height - 20, position.width, 20);
    GUILayout.BeginArea(r);
    Footer(r);
    GUILayout.EndArea();
  }

  void Footer(Rect r) {
    GUI.color = EditorGUIUtility.isProSkin ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.45f, 0.45f, 0.45f);
    GUILayout.BeginHorizontal(BoltEditorGUI.WhiteTextureBackgroundStyle);
    GUI.color = Color.white;

    GUILayout.Label(BoltEditorGUI.Tooltip ?? "");

    GUILayout.EndHorizontal();
  }

  void Editor() {

    if ((Selected is AssetDefinition) && (ReferenceEquals(Selected, SelectedAsset) == false)) {
      SelectedAsset = (AssetDefinition)Selected;
    }

    if (SelectedAsset != null) {
      if (SelectedAsset is StateDefinition) {
        EditState((StateDefinition)SelectedAsset);
      }

      if (SelectedAsset is StructDefinition) {
        EditStruct((StructDefinition)SelectedAsset);
      }

      if (SelectedAsset is EventDefinition) {
        EditEvent((EventDefinition)SelectedAsset);
      }

      if (SelectedAsset is CommandDefinition) {
        EditCommand((CommandDefinition)SelectedAsset);
      }
    }
  }


  void EditState(StateDefinition def) {

    EditHeader(def, BoltEditorGUI.StateHeaderStyle, BoltEditorGUI.StateHeaderColor, () => {
      // separator
      GUILayout.Label(":", BoltEditorGUI.InheritanceSeparatorStyle, GUILayout.ExpandWidth(false));

      // inheritnace
      def.ParentGuid = BoltEditorGUI.AssetPopup(Project.States.Cast<AssetDefinition>(), def.ParentGuid, new Guid[] { });

      // 
      if (BoltEditorGUI.LabelButton("abstract", def.IsAbstract, 0.25f, GUILayout.Width(45))) {
        def.IsAbstract = !def.IsAbstract;
      }
    }, new string[] { "compile" }, () => {

      def.PacketMaxBits = Mathf.Clamp(BoltEditorGUI.IntFieldOverlay(def.PacketMaxBits, "Bits/Packet"), 128, 4096);
      def.PacketMaxProperties = Mathf.Clamp(BoltEditorGUI.IntFieldOverlay(def.PacketMaxProperties, "Properties/Packet"), 1, 255);

    });

    // add button
    // GUILayout.Label("Defined Properties", BoltEditorGUI.MiniLabelButtonStyle);

    BoltEditorGUI.AddButton("Defined Properties", def.Properties, () => new PropertyStateSettings());

    // list properties
    EditPropertyList(def, def.Properties, StateAndStructToolbar);

    Guid guid = def.ParentGuid;

    while (guid != Guid.Empty) {
      var parent = Project.FindState(guid);
      GUILayout.Label(string.Format("Inherited from {0}", parent.Name), BoltEditorGUI.MiniLabelButtonStyle);

      EditorGUI.BeginDisabledGroup(true);
      EditPropertyList(parent, parent.Properties, StateAndStructToolbar);
      EditorGUI.EndDisabledGroup();

      guid = parent.ParentGuid;
    }
  }

  void EditStruct(StructDefinition def) {
    EditHeader(def, BoltEditorGUI.StructHeaderStyle, BoltEditorGUI.StructHeaderColor, () => {

    });

    // add button
    BoltEditorGUI.AddButton("Defined Properties", def.Properties, () => new PropertyStateSettings());

    // list properties
    EditPropertyList(def, def.Properties, StateAndStructToolbar);

  }

  void EditEvent(EventDefinition def) {
    EditHeader(def, BoltEditorGUI.EventHeaderStyle, BoltEditorGUI.EventHeaderColor, () => {
      def.Priority = BoltEditorGUI.EditPriority(def.Priority, !def.Global);

      if (BoltEditorGUI.LabelButton(def.Global ? "global (reliable)" : "entity (unreliable)", true, 0.5f, GUILayout.Width(92))) {
        def.Global = !def.Global;
      }
    }, new string[] { "upload" }, () => {
      if (def.Global) {
        def.GlobalSenders = (GlobalEventSenders)EditorGUILayout.EnumPopup(def.GlobalSenders);
      }
      else {
        def.EntitySenders = (EntityEventSenders)EditorGUILayout.EnumPopup(def.EntitySenders);
      }

      BoltEditorGUI.SetTooltip("Senders, who can send this event?");

      if (def.Global) {
        def.GlobalTargets = (GlobalEventTargets)EditorGUILayout.EnumMaskField(def.GlobalTargets);
      }
      else {
        def.EntityTargets = (EntityEventTargets)EditorGUILayout.EnumPopup(def.EntityTargets);
      }

      BoltEditorGUI.SetTooltip("Receivers, who will receive this event?");
      BoltEditorGUI.Icon("download", new RectOffset(0, +3, +2, 0));
    });

    // add button
    BoltEditorGUI.AddButton("Defined Properties", def.Properties, () => new PropertyEventSettings());

    // list properties
    EditPropertyList(def, def.Properties, null);
  }

  void EditCommand(CommandDefinition def) {
    EditHeader(def, BoltEditorGUI.CommandHeaderStyle, BoltEditorGUI.CommandHeaderColor, () => {

    });

    // add button
    BoltEditorGUI.AddButton("Input", def.Input, () => new PropertyCommandSettings());

    // list properties
    EditPropertyList(def, def.Input, null);

    // add button
    BoltEditorGUI.AddButton("Result", def.Result, () => new PropertyCommandSettings());

    // list properties
    EditPropertyList(def, def.Result, null);
  }

  void EditHeader(AssetDefinition def, GUIStyle style, Color color, Action action, params Action[] rows) {
    EditHeader(def, style, color, action, new string[0], rows);
  }

  void EditHeader(AssetDefinition def, GUIStyle style, Color color, Action action, string[] icons, params Action[] rows) {
    GUI.color = color;
    GUILayout.BeginVertical(style);
    GUI.color = Color.white;
    GUILayout.BeginHorizontal();

    if (def is EventDefinition) {
      BoltEditorGUI.Icon("boltico_event2", new RectOffset(3, 0, 2, 0));
    }

    if (def is StructDefinition) {
      BoltEditorGUI.Icon("boltico_object", new RectOffset(3, 0, 2, 0));
    }

    if (def is StateDefinition) {
      BoltEditorGUI.Icon("boltico_replistate2", new RectOffset(3, 0, 2, 0));
    }

    if (def is CommandDefinition) {
      BoltEditorGUI.Icon("boltico_playcom2", new RectOffset(3, 0, 2, 0));
    }

    // edit asset name
    def.Name = EditorGUILayout.TextField(def.Name);

    // remaining header
    action();

    GUILayout.EndHorizontal();


    for (int i = 0; i < rows.Length; ++i) {
      GUILayout.BeginHorizontal();
      if (icons != null && i < icons.Length) {
        BoltEditorGUI.Icon(icons[i], new RectOffset(3, 0, 2, 0));
      }
      else {
        GUILayout.Space(23);
      }

      rows[i]();
      GUILayout.EndHorizontal();
    }

    GUILayout.Label("Comment", BoltEditorGUI.SmallWhiteText);

    GUILayout.BeginHorizontal();
    def.Comment = EditorGUILayout.TextArea(def.Comment);
    GUILayout.EndHorizontal();

    GUILayout.EndVertical();
  }

  void EditPropertyList(AssetDefinition def, List<PropertyDefinition> list, Action<AssetDefinition, PropertyDefinition> toolbar) {
    for (int i = 0; i < list.Count; ++i) {
      EditProperty(def, list[i], toolbar);
    }

    // move nudged property
    for (int i = 0; i < list.Count; ++i) {
      switch (list[i].Nudge) {
        case -1:
          if (i > 0) {
            var a = list[i];
            var b = list[i - 1];

            list[i] = b;
            list[i - 1] = a;
          }
          break;

        case +1:
          if (i + 1 < list.Count) {
            var a = list[i];
            var b = list[i + 1];

            list[i] = b;
            list[i + 1] = a;
          }
          break;
      }
    }

    // remove deleted property
    for (int i = 0; i < list.Count; ++i) {
      if (list[i].Deleted) {
        // remove 
        list.RemoveAt(i);

        // rewind index
        i -= 1;
      }
    }
  }

  void EditProperty(AssetDefinition def, PropertyDefinition p, Action<AssetDefinition, PropertyDefinition> toolbar) {
    EditorGUILayout.BeginVertical(BoltEditorGUI.ParameterBackgroundStyle);
    EditorGUILayout.BeginHorizontal();

    if ((Event.current.modifiers & EventModifiers.Control) == EventModifiers.Control) {
      if (BoltEditorGUI.IconButton("boltico_x".ToContent())) {
        p.Deleted = true;
      }
    }
    else {
      if (BoltEditorGUI.OnOffButton("boltico_arrow_down".ToContent(), "boltico_arrow_right".ToContent(), p.Expanded && (p.PropertyType.HasSettings || p.PropertyType.MecanimApplicable))) {
        p.Expanded = !p.Expanded;
      }
    }

    // edit priority
    p.Priority = BoltEditorGUI.EditPriority(p.Priority, p.PropertyType.HasPriority);

    // edit name
    p.Name = EditorGUILayout.TextField(p.Name, GUILayout.Width(164));
    BoltEditorGUI.SetTooltip("Property Name. Has to be a valid C# property name.");

    // edit property type
    BoltEditorGUI.PropertyTypePopup(def, p);
    BoltEditorGUI.SetTooltip("Property Type.");

    if (toolbar != null) {
      toolbar(def, p);
    }
    else {
      GUILayout.Space(20);
    }

    EditorGUILayout.EndHorizontal();

    if (p.Expanded) {

      SettingsSection("Comment", () => {
        p.Comment = EditorGUILayout.TextField(p.Comment);
      });

      if (p.PropertyType.MecanimApplicable && (def is StateDefinition)) {
        SettingsSection("Mecanim", () => {
          BoltEditorGUI.WithLabel("Auto Apply", () => {
            p.StateAssetSettings.Mecanim = EditorGUILayout.Toggle(p.StateAssetSettings.Mecanim);
          });

          if (p.PropertyType is PropertyTypeFloat) {
            BoltEditorGUI.WithLabel("Damping Time", () => {
              p.StateAssetSettings.MecanimDamping = EditorGUILayout.FloatField(p.StateAssetSettings.MecanimDamping);
            });
          }
        });
      }

      if (def is CommandDefinition) {
        if (p.PropertyType.CanSmoothCorrections && ((CommandDefinition)def).Result.Contains(p)) {
          SettingsSection("Corrections", () => {
            BoltEditorGUI.WithLabel("Smooth Corrections", () => {
              p.CommandAssetSettings.SmoothCorrection = EditorGUILayout.Toggle(p.CommandAssetSettings.SmoothCorrection);
            });
          });
        }
      }

      if (p.PropertyType.HasSettings) {
        SettingsSection("Compression", () => {
          if (IsStateOrStruct(def)) {
            EditStateAssetSettings(p);
          }

          PropertyEditorRegistry.GetEditor(p.PropertyType.GetType()).Edit(def, p);
        });
      }
    }

    EditorGUILayout.EndVertical();
  }

  void SettingsSection(string label, Action gui) {
    EditorGUILayout.BeginHorizontal();
    GUILayout.Space(20);

    EditorGUILayout.BeginVertical();
    EditorGUILayout.LabelField(label, BoltEditorGUI.SmallWhiteText);

    gui();

    EditorGUILayout.EndVertical();

    GUILayout.Space(20);
    EditorGUILayout.EndHorizontal();
  }

  void EditStateAssetSettings(PropertyDefinition p) {
    if (p.PropertyType.InterpolateAllowed) {
      BoltEditorGUI.WithLabel("Smoothing Algorithm", () => {
        p.StateAssetSettings.EstimationAlgorithm = (StateEstimationAlgorithm)EditorGUILayout.EnumPopup(p.StateAssetSettings.EstimationAlgorithm);

        if (p.StateAssetSettings.EstimationAlgorithm == StateEstimationAlgorithm.DeadReckoning) {
          p.StateAssetSettings.DeadReckoningErrorTolerance = BoltEditorGUI.FloatFieldOverlay(p.StateAssetSettings.DeadReckoningErrorTolerance, "Error Tolerance");
        }
      });
    }
  }

  bool IsStateOrStruct(AssetDefinition def) {
    return (def is StateDefinition) || (def is StructDefinition);
  }

  void StateAndStructToolbar(AssetDefinition def, PropertyDefinition p) {
    EditFilters(p);

    if (BoltEditorGUI.IconButton("boltico_playcom2".ToContent("This property should be replicated to the controller"), p.Controller)) {
      p.Controller = !p.Controller;
    }
  }

  GenericMenu.MenuFunction FilterSetter(PropertyDefinition p, FilterDefinition f) {
    return () => { p.Filters ^= f.Bit; };
  }

  void EditFilters(PropertyDefinition p) {
    if (Project.UseFilters) {
      GUIStyle s = new GUIStyle(EditorStyles.miniButton);
      s.alignment = TextAnchor.MiddleLeft;

      Rect menuRect;

      menuRect = GUILayoutUtility.GetLastRect();
      menuRect.x += 85;

      if (GUILayout.Button("", s, GUILayout.MinWidth(200))) {
        GenericMenu menu = new GenericMenu();

        foreach (FilterDefinition f in Project.EnabledFilters) {
          menu.AddItem(new GUIContent(f.Name), f.IsOn(p.Filters), FilterSetter(p, f));
        }

        menu.DropDown(menuRect);
        EditorGUIUtility.ExitGUI();
      }

      // rect of the button
      var r = GUILayoutUtility.GetLastRect();
      var labelRect = r;

      labelRect.xMin += 3;
      labelRect.yMin -= 1;
      labelRect.xMax -= 17;

      //GUILayout.BeginArea(r);

      foreach (FilterDefinition f in Project.EnabledFilters) {
        if (f.IsOn(p.Filters)) {
          var label = BoltEditorGUI.MiniLabelWithColor(f.Color.ToUnityColor());
          var sizex = Mathf.Min(label.CalcSize(new GUIContent(f.Name)).x, labelRect.width);

          GUI.Label(labelRect, f.Name, label);

          labelRect.xMin += sizex;
          labelRect.xMin = Mathf.Min(labelRect.xMin, labelRect.xMax);
        }
      }

      //GUILayout.EndArea();

      GUI.DrawTexture(new Rect(r.xMax - 18, r.yMin, 16, 16), BoltEditorGUI.LoadIcon("boltico_arrow_down"));
    }
  }
}

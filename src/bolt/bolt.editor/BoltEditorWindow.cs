using Bolt.Compiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BoltEditorWindow : BoltWindow {
  [MenuItem("Window/Bolt Engine/Editor", priority = -99)]
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

    if (HasProject) {
      scroll = GUILayout.BeginScrollView(scroll, false, false);

      Editor();

      GUILayout.EndScrollView();
    } 

    if (GUI.changed) {
      Save();
    }

    ClearAllFocus();
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
    Action gui = () => {
      // inheritnace
      def.ParentGuid = BoltEditorGUI.AssetPopup(Project.States.Cast<AssetDefinition>(), def.ParentGuid, Project.GetInheritanceTree(def));

      // 
      if (BoltEditorGUI.LabelButton("abstract", def.IsAbstract, 0.25f, GUILayout.Width(45))) {
        def.IsAbstract = !def.IsAbstract;
      }
    };

    if (def.IsAbstract) {
      EditHeader(def, gui);
    }
    else {
      EditHeader(def, gui, () => {
        GUILayout.Label("Bandwidth", BoltEditorGUI.AccentText);

        GUILayout.BeginHorizontal();
        def.PacketMaxBits = Mathf.Clamp(BoltEditorGUI.IntFieldOverlay(def.PacketMaxBits, "Bits/Packet"), 128, 4096);
        def.PacketMaxProperties = Mathf.Clamp(BoltEditorGUI.IntFieldOverlay(def.PacketMaxProperties, "Properties/Packet"), 1, 255);
        GUILayout.EndHorizontal();
      });
    }

    BoltEditorGUI.AddButton("Properties", def.Properties, () => new PropertyStateSettings());
    EditPropertyList(def, def.Properties);

    if (def.Properties.Count > 0)
      BoltEditorGUI.AddButton("", def.Properties, () => new PropertyStateSettings());

    Guid guid = def.ParentGuid;

    while (guid != Guid.Empty) {
      var parent = Project.FindState(guid);
      GUILayout.Label(string.Format("Inherited from {0}", parent.Name), BoltEditorGUI.MiniLabelButtonStyle);

      EditorGUI.BeginDisabledGroup(true);
      EditPropertyList(parent, parent.Properties);
      EditorGUI.EndDisabledGroup();

      guid = parent.ParentGuid;
    }
  }

  void EditStruct(StructDefinition def) {
    EditHeader(def, () => {

    });

    // add button
    BoltEditorGUI.AddButton("Properties", def.Properties, () => new PropertyStateSettings());
    EditPropertyList(def, def.Properties);

    if (def.Properties.Count > 0)
      BoltEditorGUI.AddButton("", def.Properties, () => new PropertyStateSettings());
  }

  void EditEvent(EventDefinition def) {
    EditHeader(def, () => {

    }, () => {
      GUILayout.BeginHorizontal();
      GUILayout.Label("Global Senders", BoltEditorGUI.AccentText);
      GUILayout.FlexibleSpace();
      GUILayout.Label("Entity Senders", BoltEditorGUI.AccentText);
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      def.GlobalSenders = (GlobalEventSenders)EditorGUILayout.EnumPopup(def.GlobalSenders);
      BoltEditorGUI.SetTooltip("Global Senders, who can send this as an global event?");

      def.EntitySenders = (EntityEventSenders)EditorGUILayout.EnumPopup(def.EntitySenders);
      BoltEditorGUI.SetTooltip("Entity Senders, who can send this as an entity event?");
      GUILayout.EndHorizontal();
    });

    // add button
    BoltEditorGUI.AddButton("Properties", def.Properties, () => new PropertyEventSettings());
    EditPropertyList(def, def.Properties);

    if (def.Properties.Count > 0)
      BoltEditorGUI.AddButton("", def.Properties, () => new PropertyEventSettings());
  }

  void EditCommand(CommandDefinition def) {
    EditHeader(def, () => {
      def.SmoothFrames = BoltEditorGUI.IntFieldOverlay(def.SmoothFrames, "Correction Interpolation Frames");
    });

    // add button
    BoltEditorGUI.AddButton("Input", def.Input, () => new PropertyCommandSettings());
    EditPropertyList(def, def.Input);

    if (def.Input.Count > 0)
      BoltEditorGUI.AddButton("", def.Input, () => new PropertyCommandSettings());

    // add button
    BoltEditorGUI.AddButton("Result", def.Result, () => new PropertyCommandSettings());
    EditPropertyList(def, def.Result);

    if (def.Result.Count > 0)
      BoltEditorGUI.AddButton("", def.Result, () => new PropertyCommandSettings());
  }

  void BeginBackground() {
    GUILayout.BeginVertical();
  }

  void EndBackground() {
    GUILayout.EndVertical();
  }

  void EditHeader(AssetDefinition def, Action action, params Action[] rows) {
    BeginBackground();

    GUIStyle sceneStyle = "TE NodeBoxSelected";
    sceneStyle.padding = new RectOffset(3, 5, 5, 4);
    GUILayout.BeginHorizontal(sceneStyle, GUILayout.Height(22));

    GUILayout.Space(3);

    var icon = "";
    if (def is StateDefinition) { icon = "mc_state"; }
    if (def is StructDefinition) { icon = "mc_struct"; }
    if (def is EventDefinition) { icon = "mc_event"; }
    if (def is CommandDefinition) { icon = "mc_controller"; }

    if (BoltEditorGUI.Button(icon)) {

    }

    // edit asset name
    def.Name = EditorGUILayout.TextField(def.Name);

    // remaining header
    action();

    GUILayout.EndHorizontal();

    GUILayout.Label("Comment", BoltEditorGUI.AccentText);
    def.Comment = EditorGUILayout.TextArea(def.Comment);

    for (int i = 0; i < rows.Length; ++i) {
      rows[i]();
    }

    EndBackground();
  }

  void EditPropertyList(AssetDefinition def, List<PropertyDefinition> list) {
    for (int i = 0; i < list.Count; ++i) {
      EditProperty(def, list[i]);
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

  void EditProperty(AssetDefinition def, PropertyDefinition p) {
    BeginBackground();

    GUIStyle sceneStyle = "TE NodeBox";
    sceneStyle.padding = new RectOffset(3, 5, 5, 4);
    GUILayout.BeginHorizontal(sceneStyle, GUILayout.Height(22));

    if ((Event.current.modifiers & EventModifiers.Control) == EventModifiers.Control) {
      if (BoltEditorGUI.Button("mc_minus")) {
        p.Deleted = true;
      }
    }
    else {
      if (BoltEditorGUI.Toggle("mc_arrow_down", "mc_arrow_right", p.Expanded && (p.PropertyType.HasSettings || p.PropertyType.MecanimApplicable))) {
        p.Expanded = !p.Expanded;
      }
    }

    if (IsStateOrStruct(def)) {
      p.Name = EditorGUILayout.TextField(p.Name, GUILayout.Width(144));
      BoltEditorGUI.SetTooltip("Name. The name of this property, has to be a valid C# property name.");

      p.Priority = BoltEditorGUI.EditPriority(p.Priority, p.PropertyType.HasPriority);
      BoltEditorGUI.SetTooltip("Priority. A higher priority means this property is more likely to be sent.");

      if (BoltEditorGUI.Toggle("mc_controller", p.Controller)) {
        p.Controller = !p.Controller;
      }

      BoltEditorGUI.SetTooltip("Replicate To Controller. Enable if this property should be replicated from the Owner to the Controller");
    }
    else {
      p.Name = EditorGUILayout.TextField(p.Name, GUILayout.Width(200));
      BoltEditorGUI.SetTooltip("Name. The name of this property, has to be a valid C# property name.");
    }

    // edit property type
    BoltEditorGUI.PropertyTypePopup(def, p);
    BoltEditorGUI.SetTooltip("Type. The type of this property.");

    EditorGUILayout.EndHorizontal();

    if (p.Expanded) {
      //BoltEditorGUI.SettingsSection("Comment", () => {
      //  p.Comment = EditorGUILayout.TextField(p.Comment);
      //});

      if (def is CommandDefinition) {
        if (p.PropertyType.CanSmoothCorrections && ((CommandDefinition)def).Result.Contains(p)) {
          BoltEditorGUI.SettingsSection("Corrections", () => {
            BoltEditorGUI.WithLabel("Smooth Corrections", () => {
              p.CommandAssetSettings.SmoothCorrection = EditorGUILayout.Toggle(p.CommandAssetSettings.SmoothCorrection);
            });
          });
        }
      }

      if (p.PropertyType.HasSettings) {
        PropertyEditorRegistry.GetEditor(p.PropertyType.GetType()).Edit(def, p);
      }

      if (p.PropertyType.MecanimApplicable && (def is StateDefinition)) {
        BoltEditorGUI.SettingsSectionToggle("Mecanim", ref p.StateAssetSettings.MecanimEnabled, () => {

          // value push settings
          {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("Owner");
            p.StateAssetSettings.MecanimOwnerDirection = (MecanimDirection)EditorGUILayout.EnumPopup(p.StateAssetSettings.MecanimOwnerDirection);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("Controller");
            p.StateAssetSettings.MecanimControllerDirection = (MecanimDirection)EditorGUILayout.EnumPopup(p.StateAssetSettings.MecanimControllerDirection);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("Others");
            p.StateAssetSettings.MecanimOthersDirection = (MecanimDirection)EditorGUILayout.EnumPopup(p.StateAssetSettings.MecanimOthersDirection);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
          }

          if (p.PropertyType is PropertyTypeFloat) {
            EditorGUILayout.BeginHorizontal();

            p.StateAssetSettings.MecanimMode = (MecanimMode)EditorGUILayout.EnumPopup(p.StateAssetSettings.MecanimMode);

            switch (p.StateAssetSettings.MecanimMode) {
              case MecanimMode.Property: p.StateAssetSettings.MecanimDamping = BoltEditorGUI.FloatFieldOverlay(p.StateAssetSettings.MecanimDamping, "Damping Time"); break;
              case MecanimMode.LayerWeight: p.StateAssetSettings.MecanimLayer = BoltEditorGUI.IntFieldOverlay(p.StateAssetSettings.MecanimLayer, "Layer Index"); break;
            }

            EditorGUILayout.EndHorizontal();
          }
          else {
            //BoltEditorGUI.WithLabel("Enabled", () => {
            //  switch (EditorGUILayout.Toggle(p.StateAssetSettings.MecanimMode == MecanimMode.Property)) {
            //    case true: p.StateAssetSettings.MecanimMode = MecanimMode.Property; break;
            //    case false: p.StateAssetSettings.MecanimMode = MecanimMode.None; break;
            //  }
            //});
          }


          //if (p.PropertyType is PropertyTypeTrigger) {
          //  BoltEditorGUI.WithLabel("Layer Index", () => { p.StateAssetSettings.MecanimLayer = EditorGUILayout.IntField(p.StateAssetSettings.MecanimLayer); });
          //  BoltEditorGUI.SetTooltip("Which layer index this trigger creates transitions on");
          //}
        }, GUILayout.Width(50));
      }
    }

    EditorGUILayout.EndVertical();
  }

  bool IsStateOrStruct(AssetDefinition def) {
    return (def is StateDefinition) || (def is StructDefinition);
  }

  void StateAndStructToolbar(AssetDefinition def, PropertyDefinition p) {
    EditFilters(p);

    if (BoltEditorGUI.Toggle("boltico_playcom2", p.Controller)) {
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

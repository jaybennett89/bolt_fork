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
    EditHeader(def);

    BoltEditorGUI.WithLabel("IsAbstract", () => {
      def.IsAbstract = BoltEditorGUI.Toggle(def.IsAbstract);
    });

    BoltEditorGUI.WithLabel("Parent State", () => {
      def.ParentGuid = BoltEditorGUI.AssetPopup(Project.States.Cast<AssetDefinition>(), def.ParentGuid, Project.GetInheritanceTree(def));
    });

    if (!def.IsAbstract) {
      BoltEditorGUI.WithLabel("Bandwidth", () => {
        GUILayout.BeginHorizontal();
        def.PacketMaxBits = Mathf.Clamp(BoltEditorGUI.IntFieldOverlay(def.PacketMaxBits, "Bits/Packet"), 128, 4096);
        def.PacketMaxProperties = Mathf.Clamp(BoltEditorGUI.IntFieldOverlay(def.PacketMaxProperties, "Properties/Packet"), 1, 255);
        GUILayout.EndHorizontal();
      });
    }

    BoltEditorGUI.AddButton("Properties", def.Properties, () => new PropertyStateSettings());
    EditPropertyList(def, def.Properties);

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
    EditHeader(def);

    // add button
    BoltEditorGUI.AddButton("Properties", def.Properties, () => new PropertyStateSettings());
    EditPropertyList(def, def.Properties);
  }

  void EditEvent(EventDefinition def) {
    EditHeader(def);

    BoltEditorGUI.WithLabel("Global Senders", () => {
      def.GlobalSenders = (GlobalEventSenders)EditorGUILayout.EnumPopup(def.GlobalSenders);
      BoltEditorGUI.SetTooltip("Who can send this as an global event?");
    });

    BoltEditorGUI.WithLabel("Entity Senders", () => {
      def.EntitySenders = (EntityEventSenders)EditorGUILayout.EnumPopup(def.EntitySenders);
      BoltEditorGUI.SetTooltip("Who can send this as an entity event?");
    });

    // add button
    BoltEditorGUI.AddButton("Properties", def.Properties, () => new PropertyEventSettings());
    EditPropertyList(def, def.Properties);
  }

  void EditCommand(CommandDefinition def) {
    EditHeader(def);

    BoltEditorGUI.WithLabel("Correction Interpolation", () => {
      def.SmoothFrames = BoltEditorGUI.IntFieldOverlay(def.SmoothFrames, "Frames");
    });

    // add button
    BoltEditorGUI.AddButton("Input", def.Input, () => new PropertyCommandSettings());
    EditPropertyList(def, def.Input);

    if (def.Input.Count > 0)
      BoltEditorGUI.AddButton("", def.Input, () => new PropertyCommandSettings());

    // add button
    BoltEditorGUI.AddButton("Result", def.Result, () => new PropertyCommandSettings());
    EditPropertyList(def, def.Result);
  }

  void BeginBackground() {
    GUILayout.BeginVertical();
  }

  void EndBackground() {
    GUILayout.EndVertical();
  }

  void EditHeader(AssetDefinition def) {
    BeginBackground();

    GUIStyle sceneStyle = "TE NodeBoxSelected";
    sceneStyle.padding = new RectOffset(3, 5, 5, 4);
    GUILayout.BeginHorizontal(sceneStyle, GUILayout.Height(22));

    GUILayout.Space(3);

    if (def is StateDefinition) { BoltEditorGUI.Button("mc_state"); }
    if (def is StructDefinition) { BoltEditorGUI.Button("mc_struct"); }
    if (def is EventDefinition) { BoltEditorGUI.Button("mc_event"); }
    if (def is CommandDefinition) { BoltEditorGUI.Button("mc_controller"); }

    // edit asset name
    def.Name = EditorGUILayout.TextField(def.Name);

    GUILayout.EndHorizontal();
    GUILayout.Space(4);

    BoltEditorGUI.WithLabel("Comment", () => { def.Comment = EditorGUILayout.TextArea(def.Comment); });

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

    p.Name = EditorGUILayout.TextField(p.Name, GUILayout.Width(200));
    BoltEditorGUI.SetTooltip("Name. The name of this property, has to be a valid C# property name.");

    // edit property type
    BoltEditorGUI.PropertyTypePopup(def, p);
    BoltEditorGUI.SetTooltip("Type. The type of this property.");

    EditorGUILayout.EndHorizontal();

    if (p.Expanded) {
      GUILayout.Space(2);

      BoltEditorGUI.WithLabel("Comment", () => {
        p.Comment = EditorGUILayout.TextField(p.Comment);
      });

      if (def is StateDefinition || def is StructDefinition) {
        BoltEditorGUI.WithLabel("Priority", () => {
          p.Priority = BoltEditorGUI.EditPriority(p.Priority, p.PropertyType.HasPriority);
        });

        BoltEditorGUI.WithLabel("To Controller", () => {
          p.Controller = BoltEditorGUI.Toggle(p.Controller);
        });
      }
      else if (def is CommandDefinition) {
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
        BoltEditorGUI.WithLabel("Mecanim", () => {
          EditorGUILayout.BeginHorizontal();

          if (p.PropertyType is PropertyTypeFloat) {
            p.StateAssetSettings.MecanimMode = (MecanimMode)EditorGUILayout.EnumPopup(p.StateAssetSettings.MecanimMode);
            EditorGUI.BeginDisabledGroup(p.StateAssetSettings.MecanimMode == MecanimMode.Disabled);

            p.StateAssetSettings.MecanimDirection = (MecanimDirection)EditorGUILayout.EnumPopup(p.StateAssetSettings.MecanimDirection);

            switch (p.StateAssetSettings.MecanimMode) {
              case MecanimMode.Parameter: p.StateAssetSettings.MecanimDamping = BoltEditorGUI.FloatFieldOverlay(p.StateAssetSettings.MecanimDamping, "Damping Time"); break;
              case MecanimMode.LayerWeight: p.StateAssetSettings.MecanimLayer = BoltEditorGUI.IntFieldOverlay(p.StateAssetSettings.MecanimLayer, "Layer Index"); break;
            }

            EditorGUI.EndDisabledGroup();
          }
          else {
            p.StateAssetSettings.MecanimMode = (MecanimMode)(int)EditorGUILayout.Popup((int)p.StateAssetSettings.MecanimMode, new string[] { "Disabled", "Parameter" });

            EditorGUI.BeginDisabledGroup(p.StateAssetSettings.MecanimMode == MecanimMode.Disabled);
            p.StateAssetSettings.MecanimDirection = (MecanimDirection)EditorGUILayout.EnumPopup(p.StateAssetSettings.MecanimDirection);

            if (p.PropertyType is PropertyTypeTrigger) {
              p.StateAssetSettings.MecanimLayer = BoltEditorGUI.IntFieldOverlay(p.StateAssetSettings.MecanimLayer, "Layer Index");
            }
            EditorGUI.EndDisabledGroup();
          }

          EditorGUILayout.EndHorizontal();
        });
      }
    }

    EditorGUILayout.EndVertical();
  }

  bool IsStateOrStruct(AssetDefinition def) {
    return ;
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

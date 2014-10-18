using Bolt.Compiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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


  void Editor() {
    if ((Selected is AssetDefinition) && (ReferenceEquals(Selected, Selected) == false)) {
      Selected = (AssetDefinition)Selected;
    }

    if (Selected != null) {
      if (Selected is StateDefinition) {
        EditState((StateDefinition)Selected);
      }

      if (Selected is StructDefinition) {
        EditStruct((StructDefinition)Selected);
      }

      if (Selected is EventDefinition) {
        EditEvent((EventDefinition)Selected);
      }

      if (Selected is CommandDefinition) {
        EditCommand((CommandDefinition)Selected);
      }
    }
  }


  void EditState(StateDefinition def) {
    EditHeader(def);

    BoltEditorGUI.WithLabel("Is Abstract", () => {
      def.IsAbstract = BoltEditorGUI.Toggle(def.IsAbstract);
    });

    BoltEditorGUI.WithLabel("Parent State", () => {
      def.ParentGuid = BoltEditorGUI.AssetPopup(Project.States.Cast<AssetDefinition>(), def.ParentGuid, Project.GetInheritanceTree(def));
    });

    EditorGUI.BeginDisabledGroup(def.IsAbstract);

    BoltEditorGUI.WithLabel("Bandwidth", () => {
      GUILayout.BeginHorizontal();
      def.PacketMaxBits = Mathf.Clamp(BoltEditorGUI.IntFieldOverlay(def.PacketMaxBits, "Bits/Packet"), 128, 4096);
      def.PacketMaxProperties = Mathf.Clamp(BoltEditorGUI.IntFieldOverlay(def.PacketMaxProperties, "Properties/Packet"), 1, 255);
      GUILayout.EndHorizontal();
    });

    EditorGUI.EndDisabledGroup();

    var groups =
      def.Properties
        .Where(x => x.PropertyType.MecanimApplicable)
        .Where(x => x.StateAssetSettings.MecanimMode != MecanimMode.Disabled)
        .GroupBy(x => x.StateAssetSettings.MecanimDirection);

    if (groups.Count() == 1) {
      var currentDirection = groups.First().Key;

      BoltEditorGUI.WithLabel("Mecanim (State Wide)", () => {
        var selectedDirection = (MecanimDirection)EditorGUILayout.EnumPopup(currentDirection);

        if (currentDirection != selectedDirection) {
          foreach (var property in def.Properties.Where(x => x.PropertyType.MecanimApplicable)) {
            property.StateAssetSettings.MecanimDirection = selectedDirection;
          }

          Save();
        }
      });
    }
    else if (groups.Count() > 1) {
      BoltEditorGUI.WithLabel("Mecanim (State Wide)", () => {
        string[] options = new string[] { "Using Animator Methods", "Using Bolt Properties", "Mixed (WARNING)" };

        int index = EditorGUILayout.Popup(2, options);

        if (index != 2) {
          foreach (var property in def.Properties.Where(x => x.PropertyType.MecanimApplicable)) {
            property.StateAssetSettings.MecanimDirection = (MecanimDirection)index;
          }

          Save();
        }
      });
    }

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
    EditPropertyList(def, def.Properties);
  }

  PropertyDefinition CreateProperty(PropertyAssetSettings settings) {
    return new PropertyDefinition {
      Name = "NewProperty",
      Comment = "",
      Deleted = false,
      Enabled = true,
      Expanded = true,
      PropertyType = new PropertyTypeFloat { Compression = FloatCompression.Default() },
      AssetSettings = settings
    };
  }

  void EditCommand(CommandDefinition def) {
    EditHeader(def);

    BoltEditorGUI.WithLabel("Correction Interpolation", () => {
      def.SmoothFrames = BoltEditorGUI.IntFieldOverlay(def.SmoothFrames, "Frames");
    });

    // add button
    EditPropertyList(def, def.Input);

    // add button
    GUILayout.Label("Result", EditorStyles.boldLabel);
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

    var stateDef = def as StateDefinition;
    var structDef = def as StructDefinition;
    var cmdDef = def as CommandDefinition;
    var eventDef = def as EventDefinition;

    GUI.color = BoltEditorSkin.Selected.Variation.TintColor;
    GUILayout.BeginHorizontal(BoltEditorGUI.BoxStyle(BoltEditorSkin.Selected.Background), GUILayout.Height(22));
    GUI.color = Color.white;

    GUILayout.Space(3);

    if (def is StateDefinition) { BoltEditorGUI.Button("mc_state"); }
    if (def is StructDefinition) { BoltEditorGUI.Button("mc_struct"); }
    if (def is EventDefinition) { BoltEditorGUI.Button("mc_event"); }
    if (def is CommandDefinition) { BoltEditorGUI.Button("mc_controller"); }

    // edit asset name
    def.Name = EditorGUILayout.TextField(def.Name);

    if (cmdDef != null) {
      if (GUILayout.Button("New Input", EditorStyles.miniButtonLeft, GUILayout.Width(75))) {
        cmdDef.Input.Add(CreateProperty(new PropertyCommandSettings()));
        Save();
      }

      if (GUILayout.Button("New Result", EditorStyles.miniButtonRight, GUILayout.Width(75))) {
        cmdDef.Result.Add(CreateProperty(new PropertyCommandSettings()));
        Save();
      }
    }
    else {
      if (GUILayout.Button("New Property", EditorStyles.miniButton, GUILayout.Width(150))) {
        if (stateDef != null) {
          stateDef.Properties.Add(CreateProperty(new PropertyStateSettings()));
          Save();
        }

        if (structDef != null) {
          structDef.Properties.Add(CreateProperty(new PropertyStateSettings()));
          Save();
        }

        if (eventDef != null) {
          eventDef.Properties.Add(CreateProperty(new PropertyEventSettings()));
          Save();
        }
      }
    }

    GUILayout.EndHorizontal();
    GUILayout.Space(2);

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

    GUI.color = BoltEditorSkin.Selected.Variation.TintColor;
    GUILayout.BeginHorizontal(BoltEditorGUI.BoxStyle(BoltEditorSkin.Selected.Background), GUILayout.Height(22));
    GUI.color = Color.white;

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

    if (def is StateDefinition || def is StructDefinition) {
      p.Name = BoltEditorGUI.TextFieldOverlay(p.Name, p.Priority.ToString(), GUILayout.Width(181));
      p.Controller = BoltEditorGUI.Toggle("mc_controller", p.Controller);
    }
    else {
      p.Name = EditorGUILayout.TextField(p.Name, GUILayout.Width(200));
    }

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
        BoltEditorGUI.WithLabel("Replication", () => {
          p.Priority = BoltEditorGUI.EditPriority(p.Priority, p.PropertyType.HasPriority);
          p.Controller = BoltEditorGUI.ToggleDropdown("Replicate To Controller", "Don't Replicate To Controller", p.Controller);
        });
      }

      if (def is CommandDefinition) {
        if (p.PropertyType.CanSmoothCorrections && ((CommandDefinition)def).Result.Contains(p)) {
          BoltEditorGUI.WithLabel("Smooth Corrections", () => {
            p.CommandAssetSettings.SmoothCorrection = EditorGUILayout.Toggle(p.CommandAssetSettings.SmoothCorrection);
          });
        }
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

      if (p.PropertyType.HasSettings) {
        PropertyEditorRegistry.GetEditor(p.PropertyType.GetType()).Edit(def, p);
      }
    }

    EditorGUILayout.EndVertical();
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

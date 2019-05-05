using Bolt.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY5
using AC = UnityEditor.Animations.AnimatorController;
using ACP = UnityEngine.AnimatorControllerParameter;
using ACPT = UnityEngine.AnimatorControllerParameterType;
#else
using AC = UnityEditorInternal.AnimatorController;
using ACP = UnityEditorInternal.AnimatorControllerParameter;
using ACPT = UnityEditorInternal.AnimatorControllerParameterType;
#endif

public class BoltEditorWindow : BoltWindow {
  [MenuItem("Window/Bolt/Editor", priority = -99)]
  public static void Open() {
    BoltEditorWindow w;

    w = EditorWindow.GetWindow<BoltEditorWindow>();
    w.title = "Bolt Editor";
    w.name = "Bolt Editor";
    w.minSize = new Vector2(300, 400);
    w.Show();
    w.Focus();
  }

  Vector2 scroll;

  new void OnGUI() {
    base.OnGUI();

    if (HasProject) {
      Editor();
      Header();
    }

    if (GUI.changed) {
      Save();
    }

    ClearAllFocus();
  }


  void Editor() {
    if (Selected != null) {
      GUILayout.BeginArea(new Rect(BoltEditorGUI.GLOBAL_INSET, 22, position.width - (BoltEditorGUI.GLOBAL_INSET * 2), position.height - 22));

      scroll = GUILayout.BeginScrollView(scroll, false, false, GUIStyle.none, GUIStyle.none);

      GUILayout.Space(5);

      if (Selected is StateDefinition) {
        EditState((StateDefinition)Selected);
      }

      if (Selected is ObjectDefinition) {
        EditStruct((ObjectDefinition)Selected);
      }

      if (Selected is EventDefinition) {
        EditEvent((EventDefinition)Selected);
      }

      if (Selected is CommandDefinition) {
        EditCommand((CommandDefinition)Selected);
      }

      GUILayout.EndScrollView();
      GUILayout.EndArea();
    }
  }

  void Header() {
    if (Selected != null) {
      EditHeader(Selected);
    }
  }

  RuntimeAnimatorController mecanimController;

  void ImportMecanimLayer(StateDefinition def, AC ac, int layer) {
    string name = "MecanimLayer_" + layer + "_Weight";

    PropertyDefinition pdef = def.Properties.FirstOrDefault(x => x.StateAssetSettings.MecanimLayer == layer && x.StateAssetSettings.MecanimMode == MecanimMode.LayerWeight);

    if (pdef == null) {
      pdef = CreateProperty(new PropertyStateSettings());
      pdef.PropertyType = new PropertyTypeFloat() { Compression = new FloatCompression { Accuracy = 0.01f, MinValue = 0, MaxValue = 1, Enabled = true } };
      pdef.Name = name;
      pdef.StateAssetSettings.MecanimLayer = layer;
      pdef.StateAssetSettings.MecanimMode = MecanimMode.LayerWeight;
      pdef.StateAssetSettings.MecanimDirection = MecanimDirection.UsingAnimatorMethods;

      Debug.Log(string.Format("Imported Mecanim Layer: {0}", pdef.Name));

      def.Properties.Add(pdef);
    }
    else if (!(pdef.PropertyType is PropertyTypeFloat)) {
      pdef.PropertyType = new PropertyTypeFloat() { Compression = new FloatCompression { Accuracy = 0.01f, MinValue = 0, MaxValue = 1, Enabled = true } };
      Debug.Log(string.Format("Updated Mecanim Layer: {0}", pdef.Name));
    }
  }

  void ImportMecanimParameter(StateDefinition def, ACP p) {
    PropertyType type = null;

    switch (p.type) {
      case ACPT.Trigger: type = new PropertyTypeTrigger(); break;
      case ACPT.Bool: type = new PropertyTypeBool(); break;
      case ACPT.Int: type = new PropertyTypeInteger(); break;
      case ACPT.Float: type = new PropertyTypeFloat(); break;
    }

    PropertyDefinition pdef = def.Properties.FirstOrDefault(x => x.Name == p.name);

    if (pdef == null) {
      pdef = CreateProperty(new PropertyStateSettings());
      pdef.PropertyType = type;
      pdef.Name = p.name;
      pdef.ReplicationMode = replicationMode;
      pdef.StateAssetSettings.MecanimMode = MecanimMode.Parameter;
      pdef.StateAssetSettings.MecanimDirection = mecanimDirection;

      Debug.Log(string.Format("Imported Mecanim Parameter: {0}", pdef.Name));

      def.Properties.Add(pdef);
    }
    else if (pdef.PropertyType.GetType() != type.GetType()) {
      pdef.PropertyType = type;
      Debug.Log(string.Format("Updated Mecanim Parameter: {0}", pdef.Name));
    }
  }

  ReplicationMode replicationMode;
  MecanimDirection mecanimDirection;

  void EditState(StateDefinition def) {
    BoltEditorGUI.WithLabel("Inheritance", () => {
      def.IsAbstract = BoltEditorGUI.ToggleDropdown("Is Abstract", "Is Concrete", def.IsAbstract);
      def.ParentGuid = BoltEditorGUI.AssetPopup("Parent: ", Project.States.Cast<AssetDefinition>(), def.ParentGuid, Project.GetInheritanceTree(def));
    });

    EditorGUI.BeginDisabledGroup(def.IsAbstract);

    BoltEditorGUI.WithLabel("Bandwidth", () => {
      GUILayout.BeginHorizontal();
      def.PacketMaxBits = Mathf.Clamp(BoltEditorGUI.IntFieldOverlay(def.PacketMaxBits, "Bits/Packet"), 128, 4096);
      def.PacketMaxProperties = Mathf.Clamp(BoltEditorGUI.IntFieldOverlay(def.PacketMaxProperties, "Properties/Packet"), 1, 255);
      GUILayout.EndHorizontal();
    });

    EditorGUI.EndDisabledGroup();

    BoltEditorGUI.WithLabel("Import Mecanim Modes", () => {
      replicationMode = (ReplicationMode)EditorGUILayout.EnumPopup("Replication Mode", replicationMode);
      mecanimDirection = (MecanimDirection)EditorGUILayout.EnumPopup("Mecanim Mode", mecanimDirection);
    });

    BoltEditorGUI.WithLabel("Import Mecanim Parameters", () => {
      mecanimController = EditorGUILayout.ObjectField(mecanimController, typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;

      if (mecanimController) {
        if (GUILayout.Button("Import", EditorStyles.miniButton)) {
          try {
            AC ac = (AC)mecanimController;

#if UNITY5
            for (int i = 0; i < ac.parameters.Length; ++i) {
              ImportMecanimParameter(def, ac.parameters[i]);
            }
#else
            for (int i = 0; i < ac.parameterCount; ++i) {
              ImportMecanimParameter(def, ac.GetParameter(i));
            }

            for (int i = 0; i < ac.layerCount; ++i) {
              ImportMecanimLayer(def, ac, i);
            }
#endif

            Save();
          }
          finally {
            mecanimController = null;
          }
        }
      }
    });

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


  void EditStruct(ObjectDefinition def) {
    // add button
    EditPropertyList(def, def.Properties);
  }

  void EditEvent(EventDefinition def) {
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
    PropertyDefinition def = new PropertyDefinition {
      Name = "NewProperty",
      PropertyType = new PropertyTypeFloat { Compression = FloatCompression.Default() },
      AssetSettings = settings,
      ReplicationMode = ReplicationMode.Everyone,
    };

    def.Oncreated();
    return def;
  }

  void EditCommand(CommandDefinition def) {
    BoltEditorGUI.WithLabel("Correction Interpolation", () => {
      def.SmoothFrames = BoltEditorGUI.IntFieldOverlay(def.SmoothFrames, "Frames");
    });

    //BoltEditorGUI.WithLabel("Compress Zero Values", () => {
    //  def.CompressZeroValues = EditorGUILayout.Toggle(def.CompressZeroValues);
    //});

    // add button
    BoltEditorGUI.Header("Input", "mc_command");
    GUILayout.Space(2);
    EditPropertyList(def, def.Input);

    // add button
    BoltEditorGUI.Header("Result", "mc_position");
    GUILayout.Space(2);
    EditPropertyList(def, def.Result);
  }

  void BeginBackground() {
    GUILayout.BeginVertical();
  }

  void EndBackground() {
    GUILayout.EndVertical();
  }

  void EditHeader(AssetDefinition def) {
    var stateDef = def as StateDefinition;
    var structDef = def as ObjectDefinition;
    var cmdDef = def as CommandDefinition;
    var eventDef = def as EventDefinition;

    GUILayout.BeginArea(new Rect(BoltEditorGUI.GLOBAL_INSET, BoltEditorGUI.GLOBAL_INSET, position.width - (BoltEditorGUI.GLOBAL_INSET * 2), BoltEditorGUI.HEADER_HEIGHT));
    GUILayout.BeginHorizontal(BoltEditorGUI.HeaderBackgorund, GUILayout.Height(BoltEditorGUI.HEADER_HEIGHT));

    if (def is StateDefinition) { BoltEditorGUI.IconButton("mc_state"); }
    if (def is ObjectDefinition) { BoltEditorGUI.IconButton("mc_struct"); }
    if (def is EventDefinition) { BoltEditorGUI.IconButton("mc_event"); }
    if (def is CommandDefinition) { BoltEditorGUI.IconButton("mc_command"); }

    // edit asset name
    GUI.SetNextControlName("BoltEditorName");
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

    if (stateDef != null) { ExpandAllOrCollapseAll(stateDef.Properties); }
    if (structDef != null) { ExpandAllOrCollapseAll(structDef.Properties); }
    if (eventDef != null) { ExpandAllOrCollapseAll(eventDef.Properties); }
    if (cmdDef != null) { ExpandAllOrCollapseAll(cmdDef.Input, cmdDef.Result); }

    if (stateDef != null) { Duplicate(stateDef); }
    if (structDef != null) { Duplicate(structDef); }
    if (eventDef != null) { Duplicate(eventDef); }
    if (cmdDef != null) { Duplicate(cmdDef); }

    GUILayout.EndHorizontal();
    GUILayout.EndArea();
  }

  void Duplicate<T>(T obj) where T : AssetDefinition {
    if (GUILayout.Button("Duplicate", EditorStyles.miniButton, GUILayout.Width(80))) {
      obj = SerializerUtils.DeepClone(obj);
      obj.Guid = Guid.NewGuid();

      Project.RootFolder.Assets = Project.RootFolder.Assets.Add(obj);

      Save();
    }
  }

  void ExpandAllOrCollapseAll(params IEnumerable<PropertyDefinition>[] defs) {
    if (defs.SelectMany(x => x).Count(x => x.Expanded) > 0) {
      if (GUILayout.Button("Collapse All", EditorStyles.miniButton, GUILayout.Width(80))) {
        foreach (var d in defs.SelectMany(x => x)) {
          d.Expanded = false;
        }
      }
    }
    else {
      if (GUILayout.Button("Expand All", EditorStyles.miniButton, GUILayout.Width(80))) {
        foreach (var d in defs.SelectMany(x => x)) {
          d.Expanded = true;
        }
      }
    }
  }

  void EditPropertyList(AssetDefinition def, List<PropertyDefinition> list) {
    for (int i = 0; i < list.Count; ++i) {
      EditProperty(def, list[i], i == 0, i == (list.Count - 1));
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

        // save
        Save();
      }
    }

    // adjust properties
    for (int i = 0; i < list.Count; ++i) {
      if (list[i].Adjust != 0) {
        var self = list[i];
        var other = list[i + list[i].Adjust];

        list[i + list[i].Adjust] = self;
        list[i] = other;

        self.Adjust = 0;
        other.Adjust = 0;

        Save();
      }
    }

  }

  void EditProperty(AssetDefinition def, PropertyDefinition p, bool first, bool last) {
    BeginBackground();

    GUILayout.BeginHorizontal(BoltEditorGUI.HeaderBackgorund, GUILayout.Height(BoltEditorGUI.HEADER_HEIGHT));

    if ((Event.current.modifiers & EventModifiers.Control) == EventModifiers.Control) {
      if (BoltEditorGUI.IconButton("mc_minus")) {
        if (EditorUtility.DisplayDialog("Delete Property", string.Format("Do you want to delete '{0}' (Property)?", p.Name), "Yes", "No")) {
          p.Deleted = true;
        }
      }
    }
    else {
      if (BoltEditorGUI.Toggle("mc_arrow_down", "mc_arrow_right", p.Expanded && (p.PropertyType.HasSettings || p.PropertyType.MecanimApplicable))) {
        p.Expanded = !p.Expanded;
      }
    }

    if (def is StateDefinition || def is ObjectDefinition) {
      p.Name = BoltEditorGUI.TextFieldOverlay(p.Name, p.Priority.ToString(), GUILayout.Width(181));

      switch (p.ReplicationMode) {
        case ReplicationMode.Everyone:
          BoltEditorGUI.Toggle("mc_controller_plus", true);
          break;

        case ReplicationMode.EveryoneExceptController:
          BoltEditorGUI.Toggle("mc_controller", false);
          break;

        case ReplicationMode.OnlyOwnerAndController:
          BoltEditorGUI.Toggle("mc_controller_only", true);
          break;

        case ReplicationMode.LocalForEachPlayer:
          BoltEditorGUI.Toggle("mc_owner_only", true);
          break;
      }

    }
    else {
      p.Name = EditorGUILayout.TextField(p.Name, GUILayout.Width(200));
    }

    BoltEditorGUI.SetTooltip("Name. The name of this property, has to be a valid C# property name.");

    // edit property type
    BoltEditorGUI.PropertyTypePopup(def, p);
    BoltEditorGUI.SetTooltip("Type. The type of this property.");

    EditorGUI.BeginDisabledGroup(def.SortOrder != SortOrder.Manual);

    if (BoltEditorGUI.IconButton("mc_arrow_down", !last)) {
      p.Adjust += 1;
    }

    if (BoltEditorGUI.IconButton("mc_arrow_up", !first)) {
      p.Adjust -= 1;
    }

    EditorGUI.EndDisabledGroup();
    EditorGUILayout.EndHorizontal();

    if (p.Controller) {
      p.ReplicationMode = ReplicationMode.Everyone;
      p.Controller = false;
      Save();
    }

    if (p.Expanded) {
      GUILayout.Space(2);

      //BoltEditorGUI.WithLabel("Comment", () => {
      //  p.Comment = EditorGUILayout.TextField(p.Comment);
      //});

      if (def is StateDefinition || def is ObjectDefinition) {
        BoltEditorGUI.WithLabel("Replication", () => {
          p.Priority = BoltEditorGUI.EditPriority(p.Priority, p.PropertyType.HasPriority);
          p.ReplicationMode = (ReplicationMode)EditorGUILayout.EnumPopup(p.ReplicationMode);
          BoltEditorGUI.Help("http://doc.photonengine.com/en/bolt/current/in-depth/replication-modes");
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
              case MecanimMode.Parameter:
                if (p.StateAssetSettings.MecanimDirection == MecanimDirection.UsingBoltProperties) {
                  p.StateAssetSettings.MecanimDamping = BoltEditorGUI.FloatFieldOverlay(p.StateAssetSettings.MecanimDamping, "Damping Time");
                }

                break;

              case MecanimMode.LayerWeight:
                p.StateAssetSettings.MecanimLayer = BoltEditorGUI.IntFieldOverlay(p.StateAssetSettings.MecanimLayer, "Layer Index");
                break;
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

          BoltEditorGUI.Help("https://doc.photonengine.com/en/bolt/current/getting-started/bolt-105-animations");

          EditorGUILayout.EndHorizontal();
        });
      }

      if (p.PropertyType.HasSettings) {
        PropertyEditorRegistry.GetEditor(p.PropertyType.GetType()).Edit(def, p);
      }
    }
    else {
      GUILayout.Space(2);
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

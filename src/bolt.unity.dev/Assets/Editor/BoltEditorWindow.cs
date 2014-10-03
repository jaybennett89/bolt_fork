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
    });

    // add button
    // GUILayout.Label("Defined Properties", BoltEditorGUI.MiniLabelButtonStyle);

    BoltEditorGUI.AddButton("Defined Properties", def.Properties, () => new PropertyDefinitionStateAssetSettings());

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
    EditHeader(def, BoltEditorGUI.StateHeaderStyle, BoltEditorGUI.StructHeaderColor, () => {

    });

    // add button
    BoltEditorGUI.AddButton("Defined Properties", def.Properties, () => new PropertyDefinitionStateAssetSettings());

    // list properties
    EditPropertyList(def, def.Properties, StateAndStructToolbar);

  }

  void EditHeader(AssetDefinition def, GUIStyle style, Color color, Action action) {
    GUI.color = color;
    GUILayout.BeginVertical(style);
    GUI.color = Color.white;
    GUILayout.BeginHorizontal();

    if (def is StructDefinition) {
      BoltEditorGUI.Icon("boltico_object", new RectOffset(3, 0, 2, 0));
    }

    if (def is StateDefinition) {
      BoltEditorGUI.Icon("boltico_replistate2", new RectOffset(3, 0, 2, 0));

    }

    // edit asset name
    def.Name = EditorGUILayout.TextField(def.Name);

    // remaining header
    action();

    GUILayout.EndHorizontal();

    GUILayout.BeginHorizontal();
    GUILayout.Label("//", BoltEditorGUI.InheritanceSeparatorStyle, GUILayout.Width(15));
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
      if (BoltEditorGUI.OnOffButton("boltico_arrow_down".ToContent(), "boltico_arrow_right".ToContent(), p.Expanded && p.PropertyType.HasSettings)) {
        p.Expanded = !p.Expanded;
      }
    }

    // edit priority
    if (p.PropertyType.HasPriority) {
      int priorityIndex;
      priorityIndex = Mathf.Max(Array.IndexOf(PrimeList.Below1000Except2, p.Priority), 0);
      priorityIndex = EditorGUILayout.Popup(priorityIndex, PrimeList.Below1000Except2Names, GUILayout.Width(45));
      p.Priority = PrimeList.Below1000Except2[priorityIndex];
      BoltEditorGUI.SetTooltip("Property Priority. Has to be a prime number below 1000 other than 2, a higher value means the property is more likely to be sent.");
    }
    else {
      BoltEditorGUI.Disabled(() => {
        EditorGUILayout.Popup(0, new string[] { "-" }, GUILayout.Width(45));
      });
    }

    // edit name
    p.Name = EditorGUILayout.TextField(p.Name, GUILayout.Width(151));
    BoltEditorGUI.SetTooltip("Property Name. Has to be a valid C# property name.");

    // edit property type
    BoltEditorGUI.PropertyTypePopup(def, p);
    BoltEditorGUI.SetTooltip("Property Type.");

    if (toolbar != null) {
      toolbar(def, p);
    }

    EditorGUILayout.EndHorizontal();

    if (p.Expanded && p.PropertyType.HasSettings) {
      EditorGUILayout.BeginHorizontal();
      GUILayout.Space(20);

      EditorGUILayout.BeginVertical();
      EditorGUILayout.LabelField("Settings", BoltEditorGUI.SmallWhiteText);

      if (IsStateOrStruct(def)) {
        EditStateAssetSettings(p);
      }

      PropertyEditorRegistry.GetEditor(p.PropertyType.GetType()).Edit(def, p);
      EditorGUILayout.EndVertical();

      GUILayout.Space(20);
      EditorGUILayout.EndHorizontal();
    }

    EditorGUILayout.EndVertical();
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

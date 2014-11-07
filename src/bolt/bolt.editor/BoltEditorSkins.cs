//#define COLOR_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

using UE = UnityEngine;

public struct BoltEditorSkinVariation {
  public UE.Color IconColor;
  public UE.Color HeaderIconColor;
  public UE.Color TintColor;

  public BoltEditorSkinVariation(UE.Color headerColor, UE.Color iconColor) {
    IconColor = iconColor;
    HeaderIconColor = iconColor;
    TintColor = headerColor;
  }
}

public struct BoltEditorSkin {
  public static int Count {
    get { return skins.Length; } 
  }

  public static IEnumerable<BoltEditorSkin> All {
    get { return skins; }
  }

  public static BoltEditorSkin Selected {
    get {
      try {
        return skins[UE.Mathf.Clamp(BoltRuntimeSettings.instance.editorSkin, 0, skins.Length)];
      }
      catch {
        return skins[0];
      }
    }
#if COLOR_EDITOR
    set {
      try {
        skins[UE.Mathf.Clamp(BoltRuntimeSettings.instance.editorSkin, 0, skins.Length)] = value;
      }
      catch {
        skins[0] = value;
      }
    }
#endif
  }

  public readonly string Name;
  public readonly int Background;

  BoltEditorSkinVariation darkVariation;
  BoltEditorSkinVariation lightVariation;

  public BoltEditorSkinVariation Variation {
    get {
      if (EditorGUIUtility.isProSkin) {
        return darkVariation;
      }

      return lightVariation;
    }
    set {
      if (EditorGUIUtility.isProSkin) {
        darkVariation = value;
      }

      lightVariation = value;
    }
  }

  public UE.Color IconColor {
    get {
      return Variation.IconColor;
    }
  }

  BoltEditorSkin(string name, int background, BoltEditorSkinVariation variation) {
    Name = name;
    Background = background;
    darkVariation = variation;
    lightVariation = variation;
  }

  BoltEditorSkin(string name, int background, BoltEditorSkinVariation light, BoltEditorSkinVariation dark) {
    Name = name;
    Background = background;
    darkVariation = dark;
    lightVariation = light;
  }

  static readonly BoltEditorSkin[] skins;

  static BoltEditorSkin() {
    skins = new BoltEditorSkin[] {
      new BoltEditorSkin("Default", 0, 
        new BoltEditorSkinVariation(UE.Color.white, BoltEditorGUI.ColorInt(15, 20, 28)),
        new BoltEditorSkinVariation(UE.Color.white, BoltEditorGUI.ColorInt(125, 137, 156))
      ),

      new BoltEditorSkin("Blue", 1, 
        new BoltEditorSkinVariation(BoltEditorGUI.ColorInt(174, 225, 195), BoltEditorGUI.ColorInt(40, 66, 109)),
        new BoltEditorSkinVariation(BoltEditorGUI.ColorInt(174, 225, 195), BoltEditorGUI.ColorInt(137, 226, 255))
      ),

      new BoltEditorSkin("Orange", 4, 
          new BoltEditorSkinVariation(BoltEditorGUI.ColorInt(255, 209, 44), BoltEditorGUI.ColorInt(90, 31, 5)),
          new BoltEditorSkinVariation(BoltEditorGUI.ColorInt(214, 119, 0), BoltEditorGUI.ColorInt(255, 185, 0))
      ),

      new BoltEditorSkin("Green", 3, 
          new BoltEditorSkinVariation(BoltEditorGUI.ColorInt(114, 197, 0), BoltEditorGUI.ColorInt(19, 68, 0)),
          new BoltEditorSkinVariation(BoltEditorGUI.ColorInt(114, 255, 0), BoltEditorGUI.ColorInt(147, 184, 132))
      ),

      new BoltEditorSkin("Red", 6, 
          new BoltEditorSkinVariation(BoltEditorGUI.ColorInt(255, 255, 255), BoltEditorGUI.ColorInt(124, 0, 0)),
          new BoltEditorSkinVariation(BoltEditorGUI.ColorInt(255, 255, 255), BoltEditorGUI.ColorInt(231, 159, 137))
      ),

      new BoltEditorSkin("Dusk", 6, 
          new BoltEditorSkinVariation(BoltEditorGUI.ColorInt(71, 176, 172), BoltEditorGUI.ColorInt(79, 108, 126)),
          new BoltEditorSkinVariation(BoltEditorGUI.ColorInt(67, 105, 227), BoltEditorGUI.ColorInt(174, 167, 255))
      ),

      new BoltEditorSkin("Neon", 4, 
          new BoltEditorSkinVariation(BoltEditorGUI.ColorInt(150, 255, 151), BoltEditorGUI.ColorInt(172, 0, 221)),
          new BoltEditorSkinVariation(BoltEditorGUI.ColorInt(104, 255, 0), BoltEditorGUI.ColorInt(255, 227, 0))
      ),

      new BoltEditorSkin("Sweden", 2, 
          new BoltEditorSkinVariation(BoltEditorGUI.ColorInt(146, 159, 225), BoltEditorGUI.ColorInt(255, 237, 53)),
          new BoltEditorSkinVariation(BoltEditorGUI.ColorInt(209, 193, 249), BoltEditorGUI.ColorInt(255, 233, 53))
      )
    };
  }
}

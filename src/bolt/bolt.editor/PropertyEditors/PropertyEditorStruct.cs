using UnityEngine;
using System.Linq;
using Bolt.Compiler;
using System;

public class PropertyEditorStruct : PropertyEditor<PropertyTypeStruct> {
  protected override void Edit(bool array) {
    BoltEditorGUI.WithLabel("Object Type", () => {
      PropertyType.StructGuid = BoltEditorGUI.AssetPopup(BoltWindow.Project.Structs.Cast<AssetDefinition>(), PropertyType.StructGuid, new Guid[] { Asset.Guid });
    });
  }
}

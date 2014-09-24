//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEditor;

//[CustomEditor(typeof(BoltBinaryAsset))]
//public partial class BoltBinaryAssetEditor : Editor {
//  public override void OnInspectorGUI() {
//    BoltBinaryAsset asset = (BoltBinaryAsset) target;

//    switch (asset.Type) {
//      case BoltBinaryAssetTypes.State:
//        StateEditor(asset);
//        break;

//      case BoltBinaryAssetTypes.Struct:
//        StructEditor(asset);
//        break;

//      case BoltBinaryAssetTypes.Event:
//      case BoltBinaryAssetTypes.Command:
//        throw new NotImplementedException();
//    }
//  }
//}

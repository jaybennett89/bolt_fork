using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler { 
  /// <summary>
  /// Asset Folder
  /// </summary>
  [ProtoContract]
  public class AssetFolder : INamedAsset {
    [ProtoIgnore]
    public bool Deleted;

    [ProtoMember(1)]
    public string Name;

    [ProtoMember(2)]
    public bool Expanded;

    [ProtoMember(3)]
    public AssetFolder[] Folders = new AssetFolder[0];

    [ProtoMember(4)]
    public AssetDefinition[] Assets = new AssetDefinition[0];
     
    [ProtoMember(5)]
    public Guid Guid;

    public IEnumerable<INamedAsset> Children {
      get { return Folders.OrderBy(x => x.Name).Cast<INamedAsset>().Concat(Assets.OrderBy(x => x.Name).Cast<INamedAsset>()); }
    }

    public INamedAsset FindFirstChild() {
      return Children.FirstOrDefault();
    }

    public AssetFolder FindParentFolder(INamedAsset asset) {
      if (Children.Contains(asset)) {
        return this;
      }

      foreach (AssetFolder f in Folders) {
        AssetFolder parent = f.FindParentFolder(asset);

        if (parent != null) {
          return parent;
        }
      }

      return null;
    }

    public INamedAsset FindPrevSibling(INamedAsset asset) {
      return FindSibling(asset, true);
    }

    public INamedAsset FindNextSibling(INamedAsset asset) {
      return FindSibling(asset, false);
    }

    INamedAsset FindSibling(INamedAsset asset, bool prev) {
      if (Children.Contains(asset)) {
        var array = Children.ToArray();
        var index = Array.IndexOf(array, asset);

        if (prev) {
          if (index == 0) {
            return null;
          }

          return array[index - 1];
        }
        else {
          if (index + 1 == array.Length) {
            return null;
          }

          return array[index + 1];
        }
      }

      foreach (AssetFolder f in Folders) {
        INamedAsset sibling = f.FindSibling(asset, prev);

        if (sibling != null) {
          return sibling;
        }
      }

      return null;
    }

    public IEnumerable<AssetDefinition> AssetsAll {
      get { return Assets.Concat(Folders.SelectMany(x => x.AssetsAll)); }
    }

    string INamedAsset.GetName() {
      return Name;
    }
  }

}

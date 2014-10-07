using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public abstract class AssetDecorator {
    public uint TypeId;
    public CodeGenerator Generator;
    public AssetDefinition Definition;

    public abstract Guid Guid {
      get;
    }

  }

  public abstract class AssetDecorator<T> : AssetDecorator where T : AssetDefinition {
    public new T Definition {
      get { return (T)base.Definition; }
      set { base.Definition = value; }
    }

    public sealed override Guid Guid {
      get { return Definition.Guid; }
    }

    public string Name {
      get { return Path.GetFileNameWithoutExtension(Definition.Name); }
    }
  }
}

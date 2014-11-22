using System;
using System.Collections.Generic;

namespace Bolt.Compiler {
  public abstract class AssetDecorator {
    public uint TypeId;

    public int CountStorage;
    public int CountObjects;
    public int CountProperties;

    public CodeGenerator Generator;
    public AssetDefinition Definition;

    public virtual Guid Guid {
      get { return Definition.Guid; }
    }

    public virtual string Name {
      get { return Definition.Name; }
    }

    public virtual string NameMeta {
      get { return Definition.Name; }
    }

    public virtual string BaseClass {
      get { return "Bolt.NetworkObj"; }
    }

    public virtual string BaseClassMeta {
      get { return BaseClass + "_Meta"; }
    }

    public virtual bool EmitInterface {
      get { return false; }
    }

    public virtual bool EmitPropertyChanged {
      get { return true; }
    }

    public abstract string FactoryInterface { get; }
    public abstract List<PropertyDecorator> Properties { get; set; }
  }

  public abstract class AssetDecorator<T> : AssetDecorator where T : AssetDefinition {
    public new T Definition {
      get { return (T)base.Definition; }
      set { base.Definition = value; }
    }
  }
}

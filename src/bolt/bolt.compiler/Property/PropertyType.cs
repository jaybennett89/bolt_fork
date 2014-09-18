using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace bolt.compiler {
  [ProtoContract]
  [ProtoInclude(100, typeof(FloatPropertyType))]
  public abstract class PropertyType {
    [ProtoIgnore]
    public abstract Guid TypeGuid { get; }

    [ProtoIgnore]
    public abstract IEnumerable<Type> AssetTypes { get; }
  }

  [ProtoContract]
  public class FloatPropertyType : PropertyType {
    public override Guid TypeGuid {
      get { return new Guid("31b1414e-bae2-405d-b653-3d36f6968653"); }
    }

    public override IEnumerable<Type> AssetTypes {
      get {
        yield return typeof(EventDefinition);
        yield return typeof(StateDefinition);
        yield return typeof(CommandDefinition);
      }
    }
  }
}

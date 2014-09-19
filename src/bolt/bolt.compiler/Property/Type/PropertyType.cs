using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace bolt.compiler {
  [ProtoContract]
  [ProtoInclude(100, typeof(PropertyTypeFloat))]
  [ProtoInclude(200, typeof(PropertyTypeState))]
  [ProtoInclude(300, typeof(PropertyTypeArray))]
  public abstract class PropertyType {
    [ProtoIgnore]
    public Context Context;

    [ProtoIgnore]
    public abstract int ByteSize { get; }

    [ProtoIgnore]
    public abstract bool MecanimUsable { get; }

    [ProtoIgnore]
    public abstract IEnumerable<Type> AssetTypes { get; }

    public virtual void CalculateObjectCount(StateDefinition state) {

    }
  }
}

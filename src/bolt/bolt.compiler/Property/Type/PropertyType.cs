using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace bolt.compiler {
  [ProtoContract]
  [ProtoInclude(100, typeof(PropertyTypeFloat))]
  [ProtoInclude(200, typeof(PropertyTypeObject))]
  [ProtoInclude(300, typeof(PropertyTypeArray))]
  [ProtoInclude(400, typeof(PropertyTypeVector))]
  [ProtoInclude(500, typeof(PropertyTypeString))]
  [ProtoInclude(600, typeof(PropertyTypeTrigger))]
  public abstract class PropertyType {
    [ProtoIgnore]
    public Context Context;

    [ProtoIgnore]
    public abstract int ByteSize { get; }

    [ProtoIgnore]
    public virtual bool MecanimUsable { get { return false; } }

    [ProtoIgnore]
    public virtual bool IsValue { get { return true; } }

    [ProtoIgnore]
    public virtual IEnumerable<Type> AssetTypes {
      get {
        yield return typeof(EventDefinition);
        yield return typeof(StateDefinition);
        yield return typeof(ObjectDefinition);
        yield return typeof(CommandDefinition);
      }
    }

    public virtual void CalculateObjectCount(StateDefinition state) {

    }
  }
}

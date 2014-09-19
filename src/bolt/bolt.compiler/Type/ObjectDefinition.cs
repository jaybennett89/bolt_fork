using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bolt.compiler {
  public struct ObjectDefinitionCompilationData {

  }

  [ProtoContract]
  public class ObjectDefinition : AssetDefinition {
    [ProtoMember(50)]
    public List<PropertyDefinition> Properties = new List<PropertyDefinition>();

    [ProtoIgnore]
    public ObjectDefinitionCompilationData CompilationDataObject;

    public override IEnumerable<PropertyDefinition> DefinedProperties {
      get { return Properties; }
    }
  }
}

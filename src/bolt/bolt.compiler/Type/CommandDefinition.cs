using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace bolt.compiler {
  public struct CommandDefinitionCompilationData {

  }

  [ProtoContract]
  public class CommandDefinition : AssetDefinition {
    [ProtoMember(50)]
    public List<PropertyDefinition> Input = new List<PropertyDefinition>();

    [ProtoMember(51)]
    public List<PropertyDefinition> Result = new List<PropertyDefinition>();

    [ProtoIgnore]
    public CommandDefinitionCompilationData CompilationDataCommand;

    public override IEnumerable<PropertyDefinition> DefinedProperties {
      get {
        foreach (var p in Input) { yield return p; }
        foreach (var p in Result) { yield return p; }
      }
    }
  }
}

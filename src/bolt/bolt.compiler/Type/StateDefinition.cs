using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace bolt.compiler {
  public struct StateDefinitionCompilationData {
    public StateDefinition State;
    public List<PropertyDefinition> Properties;

    public int ByteCount {
      get {

      }
    }

    public int ObjectCount {
      get {

      }
    }
  }

  [ProtoContract]
  public class StateDefinition : AssetDefinition {
    [ProtoMember(50)]
    public List<PropertyDefinition> Properties = new List<PropertyDefinition>();

    [ProtoMember(53)]
    public bool IsAbstract;

    [ProtoMember(52)]
    public Guid ParentGuid;

    [ProtoIgnore]
    public StateDefinitionCompilationData CompilationDataState;

    public bool HasParent {
      get { return ParentGuid != Guid.Empty; }
    }

    public IEnumerable<StateDefinition> AllParentStates {
      get {
        if (HasParent) {
          var parent = Context.FindState(ParentGuid);

          foreach (StateDefinition def in parent.AllParentStates) {
            yield return def;
          }

          yield return parent;
        }

        yield break;
      }
    }

    public override IEnumerable<PropertyDefinition> DefinedProperties {
      get { return Properties; }
    }
  }
}

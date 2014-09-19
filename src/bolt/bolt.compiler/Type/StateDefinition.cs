using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace bolt.compiler {
  public struct StateDefinitionCompilationData {
    public int ByteCount;
    public int ObjectCount;
  }

  [ProtoContract]
  public class StateDefinition : AssetDefinition {
    [ProtoMember(50)]
    public List<PropertyDefinition> Properties = new List<PropertyDefinition>();

    [ProtoMember(51)]
    public bool SubState;

    [ProtoMember(52)]
    public Guid ParentAssetGuid;

    [ProtoMember(53)]
    public bool IsAbstract;

    [ProtoIgnore]
    public StateDefinitionCompilationData CompilationDataState;

    public bool IsRoot {
      get { return !SubState; }
    }

    public bool IsParent {
      get { return ParentAssetGuid == Guid.Empty; }
    }

    public StateDefinition ParentState {
      get {
        if (IsParent) {
          return null;
        }

        return Context.FindState(ParentAssetGuid);
      }
    }

    public IEnumerable<StateDefinition> ChildStates {
      get { return Context.States.Where(x => x.ParentAssetGuid == this.AssetGuid); }
    }

    public IEnumerable<PropertyDefinition> AllProperties {
      get {
        if (!IsParent) {
          foreach (PropertyDefinition def in ParentState.AllProperties) {
            yield return def;
          }
        }

        foreach (PropertyDefinition def in Properties) {
          yield return def;
        }
      }
    }

    public override IEnumerable<PropertyDefinition> DefinedProperties {
      get { return Properties; }
    }
  }
}

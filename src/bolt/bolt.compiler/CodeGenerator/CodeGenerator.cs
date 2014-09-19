using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using ProtoBuf;

namespace bolt.compiler {
  public class CodeGenerator {
    public Context Context;

    public CodeGenerator(string file) {

    }

    public void Run() {
      // resets all data on the context/assets
      ResetCompilationData();

      // assign class id to each asset
      AssignClassIds();

      // first up is to compile the states
      CompileStates();

      // clear out all compilation data
      ResetCompilationData();
    }

    void ResetCompilationData() {
      Context.CompilationData = new ContextCompilationData();
      Context.CompilationData.Namespace = new CodeNamespace();
      Context.CompilationData.CompileUnit = new CodeCompileUnit();
      Context.CompilationData.CompileUnit.Namespaces.Add(Context.CompilationData.Namespace);

      foreach (StateDefinition a in Context.States) {
        a.CompilationDataAsset = new AssetDefinitionCompilationData();
        a.CompilationDataState = new StateDefinitionCompilationData();
        a.CompilationDataState.State = a;
      }

      foreach (ObjectDefinition a in Context.Objects) {
        a.CompilationDataAsset = new AssetDefinitionCompilationData();
        a.CompilationDataObject = new ObjectDefinitionCompilationData();
      }

      foreach (EventDefinition a in Context.Events) {
        a.CompilationDataAsset = new AssetDefinitionCompilationData();
        a.CompilationDataEvent = new EventDefinitionCompilationData();
      }

      foreach (CommandDefinition a in Context.Commands) {
        a.CompilationDataAsset = new AssetDefinitionCompilationData();
        a.CompilationDataCommand = new CommandDefinitionCompilationData();
      }
    }

    void AssignClassIds() {
      uint classIdCounter = 0;

      foreach (AssetDefinition a in Context.Assets) {
        a.CompilationDataAsset.ClassId = ++classIdCounter;
      }
    }

    void CompileStates() {
      // flatten properties into each state
      FlattenPropertyLists();

      // step through properties and assign index numbers and bits
      AssignIndexesAndBits();
    }

    void FlattenPropertyLists() {
      foreach (StateDefinition s in Context.States) {
        // this will contain all of our properties
        s.CompilationDataState.Properties = new List<PropertyDefinition>();

        // flatten all parent properties into this one
        foreach (StateDefinition parent in s.AllParentStates) {
          CloneProperties(s, parent);
        }

        // clone my own properties last
        CloneProperties(s, s);
      }
    }

    void CloneProperties(StateDefinition into, StateDefinition from) {
      foreach (PropertyDefinition p in from.DefinedProperties) {
        if (p.Enabled) {
          PropertyDefinition clone;

          clone = Serializer.DeepClone(p);
          clone.CompilationData = new PropertyDefinitionCompilationData();
          clone.CompilationData.DefiningAsset = from;

          into.CompilationDataState.Properties.Add(clone);
        }
      }
    }

    void AssignIndexesAndBits() {
      foreach (StateDefinition s in Context.States) {
        int bit = 0;

        for (int i = 0; i < s.CompilationDataState.Properties.Count; ++i) {
          PropertyDefinition p;

          p = s.CompilationDataState.Properties[i];
          p.CompilationData.Index = i;

          if (p.Replicated && p.PropertyType.IsValue && p.StateAssetSettings.ReplicationCondition == ReplicationConditions.ValueChanged) {
            p.CompilationData.Bit = bit++;
          }
          else {
            p.CompilationData.Bit = int.MinValue;
          }
        }
      }
    }
  }
}

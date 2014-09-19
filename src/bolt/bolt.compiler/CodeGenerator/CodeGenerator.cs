using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;

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
    }

    void ResetCompilationData() {
      Context.CompilationData = new ContextCompilationData();
      Context.CompilationData.Namespace = new CodeNamespace();
      Context.CompilationData.CompileUnit = new CodeCompileUnit();
      Context.CompilationData.CompileUnit.Namespaces.Add(Context.CompilationData.Namespace);

      foreach (StateDefinition asset in Context.States) {
        asset.CompilationDataAsset = new AssetDefinitionCompilationData();
        asset.CompilationDataState = new StateDefinitionCompilationData();
      }

      foreach (EventDefinition asset in Context.Events) {
        asset.CompilationDataAsset = new AssetDefinitionCompilationData();
        asset.CompilationDataEvent = new EventDefinitionCompilationData();
      }

      foreach (CommandDefinition asset in Context.Commands) {
        asset.CompilationDataAsset = new AssetDefinitionCompilationData();
        asset.CompilationDataCommand = new CommandDefinitionCompilationData();
      }
    }

    void AssignClassIds() {
      uint classIdCounter = 0;

      foreach (AssetDefinition asset in Context.Assets) {
        asset.CompilationDataAsset.ClassId = ++classIdCounter;
      }
    }

    void CompileStates() {
      foreach (StateDefinition state in Context.States) {

      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.CodeDom;

namespace Bolt.Compiler {
  public struct ContextCompilationData {
    public CodeNamespace Namespace;
    public CodeCompileUnit CompileUnit;
  }

  public class Context {
    List<EventDefinition> events = new List<EventDefinition>();
    List<StateDefinition> states = new List<StateDefinition>();
    List<StructDefinition> structs = new List<StructDefinition>();
    List<CommandDefinition> commands = new List<CommandDefinition>();

    public ContextCompilationData CompilationData;

    public IEnumerable<EventDefinition> Events {
      get { return events; }
    }

    public IEnumerable<StateDefinition> States {
      get { return states; }
    }

    public IEnumerable<StructDefinition> Structs {
      get { return structs; }
    }

    public IEnumerable<CommandDefinition> Commands {
      get { return commands; }
    }

    public IEnumerable<AssetDefinition> Assets {
      get {
        foreach (var a in states) { yield return a; }
        foreach (var a in events) { yield return a; }
        foreach (var a in structs) { yield return a; }
        foreach (var a in commands) { yield return a; }
      }
    }

    public void Add(AssetDefinition asset) {
      asset.Context = this;

      if (asset is EventDefinition) {
        events.Add((EventDefinition)asset);
      }

      if (asset is StateDefinition) {
        states.Add((StateDefinition)asset);
      }

      if (asset is StructDefinition) {
        structs.Add((StructDefinition)asset);
      }

      if (asset is CommandDefinition) {
        commands.Add((CommandDefinition)asset);
      }
    }

    public StateDefinition FindState(Guid guid) {
      return states.First(x => x.Guid == guid);
    }

    public StructDefinition FindStruct(Guid guid) {
      return structs.First(x => x.Guid == guid);
    }

    public EventDefinition FindEvent(Guid guid) {
      return events.First(x => x.Guid == guid);
    }

    public CommandDefinition FindCommand(Guid guid) {
      return commands.First(x => x.Guid == guid);
    }

    public void GenerateCode(string file) {
      CodeGenerator cg;

      cg = new CodeGenerator();
      cg.Run(this, file);
    }
  }
}

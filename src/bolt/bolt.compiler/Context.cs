using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.CodeDom;

namespace bolt.compiler {
  public struct ContextCompilationData {
    public CodeNamespace Namespace;
    public CodeCompileUnit CompileUnit;
  }

  public class Context {
    List<EventDefinition> events = new List<EventDefinition>();
    List<StateDefinition> states = new List<StateDefinition>();
    List<ObjectDefinition> objects = new List<ObjectDefinition>();
    List<CommandDefinition> commands = new List<CommandDefinition>();

    public ContextCompilationData CompilationData;

    public IEnumerable<EventDefinition> Events {
      get { return events; }
    }

    public IEnumerable<StateDefinition> States {
      get { return states; }
    }

    public IEnumerable<ObjectDefinition> Objects {
      get { return objects; }
    }

    public IEnumerable<CommandDefinition> Commands {
      get { return commands; }
    }

    public IEnumerable<AssetDefinition> Assets {
      get {
        foreach (var a in states) { yield return a; }
        foreach (var a in events) { yield return a; }
        foreach (var a in objects) { yield return a; }
        foreach (var a in commands) { yield return a; }
      }
    }

    public void Add(AssetDefinition asset) {
      asset.Context = this;

      // assign context to all properties also
      foreach (PropertyDefinition property in asset.DefinedProperties) {
        property.Context = this;
        property.PropertyType.Context = this;
      }

      if (asset is EventDefinition) {
        events.Add((EventDefinition)asset);
      }

      if (asset is StateDefinition) {
        states.Add((StateDefinition)asset);
      }

      if (asset is ObjectDefinition) {
        objects.Add((ObjectDefinition)asset);
      }

      if (asset is CommandDefinition) {
        commands.Add((CommandDefinition)asset);
      }
    }

    public StateDefinition FindState(Guid guid) {
      return states.First(x => x.AssetGuid == guid);
    }

    public ObjectDefinition FindObject(Guid guid) {
      return objects.First(x => x.AssetGuid == guid);
    }

    public EventDefinition FindEvent(Guid guid) {
      return events.First(x => x.AssetGuid == guid);
    }

    public CommandDefinition FindCommand(Guid guid) {
      return commands.First(x => x.AssetGuid == guid);
    }

    public void GenerateCode(string file) {
      CodeGenerator cg;

      cg = new CodeGenerator(file);
      cg.Context = this;
      cg.Run();
    }
  }
}

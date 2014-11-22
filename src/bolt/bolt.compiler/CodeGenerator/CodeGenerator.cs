using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using ProtoBuf;
using System.IO;
using System.CodeDom.Compiler;
using System.Reflection;

namespace Bolt.Compiler {
  public partial class CodeGenerator {
    Project Project;

    public List<StateDecorator> States;
    public List<ObjectDecorator> Structs;
    public List<EventDecorator> Events;
    public List<CommandDecorator> Commands;
    public bool AllowStatePropertySetters = false;

    public CodeNamespace CodeNamespace;
    public CodeNamespace CodeNamespaceBolt;
    public CodeNamespace CodeNamespaceBoltInternal;
    public CodeCompileUnit CodeCompileUnit;

    public IEnumerable<FilterDefinition> Filters {
      get { return Project.EnabledFilters; }
    }

    public CodeGenerator() {
      States = new List<StateDecorator>();
      Structs = new List<ObjectDecorator>();
      Events = new List<EventDecorator>();
      Commands = new List<CommandDecorator>();
    }

    public void Run(Project context, string file) {
      Project = context;

      CodeNamespace = new CodeNamespace();
      CodeNamespaceBolt = new CodeNamespace("Bolt");
      CodeNamespaceBoltInternal = new CodeNamespace("BoltInternal");

      CodeCompileUnit = new CodeCompileUnit();
      CodeCompileUnit.Namespaces.Add(CodeNamespace);
      CodeCompileUnit.Namespaces.Add(CodeNamespaceBolt);
      CodeCompileUnit.Namespaces.Add(CodeNamespaceBoltInternal);

      // resets all data on the context/assets
      DecorateDefinitions();

      // create automagical asset objects
      CreateCommandObjects();

      // sort all states by inheritance
      SortStatesByHierarchy();

      // flatten state hierarchy
      DecorateProperties();

      // go through all of the our assets and set them up for compilation
      CreateCompilationModel();

      // assign class id to each asset
      AssignTypeIds();

      // first up is to compile the states
      EmitCodeObjectModel();

      // generate code
      GenerateSourceCode(file);
    }

    public FilterDefinition FindFilter(int index) {
      return Filters.First(x => x.Index == index);
    }

    public StateDecorator FindState(Guid guid) {
      return States.First(x => x.Definition.Guid == guid);
    }

    public ObjectDecorator FindStruct(Guid guid) {
      return Structs.First(x => x.Definition.Guid == guid);
    }

    public bool HasState(Guid guid) {
      return States.Any(x => x.Guid == guid);
    }

    void DecorateDefinitions() {
      foreach (StateDefinition def in Project.States) {
        StateDecorator decorator;

        decorator = new StateDecorator();
        decorator.Definition = def;
        decorator.Generator = this;

        States.Add(decorator);
      }

      foreach (StructDefinition def in Project.Structs) {
        ObjectDecorator decorator;

        decorator = new ObjectDecorator();
        decorator.Definition = def;
        decorator.Generator = this;

        Structs.Add(decorator);
      }

      foreach (EventDefinition def in Project.Events) {
        EventDecorator decorator;

        decorator = new EventDecorator();
        decorator.Definition = def;
        decorator.Generator = this;

        Events.Add(decorator);
      }

      foreach (CommandDefinition def in Project.Commands) {
        CommandDecorator decorator;

        decorator = new CommandDecorator();
        decorator.Definition = def;
        decorator.Generator = this;

        Commands.Add(decorator);
      }
    }

    ObjectDecorator CreateObjectDefintion(AssetDecorator asset, string suffix) {
      StructDefinition def;

      def = new StructDefinition();
      def.Deleted = false;
      def.Enabled = true;
      def.Guid = Guid.NewGuid();
      def.Name = asset.Name + suffix;
      def.Project = asset.Definition.Project;

      ObjectDecorator decorator;

      decorator = new ObjectDecorator();
      decorator.Definition = def;
      decorator.Generator = this;

      return decorator;
    }

    PropertyDefinition CreateObjectProperty(string name, ObjectDecorator propertyType)
    {
      PropertyDefinition def;
      
      def = new PropertyDefinition();
      def.Name = name;
      def.Replicated = true;
      def.Enabled = true;
      def.Deleted = false;
      def.PropertyType = new PropertyTypeStruct()
      {
        Context = Project,
        StructGuid = Guid.NewGuid()
      };

      return def;
    }

    void CreateCommandObjects()
    {
      foreach (CommandDecorator command in Commands)
      {
        ObjectDecorator input;
        input = CreateObjectDefintion(command, "Input");
        input.Definition.Properties = command.Definition.Input;

        ObjectDecorator result;
        result = CreateObjectDefintion(command, "Result");
        result.Definition.Properties = command.Definition.Result;

        Structs.Add(input);
        Structs.Add(result);

        command.Properties = new List<PropertyDecorator>()
        {
          PropertyDecorator.Decorate(CreateObjectProperty("Input", input), command),
          PropertyDecorator.Decorate(CreateObjectProperty("Result", result), command)
        };
      }
    }

    void AssignTypeIds() {
      uint typeId = 0;

      foreach (CommandDecorator decorator in Commands) {
        decorator.TypeId = ++typeId;
      }

      foreach (EventDecorator decorator in Events) {
        decorator.TypeId = ++typeId;
      }

      foreach (StateDecorator decorator in States) {
        decorator.TypeId = ++typeId;
      }

      foreach (ObjectDecorator decorator in Structs) {
        decorator.TypeId = ++typeId;
      }
    }

    void SortStatesByHierarchy() {
      // sort states by inheritance
      States.Sort((a, b) => {
        // if 'a' is a parent of 'b', then 'a' is smaller
        if (b.ParentList.Any(x => x.Definition.Guid == a.Definition.Guid)) {
          return -1;
        }

        // if 'b' is a parent 'a' of, then 'a' is larger
        if (a.ParentList.Any(x => x.Definition.Guid == b.Definition.Guid)) {
          return 1;
        }

        // neither, so order on guid
        return a.Definition.Guid.CompareTo(b.Definition.Guid);
      });
    }

    void EmitCodeObjectModel() {
      foreach (ObjectDecorator s in Structs) {
        AssetCodeEmitter emitter;
        emitter = new AssetCodeEmitter();
        emitter.Decorator = s;
        emitter.Emit();
      }

      foreach (StateDecorator s in States) {
        StateCodeEmitter emitter;
        emitter = new StateCodeEmitter();
        emitter.Decorator = s;
        emitter.EmitInterface();
        emitter.EmitObject();
        emitter.EmitFactory();
      }

      foreach (EventDecorator d in Events) {
        EventCodeEmitter emitter;
        emitter = new EventCodeEmitter();
        emitter.Decorator = d;
        emitter.EmitTypes();
      }

      foreach (CommandDecorator d in Commands) {
        CommandCodeEmitter emitter;
        emitter = new CommandCodeEmitter();
        emitter.Decorator = d;
        emitter.EmitTypes();
      }

      EmitEventBaseClasses();
      EmitStateTypeIdLookup();

      //EmitFilters();
    }

    void EmitStateTypeIdLookup() {
      CodeTypeDeclaration type;

      type = new CodeTypeDeclaration("StateSerializerTypeIds");
      type.TypeAttributes = TypeAttributes.Public | TypeAttributes.Abstract;

      foreach (StateDecorator s in States) {
        if (s.Definition.IsAbstract) {
          continue;
        }

        var field = type.DeclareField("Bolt.UniqueId", s.InterfaceName);
        field.Attributes = MemberAttributes.Public | MemberAttributes.Static;
        field.InitExpression = new CodeSnippetExpression(string.Format("new Bolt.UniqueId({0})", s.Guid.ToByteArray().Join(", ")));
      }

      CodeNamespaceBoltInternal.Types.Add(type);
    }

    void EmitEventBaseClasses() {
      var globalListener = CodeNamespaceBolt.DeclareClass("GlobalEventListener");
      var entityListener = CodeNamespaceBolt.DeclareClass("EntityEventListener");
      var entityListenerGeneric = CodeNamespaceBolt.DeclareClass("EntityEventListener<TState>");

      globalListener.BaseTypes.Add("BoltInternal.GlobalEventListenerBase");
      entityListener.BaseTypes.Add("BoltInternal.EntityEventListenerBase");
      entityListenerGeneric.BaseTypes.Add("BoltInternal.EntityEventListenerBase<TState>");

      foreach (EventDecorator d in Events) {
        EmitEventMethodOverride(globalListener, d);
        EmitEventMethodOverride(entityListener, d);
        EmitEventMethodOverride(entityListenerGeneric, d);
      }
    }

    void EmitEventMethodOverride(CodeTypeDeclaration type, EventDecorator d) {
      type.BaseTypes.Add(d.ListenerName);

      type.DeclareMethod(typeof(void).FullName, "OnEvent", method => {
        method.DeclareParameter(d.Name, "evnt");
      }).Attributes = MemberAttributes.Public;
    }

    void EmitFilters() {
      CodeTypeDeclaration type = DeclareStruct("BoltFilters");

      foreach (FilterDefinition filter in Filters) {
        type.DeclareProperty("Bolt.Filter", filter.Name, get => {
          get.Expr("return new Bolt.Filter({0})", 1 << filter.Index);
        }).Attributes |= MemberAttributes.Static;
      }
    }

    void DecorateProperties() {
      // Structs
      foreach (ObjectDecorator s in Structs) {
        s.Properties = PropertyDecorator.Decorate(s.Definition.Properties, s);
      }

      // Events
      foreach (EventDecorator d in Events) {
        d.Properties = PropertyDecorator.Decorate(d.Definition.Properties, d);
      }

      // Events
      foreach (CommandDecorator d in Commands) {
        d.InputProperties = PropertyDecorator.Decorate(d.Definition.Input, d);
        d.ResultProperties = PropertyDecorator.Decorate(d.Definition.Result, d);
      }

      // States
      foreach (StateDecorator s in States) {
        // decorate and clone all parent properties, in order
        foreach (StateDecorator parent in s.ParentList) {
          s.Properties.AddRange(PropertyDecorator.Decorate(parent.Definition.Properties, parent));
        }

        // decorate own properties
        s.Properties.AddRange(PropertyDecorator.Decorate(s.Definition.Properties, s));

        // setup root struct definition
        StructDefinition rootDef = new StructDefinition();
        rootDef.Enabled = s.Definition.Enabled;
        rootDef.Guid = s.Definition.Guid;
        rootDef.Comment = s.Definition.Comment;
        rootDef.Name = s.Definition.Name;
        rootDef.Properties = new List<PropertyDefinition>(s.Definition.Properties);

        // setup root struct decorator
        ObjectDecorator rootDec = new ObjectDecorator();
        rootDec.Definition = rootDef;
        rootDec.TypeId = s.TypeId;
        rootDec.Generator = s.Generator;
        rootDec.Properties = s.Properties;
        rootDec.SourceState = s;

        // and in our struct list
        Structs.Add(rootDec);
      }
    }

    void OrderStructsByDependancies() {
      List<ObjectDecorator> structs = Structs.Where(x => x.Dependencies.Count() == 0).ToList();
      List<ObjectDecorator> structsWithDeps = Structs.Where(x => x.Dependencies.Count() != 0).ToList();

      while (structsWithDeps.Count > 0) {
        for (int i = 0; i < structsWithDeps.Count; ++i) {
          ObjectDecorator s = structsWithDeps[i];

          if (s.Dependencies.All(x => structs.Any(y => y.Guid == x.Guid))) {
            // remove from deps list
            structsWithDeps.RemoveAt(i);

            // insert into structs list
            structs.Add(s);

            // decrement index counter
            i -= 1;
          }
        }
      }

      Structs = structs;
    }

    void CreateCompilationModel() {
      // order all structs by their dependancies
      OrderStructsByDependancies();

      // sort, index and assign bits for properties
      for (int i = 0; i < Structs.Count; ++i) {
        ObjectDecorator decorator;

        decorator = Structs[i];
        decorator.CountObjects = 1;

        for (int p = 0; p < decorator.Properties.Count; ++p) {
          if (decorator.Properties[p].RequiredObjects == 0) {
            decorator.Properties[p].OffsetObjects = 0;
          }
          else {
            decorator.Properties[p].OffsetObjects = decorator.CountObjects;
          }

          decorator.Properties[p].OffsetStorage = decorator.CountStorage;
          decorator.Properties[p].OffsetProperties = decorator.CountProperties;

          decorator.CountObjects += decorator.Properties[p].RequiredObjects;
          decorator.CountStorage += decorator.Properties[p].RequiredStorage;
          decorator.CountProperties += decorator.Properties[p].RequiredProperties;
        }

      }
    }

    void GenerateUsingStatements(StringBuilder sb) {
      sb.AppendLine("using System;");
      sb.AppendLine("using System.Collections.Generic;");
      sb.AppendLine();
      sb.AppendLine("using UE = UnityEngine;");
      sb.AppendLine("using Encoding = System.Text.Encoding;");
      sb.AppendLine();
    }

    void GenerateSourceCode(string file) {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      GenerateUsingStatements(sb);

      CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
      CodeGeneratorOptions options = new CodeGeneratorOptions();

      options.BlankLinesBetweenMembers = false;
      options.IndentString = "  ";

      provider.GenerateCodeFromCompileUnit(CodeCompileUnit, sw, options);

      sw.Flush();
      sw.Dispose();

      File.WriteAllText(file, sb.ToString());
    }

  }
}

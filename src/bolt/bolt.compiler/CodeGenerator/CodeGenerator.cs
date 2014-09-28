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
    Project Context;

    public List<StateDecorator> States;
    public List<StructDecorator> Structs;

    public CodeNamespace CodeNamespace;
    public CodeCompileUnit CodeCompileUnit;

    public IEnumerable<FilterDefinition> Filters {
      get { return Context.EnabledFilters; }
    }

    public CodeGenerator() {
      States = new List<StateDecorator>();
      Structs = new List<StructDecorator>();
    }

    public void Run(Project context, string file) {
      Context = context;

      CodeNamespace = new CodeNamespace();
      CodeCompileUnit = new CodeCompileUnit();
      CodeCompileUnit.Namespaces.Add(CodeNamespace);

      // resets all data on the context/assets
      DecorateDefinitions();

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

    public StructDecorator FindStruct(Guid guid) {
      return Structs.First(x => x.Definition.Guid == guid);
    }

    public bool HasState(Guid guid) {
      return States.Any(x => x.Guid == guid);
    }

    void DecorateDefinitions() {
      foreach (StateDefinition def in Context.States) {
        StateDecorator decorator;

        decorator = new StateDecorator();
        decorator.Definition = def;
        decorator.Generator = this;

        States.Add(decorator);
      }

      foreach (StructDefinition def in Context.Structs) {
        StructDecorator decorator;

        decorator = new StructDecorator();
        decorator.Definition = def;
        decorator.Generator = this;

        Structs.Add(decorator);
      }
    }

    void AssignTypeIds() {
      uint typeId = 1;

      foreach (StateDecorator decorator in States) {
        decorator.TypeId = ++typeId;
      }

      foreach (StructDecorator decorator in Structs) {
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
      foreach (StateDecorator s in States) {
        StateCodeEmitter emitter;
        emitter = new StateCodeEmitter();
        emitter.Decorator = s;
        emitter.EmitInterface();
      }

      foreach (StructDecorator s in Structs) {
        StructCodeEmitter emitter;
        emitter = new StructCodeEmitter();
        emitter.Decorator = s;
        emitter.EmitStruct();
        emitter.EmitArray();
      }

      foreach (StructDecorator s in Structs) {
        StructCodeEmitter emitter;
        emitter = new StructCodeEmitter();
        emitter.Decorator = s;
        emitter.EmitModifierInterface();
      }

      foreach (StructDecorator s in Structs) {
        StructCodeEmitter emitter;
        emitter = new StructCodeEmitter();
        emitter.Decorator = s;
        emitter.EmitModifier();
      }

      foreach (StateDecorator s in States) {
        StateCodeEmitter emitter;
        emitter = new StateCodeEmitter();
        emitter.Decorator = s;
        emitter.EmitImplementationClass();
      }

      foreach (StateDecorator s in States) {
        StateCodeEmitter emitter;
        emitter = new StateCodeEmitter();
        emitter.Decorator = s;
        emitter.EmitFactoryClass();
      }

      EmitFilters();
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
      foreach (StructDecorator s in Structs) {
        // decorate own properties
        s.Properties = PropertyDecorator.Decorate(s.Definition.Properties, s);
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
        StructDecorator rootDec = new StructDecorator();
        rootDec.Definition = rootDef;
        rootDec.TypeId = s.TypeId;
        rootDec.Generator = s.Generator;
        rootDec.Properties = s.Properties;
        rootDec.SourceState = s;

        // store on state decorator
        s.RootStruct = rootDec;

        // and in our struct list
        Structs.Add(rootDec);
      }
    }

    void OrderStructsByDependancies() {
      List<StructDecorator> structs = Structs.Where(x => x.Dependencies.Count() == 0).ToList();
      List<StructDecorator> structsWithDeps = Structs.Where(x => x.Dependencies.Count() != 0).ToList();

      while (structsWithDeps.Count > 0) {
        for (int i = 0; i < structsWithDeps.Count; ++i) {
          StructDecorator s = structsWithDeps[i];

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
      OrderStructsByDependancies();

      // sort, index and assign bits for properties
      for (int i = 0; i < Structs.Count; ++i) {
        StructDecorator decorator = Structs[i];
        // properties are sorted in this order:
        // 1. Value Properties
        // 2. Sub-Structs
        // 3. Sub-Arrays
        decorator.Properties =
          decorator.Properties.Where(x => x.Definition.PropertyType.IsValue)
            .Concat(decorator.Properties.Where(x => x.Definition.PropertyType is PropertyTypeStruct))
            .Concat(decorator.Properties.Where(x => x.Definition.PropertyType is PropertyTypeArray))
            .ToList();
      }

      // calculate sizes for all structs
      for (int i = 0; i < Structs.Count; ++i) {
        StructDecorator decorator;

        decorator = Structs[i];
        decorator.ByteSize = 0;
        decorator.ObjectSize = 0;

        for (int n = 0; n < decorator.Properties.Count; ++n) {
          // copy byte offset to property
          decorator.Properties[n].ByteOffset = decorator.ByteSize;
          decorator.Properties[n].ObjectOffset = decorator.ObjectSize;

          // increment byte offset
          decorator.ByteSize += decorator.Properties[n].ByteSize;
          decorator.ObjectSize += decorator.Properties[n].ObjectSize;
        }

        decorator.FrameSizeCalculated = true;
      }

      // calculate absolute property indexes and filters for all properties
      foreach (var state in States) {
        StateProperty p = new StateProperty();
        p.Filters = -1;
        p.Controller = true;
        p.CallbackPath = "";
        p.CallbackIndices = new int[0];

        state.RootStruct.FindAllProperties(state.AllProperties, p);
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

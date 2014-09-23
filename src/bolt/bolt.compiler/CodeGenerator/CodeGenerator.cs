using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using ProtoBuf;
using System.IO;
using System.CodeDom.Compiler;

namespace Bolt.Compiler {
  public partial class CodeGenerator {
    Context Context;

    public List<StateDecorator> States;
    public List<StructDecorator> Structs;

    public CodeNamespace CodeNamespace;
    public CodeCompileUnit CodeCompileUnit;

    public IEnumerable<PropertyFilterDefinition> Filters {
      get { return Context.Filters; }
    }

    public CodeGenerator() {
      States = new List<StateDecorator>();
      Structs = new List<StructDecorator>();
    }

    public void Run(Context context, string file) {
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

    public PropertyFilterDefinition FindFilter(Guid guid) {
      return Filters.First(x => x.Guid == guid);
    }

    public PropertyFilterDefinition FindFilter(int index) {
      return Filters.First(x => x.Index == index);
    }

    public StateDecorator FindState(Guid guid) {
      return States.First(x => x.Definition.Guid == guid);
    }

    public StructDecorator FindStruct(Guid guid) {
      return Structs.First(x => x.Definition.Guid == guid);
    }

    void DecorateDefinitions() {
      foreach (StateDefinition def in Context.States) {
        if (def.Enabled) {
          StateDecorator decorator;

          decorator = new StateDecorator();
          decorator.Definition = def;
          decorator.Generator = this;

          States.Add(decorator);
        }
      }

      foreach (StructDefinition def in Context.Structs) {
        if (def.Enabled) {
          StructDecorator decorator;

          decorator = new StructDecorator();
          decorator.Definition = def;
          decorator.Generator = this;

          Structs.Add(decorator);
        }
      }
    }

    void AssignTypeIds() {
      uint typeId = 0;

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
      foreach (StructDecorator s in Structs) {
        AssetCodeEmitter emitter;
        emitter = new StructCodeEmitter();
        emitter.Decorator = s;
        emitter.EmitTypes();
      }

      foreach (StateDecorator s in States) {
        AssetCodeEmitter emitter;
        emitter = new StateCodeEmitter();
        emitter.Decorator = s;
        emitter.EmitTypes();
      }

      EmitFilters();
    }

    void EmitFilters() {
      CodeTypeDeclaration type = DeclareStruct("BoltFilters");

      foreach (PropertyFilterDefinition filter in Filters) {
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
        rootDef.AssetPath = s.Definition.AssetPath;
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

        // assign indexes
        for (int n = 0; n < decorator.Properties.Count; ++n) {
          PropertyDecorator p;

          p = decorator.Properties[n];
          p.Index = n;

          if (p.Definition.Replicated && p.Definition.PropertyType.IsValue && p.Definition.StateAssetSettings.Condition == ReplicationConditions.ValueChanged) {
            p.MaskBit = decorator.StructCount++;
          }
          else {
            p.MaskBit = int.MinValue;
          }
        }
      }

      // calculate sizes for all structs
      for (int i = 0; i < Structs.Count; ++i) {
        StructDecorator decorator = Structs[i];
        decorator.FrameSize = 0;

        for (int n = 0; n < decorator.Properties.Count; ++n) {
          // copy byte offset to property
          decorator.Properties[n].ByteOffset = decorator.FrameSize;

          // increment byte offset
          decorator.FrameSize += decorator.Properties[n].ByteSize;
        }

        decorator.ByteSizeCalculated = true;
      }

      // calculate mask counts for all structs
      for (int i = 0; i < Structs.Count; ++i) {
        StructDecorator decorator = Structs[i];
        decorator.StructCount = 1;

        for (int n = 0; n < decorator.Properties.Count; ++n) {
          decorator.StructCount += decorator.Properties[n].StructCount;
        }
      }
    }

    void GenerateUsingStatements(StringBuilder sb) {
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

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
    public List<StructDecorator> Structs;
    public List<EventDecorator> Events;
    public List<CommandDecorator> Commands;

    public CodeNamespace CodeNamespace;
    public CodeNamespace CodeNamespaceBolt;
    public CodeNamespace CodeNamespaceBoltInternal;
    public CodeCompileUnit CodeCompileUnit;

    public IEnumerable<FilterDefinition> Filters {
      get { return Project.EnabledFilters; }
    }

    public CodeGenerator() {
      States = new List<StateDecorator>();
      Structs = new List<StructDecorator>();
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

    public string CreateSmoothingSettings(PropertyDefinition p) {
      var s = p.StateAssetSettings;
      if (s != null) {
        var t = p.PropertyType as PropertyTypeTransform;

        switch (s.SmoothingAlgorithm) {
          case SmoothingAlgorithms.Interpolation:
            return "Bolt.PropertySmoothingSettings.CreateInterpolation()";

          case SmoothingAlgorithms.Extrapolation:
            return string.Format("Bolt.PropertySmoothingSettings.CreateExtrapolation({0}, {1}, {2}f, {3})",
              s.ExtrapolationMaxFrames,
              s.ExtrapolationCorrectionFrames,
              s.ExtrapolationErrorTolerance,
              t == null
                ? string.Format("Bolt.ExtrapolationVelocityModes.{0}", t.ExtrapolationVelocityMode)
                : "default(Bolt.ExtrapolationVelocityModes)"
            );
        }
      }

      return "default(Bolt.PropertySmoothingSettings)";
    }

    public string CreateFloatCompressionExpression(FloatCompression c) {
      return CreateFloatCompressionExpression(c, true);
    }

    public string CreateVectorCompressionExpression(FloatCompression[] axes, AxisSelections selection) {
      if (axes == null) {
        selection = AxisSelections.XYZ;
      }

      List<string> args = new List<string>();
      args.Add(CreateFloatCompressionExpression(axes[Axis.X], (selection & AxisSelections.X) == AxisSelections.X));
      args.Add(CreateFloatCompressionExpression(axes[Axis.Y], (selection & AxisSelections.Y) == AxisSelections.Y));
      args.Add(CreateFloatCompressionExpression(axes[Axis.Z], (selection & AxisSelections.Z) == AxisSelections.Z));
      return string.Format("Bolt.PropertyVectorCompressionSettings.Create({0})", args.Join(", "));
    }

    public string CreateRotationCompressionExpression(FloatCompression[] axes, FloatCompression quaternion, AxisSelections selection) {
      if (axes == null || quaternion == null || selection == AxisSelections.XYZ) {
        return string.Format("Bolt.PropertyQuaternionCompression.Create({0})", CreateFloatCompressionExpression(quaternion));
      }
      else {
        return string.Format("Bolt.PropertyQuaternionCompression.Create({0})", CreateVectorCompressionExpression(axes, selection));
      }
    }

    public string CreateFloatCompressionExpression(FloatCompression c, bool enabled) {
      if (c == null) {
        c = FloatCompression.Default();
      }

      if (enabled) {
        if (c.Enabled) {
          return string.Format("Bolt.PropertyFloatCompressionSettings.Create({0}, {1}f, {2}f, {3}f)", c.BitsRequired, c.Shift, c.Pack, c.Read);
        }
        else {
          return string.Format("Bolt.PropertyFloatCompressionSettings.Create()");
        }
      }
      else {
        return string.Format("default(Bolt.PropertyFloatCompressionSettings)");
      }
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
      foreach (StateDefinition def in Project.States) {
        StateDecorator decorator;

        decorator = new StateDecorator();
        decorator.Definition = def;
        decorator.Generator = this;

        States.Add(decorator);
      }

      foreach (StructDefinition def in Project.Structs) {
        StructDecorator decorator;

        decorator = new StructDecorator();
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
      foreach (StructDecorator s in Structs) {
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
      // Calculate size for events
      for (int i = 0; i < Events.Count; ++i) {
        EventDecorator decorator = Events[i];

        for (int n = 0; n < decorator.Properties.Count; ++n) {
          decorator.Properties[n].ByteOffset = decorator.ByteSize;
          decorator.ByteSize += decorator.Properties[n].ByteSize;
        }
      }

      // Calculate size for commands
      for (int i = 0; i < Commands.Count; ++i) {
        CommandDecorator decorator = Commands[i];

        for (int n = 0; n < decorator.InputProperties.Count; ++n) {
          decorator.InputProperties[n].ByteOffset = decorator.InputByteSize;
          decorator.InputByteSize += decorator.InputProperties[n].ByteSize;
        }

        for (int n = 0; n < decorator.ResultProperties.Count; ++n) {
          decorator.ResultProperties[n].ByteOffset = decorator.ResultByteSize;
          decorator.ResultByteSize += decorator.ResultProperties[n].ByteSize;
        }
      }

      // order all structs by their dependancies
      OrderStructsByDependancies();

      // sort, index and assign bits for properties
      for (int i = 0; i < Structs.Count; ++i) {
        StructDecorator decorator = Structs[i];

        // properties are sorted in this order:
        // - Values
        // - Triggers
        // - Structs
        // - Arrays
        decorator.Properties =
          decorator.Properties.Where(x => x.Definition.PropertyType.IsValue)
            .Concat(decorator.Properties.Where(x => x.Definition.PropertyType is PropertyTypeTrigger))
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
        state.RootStruct.FindAllProperties(state.AllProperties, new StateProperty());

        int offsetBytes = 0;
        int offsetObjects = 0;

        for (int i = 0; i < state.AllProperties.Count; ++i) {
          state.AllProperties[i] = state.AllProperties[i].Combine(offsetBytes, offsetObjects);

          offsetBytes += state.AllProperties[i].Decorator.ByteSize;
          offsetObjects += state.AllProperties[i].Decorator.ObjectSize;
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

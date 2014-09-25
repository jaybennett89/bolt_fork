﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bolt.Compiler {
  [ProtoContract]
  public class Project {
    [ProtoMember(1)]
    public AssetFolder RootFolder = new AssetFolder();

    [ProtoMember(2)]
    public PropertyFilterDefinition[] Filters = new PropertyFilterDefinition[0];

    public IEnumerable<StateDefinition> States {
      get { return RootFolder.AssetsAll.Where(x => x is StateDefinition).Cast<StateDefinition>(); }
    }

    public IEnumerable<StructDefinition> Structs {
      get { return RootFolder.AssetsAll.Where(x => x is StructDefinition).Cast<StructDefinition>(); }
    }

    public bool UseFilters {
      get { return Filters.Count(x => x.Enabled) > 0; }
    }

    public StateDefinition FindState(Guid guid) {
      return States.First(x => x.Guid == guid);
    }

    public StructDefinition FindStruct(Guid guid) {
      return Structs.First(x => x.Guid == guid);
    }

    public void GenerateCode(string file) {
      CodeGenerator cg;

      cg = new CodeGenerator();
      cg.Run(this, file);
    }
  }
}

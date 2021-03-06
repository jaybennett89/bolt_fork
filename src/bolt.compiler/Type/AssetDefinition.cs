﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Bolt.Compiler {
  [ProtoContract]
  public enum SortOrder {
    Manual,
    Name,
    Priority
  }

  [ProtoContract]
  [ProtoInclude(100, typeof(StateDefinition))]
  [ProtoInclude(200, typeof(EventDefinition))]
  [ProtoInclude(300, typeof(ObjectDefinition))]
  [ProtoInclude(400, typeof(CommandDefinition))]
  public abstract class AssetDefinition {
    [ProtoIgnore]
    public bool Deleted;

    [ProtoIgnore]
    public Project Project;

    [ProtoMember(2)]
    public string Name;

    [ProtoMember(1)]
    public Guid Guid;

    [ProtoMember(5)]
    public string Comment;

    [ProtoMember(6)]
    public bool Enabled;

    [ProtoMember(9, OverwriteList = true)]
    public HashSet<string> Groups = new HashSet<string>();

    [ProtoMember(10)]
    public SortOrder SortOrder;

    public abstract IEnumerable<Type> AllowedPropertyTypes {
      get;
    }
  }
}

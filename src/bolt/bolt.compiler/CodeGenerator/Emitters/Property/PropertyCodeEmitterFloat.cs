﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterFloat : PropertyCodeEmitter<PropertyDecoratorFloat> {
    void DeclareProperty(CodeTypeDeclaration type, bool emitSetter) {
      Action<CodeStatementCollection> getter = get => {
        get.Expr("return Blit.ReadF32(data, offset + {0})", Decorator.ByteOffset);
      };

      Action<CodeStatementCollection> setter = set => {
        set.Expr("Blit.PackF32(data, offset + {0}, value)", Decorator.ByteOffset);
      };

      type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, getter, emitSetter ? setter : null);
    }

    public override void EmitShimMembers(CodeTypeDeclaration type) {
      DeclareProperty(type, false);
    }

    public override void EmitModifierMembers(CodeTypeDeclaration type) {
      DeclareProperty(type, true);
    }
  }
}

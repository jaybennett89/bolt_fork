using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterString : PropertyCodeEmitter<PropertyDecoratorString> {

    string Get(object data, object offset) {
      return string.Format("return Bolt.Blit.ReadString({0}, {1}, System.Text.Encoding.{2})", data, offset, Decorator.PropertyType.Encoding);
    }

    string Set(object data, object offset) {
      return string.Format("Bolt.Blit.PackString({0}, {1}, System.Text.Encoding.{2}, value, {3}, {4})",
        data,
        offset,
        Decorator.PropertyType.Encoding,
        Decorator.PropertyType.MaxLength,
        Decorator.PropertyType.EncodingClass.GetMaxByteCount(Decorator.PropertyType.MaxLength)
      );
    }

    void DeclareProperty(CodeTypeDeclaration type, bool emitSetter) {
      var offset = "offsetBytes + " + Decorator.ByteOffset;
      type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, Get("frame.Data", offset), emitSetter ? Set("frame.Data", offset) : null);
    }

    public override void EmitStructMembers(CodeTypeDeclaration type) {
      DeclareProperty(type, false);
    }

    public override void EmitModifierMembers(CodeTypeDeclaration type) {
      DeclareProperty(type, true);
    }

    public override void EmitEventMembers(CodeTypeDeclaration type) {
      var offset = Decorator.ByteOffset;
      type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, Get("Data", offset), Set("Data", offset));
    }

    public override void GetAddSettingsArgument(List<string> settings) {
      settings.Add(string.Format("new Bolt.PropertyStringSettings {{ Encoding = Bolt.StringEncodings.{0} }}", Decorator.PropertyType.Encoding));
    }
  }
}

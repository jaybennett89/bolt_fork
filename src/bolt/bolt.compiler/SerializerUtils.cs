using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public static class SerializerUtils {
    public static byte[] ToByteArray<T>(this T obj) {
      MemoryStream ms = new MemoryStream();
      Serializer.Serialize<T>(ms, obj);
      return ms.ToArray();
    }

    public static T ToObject<T>(this byte[] data) where T : class {
      MemoryStream ms;

      ms = new MemoryStream(data);
      ms.Position = 0;

      return Serializer.Deserialize<T>(ms);
    }
  }
}

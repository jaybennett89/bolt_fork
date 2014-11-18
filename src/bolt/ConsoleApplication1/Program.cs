using Bolt.Compiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1 {
  class Program {
    static void Main(string[] args) {
      var project = SerializerUtils.ToObject<Project>(File.ReadAllBytes(@"C:\Users\Fredrik\Documents\GitHub\bolt\src\bolt.unity.tutorial\Assets\bolt\project.bytes"));
      var cg = new CodeGenerator();
      cg.Run(project, "test.cs");
    }
  }
}

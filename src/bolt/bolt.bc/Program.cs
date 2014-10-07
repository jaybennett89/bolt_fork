using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bolt.Compiler;
using System.IO;

namespace Bolt.Bc {
  class Program {
    static void Main(string[] args) {

      Project project = File.ReadAllBytes(@"C:\Users\Fredrik\Documents\GitHub\bolt\src\bolt.unity.dev\Assets\bolt\project.bytes").ToObject<Project>();
      project.GenerateCode("Test.cs");

    }
  }
}

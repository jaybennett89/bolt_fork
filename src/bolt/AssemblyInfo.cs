using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if BOLT_DLL
[assembly: AssemblyTitle("bolt")]
[assembly: Guid("e24988b8-bcbc-4e93-a24e-8afc569782fe")]
#elif BOLT_EDITOR_DLL
[assembly: AssemblyTitle("bolt.editor")]
[assembly: Guid("6c922342-f87e-4d21-b4dd-ed7a706e9071")]
#elif BOLT_COMPILER_DLL
[assembly: AssemblyTitle("bolt.compiler")]
[assembly: Guid("cacebb0c-ef99-469d-84cc-5af578b6bb44")]
#endif

[assembly: InternalsVisibleTo("bolt.user")]
[assembly: InternalsVisibleTo("bolt.editor")]
[assembly: InternalsVisibleTo("bolt.compiler")]

[assembly: AssemblyDescription("Networking middleware for Unity3D")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Bolt Engine AB")]
[assembly: AssemblyProduct("Bolt")]
[assembly: AssemblyCopyright("Copyright © Fredrik Holmstrom 2012 - 2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("0.4.0.15")]

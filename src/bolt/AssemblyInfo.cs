using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if BOLT_DLL
[assembly: AssemblyTitle("bolt")]
[assembly: Guid("e24988b8-bcbc-4e93-a24e-8afc569782fe")]
#elif BOLT_EDITOR_DLL
[assembly: AssemblyTitle("bolt.editor")]
[assembly: Guid("6c922342-f87e-4d21-b4dd-ed7a706e9071")]
#endif

[assembly: InternalsVisibleTo("bolt.user")]
[assembly: InternalsVisibleTo("bolt.editor")]

[assembly: AssemblyDescription("Networking middleware for Unity3D")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("bolt")]
[assembly: AssemblyCopyright("Copyright © Fredrik Holmstrom 2012 - 2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("0.3.1")]

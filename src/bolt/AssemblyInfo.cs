using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if BOLT_DLL
[assembly: AssemblyTitle("bolt")]
[assembly: Guid("e24988b8-bcbc-4e93-a24e-8afc569782fe")]
[assembly: InternalsVisibleTo("bolt.editor")]
[assembly: InternalsVisibleTo("bolt.user")]
#elif BOLT_EDITOR_DLL
[assembly: AssemblyTitle("bolt.editor")]
[assembly: Guid("6c922342-f87e-4d21-b4dd-ed7a706e9071")]
#endif

//uncomment this to access the internals of bolt.dll and bolt.editor.dll in the runtime and editor
//[assembly: InternalsVisibleTo("Assembly-CSharp")]
//[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]

[assembly: AssemblyDescription("Networking middleware for Unity3D")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("bolt")]
[assembly: AssemblyCopyright("Copyright © Fredrik Holmstrom 2012 - 2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("0.2.*")]
//[assembly: AssemblyFileVersion("0.2.*")]

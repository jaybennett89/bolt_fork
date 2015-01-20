using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if UDPKIT
[assembly: AssemblyTitle("udpkit")]
#elif UDPKIT_COMMON
[assembly: AssemblyTitle("udpkit.common")]
#endif

[assembly: AssemblyProduct("udpkit")]

[assembly: InternalsVisibleTo("bolt")]
[assembly: InternalsVisibleTo("bolt.user")]
[assembly: InternalsVisibleTo("bolt.editor")]
[assembly: InternalsVisibleTo("bolt.compiler")]

#if UDPKIT_COMMON
[assembly: InternalsVisibleTo("udpkit")]
[assembly: InternalsVisibleTo("udpkit.master")]
[assembly: InternalsVisibleTo("thundercloud")]
#endif

[assembly: AssemblyDescription(".Net/Mono/Unity networking library for games")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Bolt Engine AB")]
[assembly: AssemblyCopyright("Copyright © 2012-2015 Bolt Engine AB")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("0.3.2.0")]
[assembly: AssemblyFileVersion("0.3.2.0")]

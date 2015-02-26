using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if UDPKIT
[assembly: AssemblyTitle("udpkit")]
#elif UDPKIT_COMMON
[assembly: AssemblyTitle("udpkit.common")]
#elif UDPKIT_WP8
[assembly: AssemblyTitle("udpkit.wp8")]
#endif

[assembly: AssemblyProduct("udpkit")]

#if !UDPKIT_WP8
[assembly: InternalsVisibleTo("bolt")]
[assembly: InternalsVisibleTo("bolt.user")]
[assembly: InternalsVisibleTo("bolt.editor")]
[assembly: InternalsVisibleTo("bolt.compiler")]
#endif

#if UDPKIT_COMMON
[assembly: InternalsVisibleTo("udpkit")]
[assembly: InternalsVisibleTo("udpkit.master")]
[assembly: InternalsVisibleTo("zeus")]
#endif

[assembly: AssemblyDescription(".Net/Mono/Unity networking library for games")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Bolt Engine AB")]
[assembly: AssemblyCopyright("Copyright © 2012-2015 Bolt Engine AB")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("0.4.0.0")]
[assembly: AssemblyFileVersion("0.4.0.0")]

﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if UDPKIT_DLL_UDPKIT
[assembly: AssemblyTitle("udpkit")]
[assembly: AssemblyProduct("udpkit")]
#elif UDPKIT_DLL_PLATFORM_ANDROID
[assembly: AssemblyTitle("udpkit.platform.android")]
[assembly: AssemblyProduct("udpkit.platform.android")]
#elif UDPKIT_DLL_PLATFORM_IOS
[assembly: AssemblyTitle("udpkit.platform.ios")]
[assembly: AssemblyProduct("udpkit.platform.ios")]
#elif UDPKIT_DLL_PLATFORM_WIN32
[assembly: AssemblyTitle("udpkit.platform.win32")]
[assembly: AssemblyProduct("udpkit.platform.win32")]
#elif UDPKIT_DLL_PLATFORM_MANAGED
[assembly: AssemblyTitle("udpkit.platform.managed")]
[assembly: AssemblyProduct("udpkit.platform.managed")]
#elif UDPKIT_DLL_ANDROID
[assembly: AssemblyTitle("udpkit.android")]
[assembly: AssemblyProduct("udpkit.android")]
#endif

[assembly: InternalsVisibleTo("bolt")]
[assembly: InternalsVisibleTo("bolt.editor")]
[assembly: InternalsVisibleTo("bolt.user")]

[assembly: AssemblyDescription(".Net/Mono/Unity networking library for games")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyCopyright("Copyright © 2012-2014 Fredrik Holmstrom")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("0.2.0.0")]
[assembly: AssemblyFileVersion("0.2.0.0")]

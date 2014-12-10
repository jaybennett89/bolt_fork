#include "common.h"

EXPORT_API const char* PlatformName() {
#ifdef UDPKIT_WIN32
	return "win32";
#elif UDPKIT_WIN64
    return "win64";
#elif UDPKIT_IOS_SIMULATOR
    return "ios_simulator";
#elif UDPKIT_IOS
    return "ios";
#elif UDPKIT_OSX
    return "osx";
#elif UDPKIT_ANDROID
    return "android";
#elif UDPKIT_LINUX
    return "linux";
#else
    return "unknown";
#endif
}
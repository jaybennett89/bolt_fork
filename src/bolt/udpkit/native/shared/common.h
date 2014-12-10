#if _WIN64
	#define UDPKIT_WIN64 1
	#define UDPKIT_WIN 1

#elif _WIN32
	#define UDPKIT_WIN32 1
	#define UDPKIT_WIN 1

#elif __APPLE__
	#include "TargetConditionals.h"
	#if TARGET_OS_IPHONE && TARGET_IPHONE_SIMULATOR
		#define UDPKIT_IOS 1
		#define UDPKIT_IOS_SIMULATOR 1
	#elif TARGET_OS_IPHONE
		#define UDPKIT_IOS 1
	#else
		#define UDPKIT_OSX 1
	#endif
#elif __ANDROID__
	#define UDPKIT_ANDROID 1
#elif __linux
	#define UDPKIT_LINUX 1
#endif

#if defined(_DEBUG) || defined(DEBUG)
	#define UDPKIT_DEBUG
#endif

#ifndef UNICODE
	#define UNICODE
#endif

#if UDPKIT_WIN
	#define WIN32_LEAN_AND_MEAN
	#define EXPORT_API extern "C" __declspec(dllexport)
#else
	#define EXPORT_API extern "C" 
#endif

typedef signed char S8;
typedef unsigned char U8;
typedef signed short S16;
typedef unsigned short U16;
typedef signed int S32;
typedef unsigned int U32;
typedef signed long long S64;
typedef unsigned long long U64;

#ifndef UDPKIT_ANDROID
static_assert(sizeof(U8) == 1 && sizeof(S8) == 1, "U8 and S8 must be 1 byte");
static_assert(sizeof(U16) == 2 && sizeof(S16) == 2, "U16 and S16 must be 2 bytes");
static_assert(sizeof(U32) == 4 && sizeof(S32) == 4, "U32 and S32 must be 4 bytes");
static_assert(sizeof(U64) == 8 && sizeof(S64) == 8, "U64 and S64 must be 8 bytes");
#endif

#include <errno.h>
#include <string.h>
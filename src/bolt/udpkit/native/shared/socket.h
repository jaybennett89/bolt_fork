#include "common.h"

#define UDPKIT_SOCKET_OK 0
#define UDPKIT_SOCKET_ERROR -1
#define UDPKIT_SOCKET_NOTVALID -2
#define UDPKIT_SOCKET_NODATA -3

#define UDPKIT_CHECK_RESULT(result) \
if (result == SOCKET_ERROR) { \
	return UDPKIT_SOCKET_ERROR; \
} \

#define UDPKIT_CHECK_VALIDSOCKET(s) \
if (s == NULL || s->nativeSocket == INVALID_SOCKET) { \
	return UDPKIT_SOCKET_NOTVALID; \
} \

#if UDPKIT_WIN
	#ifndef _WIN32_WINNT_WINXP 
		#define UDPKIT_WIN_WSAPOLL
	#endif

	#include <WinSock2.h>
	#include <WS2tcpip.h>
	#include <stdio.h>
	#pragma comment(lib, "Ws2_32.lib")
#else
    #include <unistd.h>
    #include <sys/types.h>
    #include <sys/socket.h>
    #include <sys/poll.h>
    #include <arpa/inet.h>
    #include <netdb.h>
    #include <netinet/in.h>
    #include <errno.h>
    #include <sys/ioctl.h>

    #define INVALID_SOCKET -1
    #define SOCKET_ERROR -1
#endif

struct udpEndPoint {
	U32 address;
	U16 port;
};

struct udpSocket {
	S32 nativeSocket;
	U32 sendBufferSize;
	U32 recvBufferSize;
	udpEndPoint endPoint;
};

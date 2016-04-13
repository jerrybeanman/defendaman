#ifndef SERVER_H_
#define SERVER_H_
#include <iostream>
#include <cstring>
#include <cstdlib>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <algorithm>
#include <netdb.h>
#include <utility>
#include <vector>
#include <map>
#include <unistd.h>
#include <pthread.h>
#include <errno.h>
#include <stdio.h>

#define PACKETLEN      	4096
#define PACKETLEN_UDP   512

#define BUFSIZE	        420	
#define MAXCONNECTIONS  8


/*------------------------------------------------------------------------------

  SOURCE FILE:              Server.h

  PROGRAM:                  Defendaman

  FUNCTIONS:                virtual int InitializeSocket(short port) = 0;
                            virtual void Broadcast(const char * message, sockaddr_in * excpt = NULL) = 0;
                            virtual void * Receive() = 0;
                            void fatal(const char* error);
                            int isReadyToInt(Player player);

  DESIGNER/PROGRAMMER:      Jerry Jia, Martin Minkov, Scott Plummer

  NOTES:                    The server base class for both UDP and TCP
-------------------------------------------------------------------------------*/
typedef struct Player
{
    int            socket;
    sockaddr_in    connection;
    int            id;
    char           username[32];
    int            team;
    int            playerClass;
    bool           isReady;
} Player;

/* List of players currently connected to the server */
static std::map<int, Player>           _PlayerTable;

extern bool gameRunning;

namespace Networking
{
	class Server
	{
		public:
			Server() {}
			~Server(){}

      virtual int InitializeSocket(short port) = 0;

      virtual void Broadcast(const char * message, sockaddr_in * excpt = NULL) = 0;

      virtual void * Receive() = 0;

      void fatal(const char* error);

      int isReadyToInt(Player player);

	 protected:
     struct sockaddr_in     _ServerAddress;
		 int 				           _UDPReceivingSocket;
     int                    _TCPAcceptingSocket;
     fd_set                 _allset;              // File descriptor set for connected sockets
     int                    _maxfd;               //Maximum amount of file descriptors
     int                    _maxi;                // Current maximum connections

	};
}
#endif

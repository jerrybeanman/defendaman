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
#include <unistd.h>
#include <pthread.h>
#include <errno.h>
#include <stdio.h>

#define PACKETLEN       256
#define BUFSIZE	        420	/* scamaz */
#define MAXCONNECTIONS  8

/*
   Structure of a PlayerNetworkEntity
 ** Will move to a more appropriate location later
 ** Unsure of info that will be required
 */
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

namespace Networking
{
	class Server
	{
		public:
			Server(){}
			~Server(){}
    	/*
		   Initialize socket, server address to lookup to, and connect to the server

		   @return: socket file descriptor
		 */
		virtual int InitializeSocket(short port) = 0;

		/*
		   Sends a message to all the clients

		 */
		virtual void Broadcast(char * message) = 0;

    virtual void * Receive() = 0;

    virtual void PrepareSelect() = 0;

    virtual int SetSocketOpt() = 0;

		void fatal(const char* error);

		protected:
		struct sockaddr_in     _ServerAddress;
		int 				           _UDPReceivingSocket;
		int                    _TCPAcceptingSocket;
    fd_set                 _allset;              // File descriptor set for connected sockets
    int                    _maxfd;               //Maximum amount of file descriptors
    int                    _maxi;                // Current maximum connections


    /* List of players currently connected to the server */
    std::vector<Player>             _PlayerList;

	};
}
#endif

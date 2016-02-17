#ifndef SERVER_H_
#define SERVER_H_
#include <iostream>
#include <cstring>
#include <cstdlib>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h>
#include <utility>
#include <vector>
#include <unistd.h>
#include <pthread.h>

#define PACKETLEN       2000
#define BUFSIZE	        420	/* scamaz */
#define MAXCONNECTIONS  8

/*
   Structure of a PlayerNetworkEntity
   ** Will move to a more appropriate location later
   ** Unsure of info that will be required
*/
typedef struct Player {
    sockaddr_in    connection;
    std::string    username;
    std::string    team;
} Player;

namespace Networking
{
    class Server
    {
        public:

            /****************************************************************************
            Initialize socket, server address to lookup to, and connect to the server

            @return: socket file descriptor
            *****************************************************************************/
            int Init_TCP_Server_Socket(char* name, short port);

            /****************************************************************************
            Initialize socket, server address to lookup to, and connect to the server

            @return: socket file descriptor
            *****************************************************************************/
            int TCP_Server_Accept();

/****************************************************************************
			Infinite Loop for listening on a connect client's socket. This is used by
			threads.

			@return: NULL
*****************************************************************************/
			void TCP_Server_Listen(int ClientSocket);

/****************************************************************************
            Recives packets from a specific socket, should be in a child proccess

            @return: packet of size PACKETLEN
            *****************************************************************************/
            std::string TCP_Server_Recieve(int TargetSocket);


            /****************************************************************************
            Initialize socket, and server address to lookup to

            @return: socket file descriptor and the server address for future use
            *****************************************************************************/
            int Init_UDP_Server_Socket(char* name, short port);


            /****************************************************************************
            Listen for incoming UDP traffics

            @return: a packet
            *****************************************************************************/
            std::string UDP_Server_Recv();

            /****************************************************************************
            prints the list of addresses currently stored

            @return: void
            *****************************************************************************/
            void PrintAddresses();

	    /****************************************************************************
            Sends a message to all the clients

            *****************************************************************************/
            void pingAll(char* message);

            void fatal(char* error);

            void UDP_Server_Send(const char* message);

        private:
        	struct sockaddr_in 	_ServerAddress;
        	int 				_ListeningSocket;
        	int 				_AcceptingSocket;

            /* List of client addresses currently connected */
            std::vector<struct sockaddr_in> _ClientAddresses;

            /* List of client sockets currently connected */
            std::vector<int>                _ClientSockets;

            /**
             * Note:
             * This is placeholder code for two teams.
             * TODO: a vector of teams.
             * /

            /* Team ONE - Player connections and info. */
            std::vector<Player>             _TeamOne;

            /* Team TWO - Player connections and info. */
            std::vector<Player>             _TeamTwo;
    };
}
#endif

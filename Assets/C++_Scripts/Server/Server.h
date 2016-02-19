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
#include <errno.h>
#include <stdio.h>

#define PACKETLEN       128
#define BUFSIZE	        420	/* scamaz */
#define MAXCONNECTIONS  8


/*
   Structure of a PlayerNetworkEntity
   ** Will move to a more appropriate location later
   ** Unsure of info that will be required
*/
typedef struct Player {
    int            socket;
    sockaddr_in    connection;
    int            id;
    char           username[32];
    char           team[32];
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
            virtual void Broadcast(char* message) = 0;

            /*
                Initialize socket, and server address to lookup to

                @return: socket file descriptor and the server address for future use
            */
            int Init_UDP_Server_Socket(short port);


            /*
                Listen for incoming UDP traffics

                @return: a packet
            */
            std::string UDP_Server_Recv();

            /*
                prints the list of addresses currently stored

                @return: void
            */
            void PrintAddresses();


            void fatal(const char* error);

            void UDP_Server_Send(const char* message);

        protected:
        	struct sockaddr_in     _ServerAddress;
        	int 				   _UDPReceivingSocket;
            int                    _TCPAcceptingSocket;

    };
}
#endif

#ifndef CLIENT_H_
#define CLIENT_H_
#include <iostream>
#include <cstring>
#include <cstdlib>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h>
#include <utility>
#include <stdio.h>
#include <errno.h>
#include <unistd.h>
#include <pthread.h>
#include "CircularBuffer.h"

#define TeamRequest1 1
#define TeamRequest2 2
//#define PACKETLEN    50
//#define MAXPACKETS   10

namespace Networking
{
    class Client
    {
        public:
          pthread_t ReadThread;

          Client();

            /*
                Initialize socket, server address to lookup to, and connect to the server

                @return: socket file descriptor
            */
            int Init_TCP_Client_Socket(const char* name, short port);

            /*
                Initialize socket, and server address to lookup to

                @return: socket file descriptor and the server address for future use
            */
            std::pair<int, struct sockaddr> Init_UDP_Client_Socket(char* name, short port);

            /*
                Initialize the socket address structure by recieving the port number and
                either the hostname or an IPV4 address

                @return: socket file descriptor and the server address for future use
            */
            struct sockaddr Init_SockAddr(const char* hostname, short hostPort);


            int receiveUDP(int sd, struct sockaddr* server, char* rbuf);

            /*
                Wrapper function for UDP sendTo function. Failing to send prints an error
                message with the data intended to send.

                @return: the number of bytes sent, otherwise return -1 for error
            */
            int sendUDP(int socket, char *data, struct sockaddr server);

            void * Recv();


            int Send(char * message, int size);

            void fatal(const char* error);

            char* GetData();
            static const int MAXPACKETS = 10;
            static const int PACKETLEN = 50;

        private:
            CircularBuffer CBPackets; // buffer for data coming in from network
            int serverSocket;
            char* currentData; // the single instance of data exposed to Unity for reading
    };
}
#endif

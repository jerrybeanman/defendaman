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

namespace Networking
{
    class Client
    {
        public:
             Client();
            ~Client();

            /****************************************************************************
            Initialize socket, server address to lookup to, and connect to the server 

            @return: socket file descriptor 
            *****************************************************************************/
            int Init_TCP_Client_Socket(char* name, short port);

            /****************************************************************************
            Initialize socket, and server address to lookup to 
            
            @return: socket file descriptor and the server address for future use 
            *****************************************************************************/
            std::pair<int, struct sockaddr> Init_UDP_Client_Socket(char* name, short port);


            /****************************************************************************
            Initialize the socket address structure by recieving the port number and 
            either the hostname or an IPV4 address
            
            @return: socket file descriptor and the server address for future use 
            *****************************************************************************/
            struct sockaddr Init_SockAddr(char* hostname, short hostPort);

            void fatal(char* error);
    };
}
#endif 

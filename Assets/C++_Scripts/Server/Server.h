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

#define PACKETLEN       2000
#define BUFSIZE	        420	/* scamaz */
#define MAXCONNECTIONS  8
namespace Networking
{
    class Server
    {
        public:
             Server();
            ~Server();

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
            void PrintAddresses()


            void fatal(char* error);

        private:
        	struct sockaddr_in 	_ServerAddress;
        	int 				_ListeningSocket;
        	int 				_AcceptingSocket;

            /* List of client addresses currently connected */
            std::vector<struct sockaddr_in> _ClientAddresses; 

            /* List of client sockets currently connected */
            std::vector<int>                _ClientSockets; 
    };
}
#endif
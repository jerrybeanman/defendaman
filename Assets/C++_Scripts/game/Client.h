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
            std::pair<int, struct sockaddr> init_udp_server_socket(char* name, short port);
            struct sockaddr init_sockaddr(char* hostname, short hostPort);
            void fatal(const char* error);
    };
}
#endif 

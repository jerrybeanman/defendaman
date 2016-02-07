#include <string>
#include <sys/types.h>
#include <arpa/inet.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h>
#include <iostream>
#include "Client.h"

//change to real port num
#define PORT_NUM 8000

typedef struct {
    uint32_t x;
    uint32_t y;
    char data[100];
} game_data; 

int main(int argc, char *argv[]) 
{
    int i = 0;
    int socketDes;
    struct sockaddr server;
    game_data data = {htonl(1), htonl(2), "Hello Server"};

    if(argc != 2)
    {
        std::cerr << "Usage: [ip]" << std::endl;
        return 1;
    }
    Networking::Client c;
    std::pair<int, struct sockaddr> socketInfo = c.init_udp_server_socket(argv[1], PORT_NUM);
    socketDes = socketInfo.first;
    server = socketInfo.second;
    while(i < 10) 
    {
        if (sendto(socketDes, &data.x, sizeof(int), 0, &server, sizeof(server)) == -1)
         {
            std::cerr << "sendto failure x" << std::endl;
            exit(1);
        }
        if (sendto(socketDes, &data.y, sizeof(int), 0, &server, sizeof(server)) == -1)
         {
            std::cerr << "sendto failure y" << std::endl;
            exit(1);
        }
        if (sendto(socketDes, data.data, sizeof(data.data), 0, &server, sizeof(server)) == -1)
         {
            std::cerr << "sendto failure string" << std::endl;
            exit(1);
        }
        i++;    
    }
}
#include "Client.h"

using namespace Networking;

int main()
{
    Client* c;
    //Connect to requested IP address and port
    int socket = c->init_tcp_server_socket("192.168.1.104", 7000);
 
    if(socket != -1)
    {
        std::cout << "socket is not null" << std::endl;
        send(socket, "Testing lobby", 13, 0);
    }
    
    return 0;
}

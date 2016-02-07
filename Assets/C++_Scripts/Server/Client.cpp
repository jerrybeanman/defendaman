#include "Client.h" 

using namespace Networking;

std::pair<int, struct sockaddr> Client::init_udp_server_socket(char* name, short port)
{
    // local address that client socket is bound to
    struct sockaddr local;
    // socket file descriptor to the new client socket
    int clientSock;

    // create TCP socket
    if((clientSock = socket(AF_INET, SOCK_DGRAM | SOCK_NONBLOCK, 0)) == -1)
    {
        fatal("failed to create TCP socket");
    }
    local = init_sockaddr(name, port);
    return std::make_pair(clientSock, local);
}

struct sockaddr Client::init_sockaddr(char* hostname, short hostport)
{
    //address that client socket should connect to
    struct sockaddr_in addr;
    
    struct hostent* host;

    //set up port and protocol of address structure
    memset(&addr, 0, sizeof(addr));
    addr.sin_family      = AF_INET;
    addr.sin_port        = htons(hostport);

    if((host = gethostbyname(hostname)) == 0)
    {
        std::cout << "Error in gethostbyname" << std::endl;
    }
    
    memcpy(&addr.sin_addr, host->h_addr, host->h_length);

    struct sockaddr ret;
    memcpy(&ret, &addr, sizeof(ret));
    return ret;
}
void Client::fatal(const char* error)
{
    std::cout << error << std::endl;
    exit(1);
}

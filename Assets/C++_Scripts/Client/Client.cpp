#include "Client.h" 

using namespace Networking;

/****************************************************************************
Initialize socket, server address to lookup to, and connect to the server 

@return: socket file descriptor 
*****************************************************************************/
int Client::Init_TCP_Client_Socket(char* name, short port)
{
    // local address that client socket is bound to
    struct sockaddr local;

    // socket file descriptor to the new client socket
    int clientSock;

    // create TCP socket
    if((clientSock = socket(AF_INET, SOCK_STREAM, 0)) == -1)
    {
        fatal("failed to create TCP socket");
        return -1;
    }

    // Initialize socket address 
    local = Init_SockAddr(name, port);
    
    if(connect(clientSock, (struct sockaddr*) &local, sizeof(local)) == -1)
    {
        fatal("failed to connect to remote host");
        return -1;
    }
    return clientSock;
}

/****************************************************************************
Initialize socket, and server address to lookup to 

@return: socket file descriptor and the server address for future use 
*****************************************************************************/
std::pair<int, struct sockaddr> Client::Init_UDP_Client_Socket(char* name, short port)
{
    // local address that client socket is bound to
    struct sockaddr local;

    // socket file descriptor to the new client socket
    int clientSock;

    // create UDP socket
    if((clientSock = socket(AF_INET, SOCK_DGRAM | SOCK_NONBLOCK, 0)) == -1)
    {
        fatal("failed to create TCP socket");
    }

    // Initialize socket addresss 
    local = Init_SockAddr(name, port);

    // return socket address as well for future use
    return std::make_pair(clientSock, local);
}

/****************************************************************************
Initialize the socket address structure by recieving the port number and 
either the hostname or an IPV4 address

@return: socket file descriptor and the server address for future use 
*****************************************************************************/
struct sockaddr Client::Init_SockAddr(char* hostname, short hostport)
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
void Client::fatal(char* error)
{
    std::cout << error << std::endl;
}

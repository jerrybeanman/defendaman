#include "Server.h" 

using namespace Networking;

/****************************************************************************
Initialize socket, server address to lookup to, and connect to the server 

@return: socket file descriptor 
*****************************************************************************/
int Server::Init_TCP_Server_Socket(char* name, short port)
{   
    int err;

    /* Create a stream socket */
    if ((_AcceptingSocket = socket(AF_INET, SOCK_STREAM, 0)) <0 )
    {
        fatal("Init_TCP_Server_Socket: socket() failed\n");
        return err;
    }

    /* Fill in server address information */
    bzero((char *)&_ServerAddress, sizeof(struct sockaddr_in));
    _ServerAddress.sin_family = AF_INET;
    _ServerAddress.sin_port = htons(port);
    _ServerAddress.sin_addr.s_addr = htonl(INADDR_ANY); // Accept connections from any client

    /* bind server address to accepting socket */
    if ((err = bind(_ListeningSocket, (struct sockaddr *)&_ServerAddress, sizeof(_ServerAddress))) == -1)
    {
        fatal("Init_TCP_Server_Socket: bind() failed\n");
        return err;
    }

    /* Listen for connections */
    listen(_ListeningSocket, MAXCONNECTIONS);

    return 0;
}


/****************************************************************************
Initialize socket, server address to lookup to, and connect to the server 

@return: socket file descriptor 
*****************************************************************************/
int Server::TCP_Server_Accept()
{
    struct sockaddr_in  ClientAddress;        
    int                 NewClientSocket;
    unsigned int        ClientLen = sizeof(ClientAddress);

    /* Accepts a connection from the client */
    if ((NewClientSocket = accept(_ListeningSocket, (struct sockaddr *)&ClientAddress, &ClientLen)) == -1)
    {
        fatal("TCP_Server_Accept: accept() failed\n");
        return NULL;
    }

    /* Adds the address and socket to the vector list */
    _ClientAddresses.push_back(ClientAddress);
    _ClientSockets.push_back(NewClientSocket);

    /***************************************************
    * Create a child process here to handle new client *
    ****************************************************/
    return 0;
}

/****************************************************************************
Recives packets from a specific socket, should be in a child proccess 

@return: packet of size PACKETLEN 
*****************************************************************************/
std::string Server::TCP_Server_Recieve(int TargetSocket)
{
    std::string         packet;                             /* packet to be returned               */                 
    int                 BytesRead;                          /* bytes read from one recv call       */
    int                 BytesToRead;                        /* remaining bytes to read from socket */
    char *              buf = (char *)malloc(BUFSIZE);      /* buffer read from one recv call      */

    BytesToRead = PACKETLEN;

    while ((BytesRead = recv (TargetSocket, buf, PACKETLEN, 0)) < PACKETLEN)
    {
        /* store buffer read into packet */
        packet += buf;

        /* decrement remaining bytes to read */
        BytesToRead -= BytesRead;
    }
    return packet;
}

/****************************************************************************
prints the list of addresses currently stored 

@return: void
*****************************************************************************/
void Server::PrintAddresses()
{
    std::cout << "List of addresses\n";  
    /*for(auto x : _ClientAddresses)  
    {
      std::cout << "scamaz\n";
    }*/
}

/****************************************************************************
Initialize server socket, fill in the server address, and binds the address
to the socket

@return: void
*****************************************************************************/
int Server::Init_UDP_Server_Socket(char* name, short port)
{
    int err;
    /* Create a file descriptor for the socket */
    
    if((err = _ListeningSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP)) < 0)
    {
        fatal("Init_UDP_Server_Socket: socket() failed\n");
        return err;
    }
      
    /* Fill in server socket address structure */
    memset((char *)&_ServerAddress, 0, sizeof(_ServerAddress));
    _ServerAddress.sin_family = AF_INET;
    _ServerAddress.sin_port = htons(port);
    _ServerAddress.sin_addr.s_addr = htonl(INADDR_ANY);

    /* Bind server address to the socket */
    if((err = bind(_ListeningSocket, (sockaddr *)&_ServerAddress, sizeof(_ServerAddress))) < 0)
    {
        fatal("Init_UDP_Server_Socket: bind() failed\n");
        return err;
    }

    return 0;

}

/****************************************************************************
Listen for incoming UDP traffics

@return: a packet
*****************************************************************************/
std::string Server::UDP_Server_Recv()
{
    int err;
    std::string packet;                     /* packet recieved  */
    struct sockaddr_in Client;              /* Incoming client's socket address information */
    unsigned ClientLen = sizeof(Client);
    char * buf = (char *)malloc(BUFSIZE);

    if((err = recvfrom(_ListeningSocket, buf, BUFSIZE, 0, (sockaddr *)&Client, &ClientLen)) <= 0)
    {
        fatal("UDP_Server_Recv: recvfrom() failed\n");
        return NULL;
    }

    _ClientAddresses.push_back(Client);

    packet = buf;
    free(buf);
    return packet;
}

void Server::fatal(char* error)
{
    std::cout << error << std::endl;
}

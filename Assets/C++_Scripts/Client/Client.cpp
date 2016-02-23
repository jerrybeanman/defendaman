#include "Client.h"

using namespace Networking;


/*
    Constructor.
*/
Client::Client()
{
  // Initialize the circular buffer
  CBInitialize(&CBPackets, MAXPACKETS, PACKETLEN);
  // allocate space for the one packet of data
  // that is being exposed to unity
  currentData = (char *)malloc(PACKETLEN);
}
/*
    Initialize socket, server address to lookup to, and connect to the server

    @return: socket file descriptor
*/
int Client::Init_TCP_Client_Socket(const char* name, short port)
{
    // local address that client socket is bound to
    struct sockaddr local;

    // create TCP socket
    if((serverSocket = socket(AF_INET, SOCK_STREAM, 0)) == -1)
    {
        fatal("failed to create TCP socket");
        return -1;
    }

    // Initialize socket address
    local = Init_SockAddr(name, port);

    if(connect(serverSocket, (struct sockaddr*) &local, sizeof(local)) == -1)
    {
        std::cout << errno << std::endl;
        fatal("failed to connect to remote host");
        return -1;
    }
    return 1;
}

/*
    Initialize socket, and server address to lookup to

    @return: socket file descriptor and the server address for future use
*/
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

/*
    Initialize the socket address structure by recieving the port number and
    either the hostname or an IPV4 address

    @return: socket file descriptor and the server address for future use
*/
struct sockaddr Client::Init_SockAddr(const char* hostname, short hostport)
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

void * Client::Recv()
{
    int bytesRead;
    while(1)
    {
        printf("in Recv() \n");
        printf("Address of serverSocket in Recv() %d", &serverSocket);
        char message[PACKETLEN];
        if((bytesRead = recv(serverSocket, message, PACKETLEN, 0)) < 0)
        {
            printf("recv() failed with errno: %d", errno);
            return (void *)errno;
        }
        printf("Recv() before CBPushBack(): %s\n", message);
        // push message to queue
        CBPushBack(&CBPackets, message);
        printf("Recv() got data: %s\n", message);
    }
    return NULL;
}



int Client::Send(char * message, int size)
{
    if (send(serverSocket, message, size, 0) == -1)
    {
      std::cerr << "send() failed with errno: " << errno << std::endl;
      return errno;
    }
    return 0;
}

/*
    Wrapper function for receiving from server. Prints message on error.

    @return: Size of message received, -1 if failed.
*/
int Client::receiveUDP(int sd, struct sockaddr* server, char* rbuf)
{

    int recSz;
    socklen_t server_len = sizeof(server);

    if((recSz = recvfrom (sd, rbuf, strlen(rbuf), 0, (struct sockaddr *)server, &server_len)) < 0)
    {
        std::cerr << "Failed in receiveUDP" << std::endl;
        return -1;
    }
    return recSz;
}

/*
    Wrapper function for UDP sendTo function. Failing to send prints an error
    message with the data intended to send.

    @return: the number of bytes sent, otherwise return -1 for error
*/
int Client::sendUDP(int socket, char *data, struct sockaddr server)
{

    int sent;
    if ((sent = sendto(socket, data, strlen(data), 0, &server, sizeof(server))) == -1)
    {
        std::cerr << "Failed to send UDP: " << data << std::endl;
        return -1;
    }
    return sent;
}


void Client::fatal(const char* error)
{
    perror(error);
}

char* Client::GetData()
{
  if (CBPackets.Count != 0) {
    memset(currentData, 0, PACKETLEN);
    CBPop(&CBPackets, currentData);
    printf("GetData() Got data %s\n", currentData);
  } else
  {
    printf("Address of serverSocket in GetData() %d\n", &serverSocket);
    strcpy(currentData, "[]");
  }
  return currentData;
}

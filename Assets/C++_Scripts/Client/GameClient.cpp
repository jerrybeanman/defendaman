#include "GameClient.h"
using namespace Networking;

/*
    Initialize socket, and server address to lookup to

    @return: socket file descriptor and the server address for future use
*/
int GameClient::Init_Client_Socket(const char* name, short port)
{
    // create UDP socket
    if((serverSocket = socket(AF_INET, SOCK_DGRAM | SOCK_NONBLOCK, 0)) == -1)
    {
        fatal("failed to create TCP socket");
        return 1;
    }

    // Initialize socket addresss
    Init_SockAddr(name, port);

    return 0;
}
void * GameClient::Recv()
{
    int bytesRead = 0;
    socklen_t length = 0;
    while(1)
    {
        length = sizeof(serverAddr);
        int bytesToRead = PACKETLEN;
        char *message = (char *) malloc(PACKETLEN);
        while((recvfrom(serverSocket, message, PACKETLEN, 0, (struct sockaddr *)&serverAddr, &length)) < PACKETLEN)
         {
          if(bytesRead < 0)
          {
            printf("recv() failed with errno: %d\n", errno);
            return (void *)errno;
          }
          message += bytesRead;
          bytesToRead -= bytesRead;
        }
        // push message to queue
        CBPushBack(&CBPackets, message);
        free(message);
    }
    return NULL;
}
/*
    Wrapper function for UDP sendTo function. Failing to send prints an error
    message with the data intended to send.
*/
int GameClient::Send(char * message, int size)
{
    socklen_t length = sizeof(serverAddr);
    if (sendto(serverSocket, message, size, 0, (struct sockaddr *)&serverAddr, length) == -1)
    {
        std::cerr << "sendto() failed with errno: " << errno << std::endl;
        return errno;
    }
    return 0;
}

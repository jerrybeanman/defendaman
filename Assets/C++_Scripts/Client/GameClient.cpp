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
    printf("GameClient::Init_Client_Socket Weee\n");

    // Initialize socket addresss
    Init_SockAddr(name, port);

    return 0;
}
void * GameClient::Recv()
{
    int bytesRead;
    socklen_t length = 0;
    while(1)
    {
        length = sizeof(serverAddr);
        int bytesToRead = PACKETLEN;
        char *message = (char *) malloc(PACKETLEN);
        bytesRead = recvfrom(serverSocket, message, PACKETLEN, 0, (struct sockaddr *)&serverAddr, &length);
        //printf("Recv socket:%zu, message:%s \n", serverAddr.sin_port, message);
        if (bytesRead < 0 && errno != 11)
        {
          printf("recv() failed with errno: %d\n", errno);
          free(message);
          return (void *)errno;
        }

        if (bytesRead <= 0)
        {
          free(message);
          continue;
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
        printf("Failed to send");
        std::cerr << "sendto() failed with errno: " << errno << std::endl;
        return errno;
    }
    return 0;
}

char* GameClient::GetData()
{
  if (CBPackets.Count != 0)
  {
    memset(currentData, 0, PACKETLEN);
    CBPop(&CBPackets, currentData);
  } else
  {
    strcpy(currentData, "[]");
  }
  //printf("GameClient::GetData(): Count:%d, Message:%s\n", CBPackets.Count, currentData);
  return currentData;
}

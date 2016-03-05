#include "LobbyClient.h"

using namespace Networking;

/*
    Initialize socket, server address to lookup to, and connect to the server

    @return: socket file descriptor
*/
int LobbyClient::Init_Client_Socket(const char* name, short port)
{
    // create TCP socket
    if((serverSocket = socket(AF_INET, SOCK_STREAM, 0)) == -1)
    {
        fatal("failed to create TCP socket");
        return -1;
    }

    // Initialize socket address
    Init_SockAddr(name, port);

    //Connect to Server
    if(connect(serverSocket, (struct sockaddr*) &serverAddr, sizeof(serverAddr)) == -1)
    {
        std::cout << errno << std::endl;
        fatal("failed to connect to remote host");
        return -1;
    }
    return 1;
}
void * LobbyClient::Recv()
{
  int bytesRead;
  while(1)
  {
      int bytesToRead = PACKETLEN;
      char *message = (char *) malloc(PACKETLEN);
      while((bytesRead = recv(serverSocket, message, bytesToRead, 0)) < PACKETLEN)
      {
        //printf("Recv\n");
        if(bytesRead < 0)
        {
          printf("recv() failed with errno: %d\n", errno);
          return (void *)errno;
        }
        message += bytesRead;
        bytesToRead -= bytesRead;
      }
      printf("%s", message);
      // push message to queue
      CBPushBack(&CBPackets, message);
      free(message);
    }
    return NULL;
}
/*
    Wrapper function for TCP send function. Failing to send prints an error
    message with the data intended to send.
*/
int LobbyClient::Send(char * message, int size)
{
    if (send(serverSocket, message, size, 0) == -1)
    {
      std::cerr << "send() failed with errno: " << errno << std::endl;
      return errno;
    }
    return 0;
}

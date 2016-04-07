/**********************************************************
Project: Defendaman
Source File: GameClient.cpp
Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
Description: Class to handle data transfers for the client during the game using UDP.
**********************************************************/
#include "GameClient.h"
using namespace Networking;
/*------------------------------------------------------------------------------

  FUNCTION:                   Init_Client_Socket

  DESIGNER/PROGRAMMER:        Martin Minkov, Gabriel Lee, Tyler Trepanier

  Revision History:            2016-03-09  Gabriel Lee
                              Added function headers and comments

  INTERFACE:                  int GameClient::Init_Client_Socket(const char* name, short port)
                                name - The host name of the server.
                                port - The port number of the server.

  RETURNS:                    int : Socket file descriptor and the server address for future use.

  NOTES:                      Initialize socket, and server address to lookup to.
-------------------------------------------------------------------------------*/
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
/*------------------------------------------------------------------------------

  FUNCTION:                   Recv

  DESIGNER/PROGRAMMER:        Jerry Jia, Martin Minkov

  Revision History:            2016-03-09  Gabriel Lee
                              Added function headers and comments

  INTERFACE:                  void * GameClient::Recv()

  RETURNS:                    int : The port file descriptor or an error code

  NOTES:                      Function to receive data from the server.
                              The data received from the server is put onto
                              the circular buffer.

-------------------------------------------------------------------------------*/
void * GameClient::Recv()
{
    int bytesRead = 0;
    socklen_t length = 0;
    while(1)
    {
        length = sizeof(serverAddr);
        char *message = (char *) malloc(PACKETLEN_UDP * sizeof(char));
        bytesRead = recvfrom(serverSocket, message, PACKETLEN_UDP, 0, (struct sockaddr *)&serverAddr, &length);
        if (bytesRead < 0 && errno != 11)
        {
          printf("recv() failed with errno: %d\n", errno);
          free(message);
          return (void *)errno; //Critical error
        }

        if (bytesRead <= 0)
        {
          free(message);
          continue;
        }
        // push message to queue
        printf("GameClient::Recv: %s\n", message);
        CBPushBack(&CBPackets, message);
        free(message);
    }
    return NULL;
}
/*------------------------------------------------------------------------------

  FUNCTION:                   Send

  DESIGNER/PROGRAMMER:        Martin Minkov

  Revision History:            2016-03-09  Gabriel Lee
                              Added function headers and comments

  INTERFACE:                  int GameClient::Send(char * message, int size)
                                message - The pointer to the data to be sent to the server
                                size - The size of the data to send

  RETURNS:                    int : Zero on success, otherwise the error number.

  NOTES:                     Wrapper function for UDP sendTo function.
                              Failing to send prints an error message with
                              the data intended to send.

-------------------------------------------------------------------------------*/
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
/*------------------------------------------------------------------------------

  FUNCTION:                   GetData

  DESIGNER/PROGRAMMER:        Jerry Jia

  INTERFACE:                  char* GameClient::GetData()

  RETURNS:                    char*: the data read in

  NOTES:                      Pops off the and returns the data

-------------------------------------------------------------------------------*/
char* GameClient::GetData()
{
  if (CBPackets.Count != 0)
  {
    memset(currentData, 0, PACKETLEN_UDP);
    CBPop(&CBPackets, currentData);
    printf("GameClient::GetData: %s\n", currentData);
  } else
  {
    strcpy(currentData, "[]");
  }
  //printf("GameClient::GetData(): Count:%d, Message:%s\n", CBPackets.Count, currentData);
  return currentData;
}

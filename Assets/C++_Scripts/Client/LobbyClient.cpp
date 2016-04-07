/*------------------------------------------------------------------------------

  SOURCE FILE:              LobbyClient.cpp

  PROGRAM:                  Defendaman

  FUNCTIONS:                int getSO_ERROR(int fd)
                            void closeSocket(int fd)
                            extern "C" LobbyClient * TCP_CreateClient()
                            extern "C" void TCP_DisposeClient(LobbyClient* client)
                            extern "C" void TCP_DisposeClient(LobbyClient* client)
                            extern "C" int TCP_ConnectToServer(LobbyClient * client, const char * name, short port)
                            void * TCP_Recv(void * arg)
                            extern "C" int TCP_StartReadThread(LobbyClient * client)
                            extern "C" char * TCP_GetData(LobbyClient * client)
                            extern "C" int TCP_Send(LobbyClient * client, char * message, int size)
                            extern "C" GameClient * UDP_CreateClient()
                            extern "C" void UDP_DisposeClient(GameClient* client)
                            extern "C" int UDP_ConnectToServer(GameClient * client, const char * name, short port)
                            void * UDP_Recv(void * arg)
                            extern "C" int UDP_StartReadThread(GameClient * client)
                            extern "C" char * UDP_GetData(GameClient * client)
                            extern "C" int UDP_Send(GameClient * client, char * message, int size)


  DESIGNER/PROGRAMMER:      Gabriel Lee, Tyler Trepanier-Bracken, Vivek Kalia, Jerry

  NOTES:                    The UDP Class to handle all UDP data from the game.

-------------------------------------------------------------------------------*/
/**********************************************************
Project: Defendaman

Source File: LobbyClient.cpp

Programmer: Jerry Jia

Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.

Description: Class to handle data transfers for the client while in the lobby using TCP.
**********************************************************/
#include "LobbyClient.h"

using namespace Networking;
/*------------------------------------------------------------------------------

  FUNCTION:                   Init_Client_Socket

  DESIGNER/PROGRAMMER:        Jerry Jia

  INTERFACE:                  int LobbyClient::Init_Client_Socket(const char* name, short port)

  RETURNS:                    Socket file descriptor.

  NOTES:
-------------------------------------------------------------------------------*/
int LobbyClient::Init_Client_Socket(const char* name, short port)
{
    // create TCP socket
    if((serverSocket = socket(AF_INET, SOCK_STREAM, 0)) == -1)
    {
        printf("failed to create TCP socket\n");
        return -1;
    }

    // Initialize socket address
    Init_SockAddr(name, port);

    //Connect to Server
    if(connect(serverSocket, (struct sockaddr*) &serverAddr, sizeof(serverAddr)) == -1)
    {
        std::cout << errno << std::endl;
        printf("failed to connect to remote host\n");
        return -1;
    }
    return 1;
}
/*------------------------------------------------------------------------------

  FUNCTION:                   Recv

  DESIGNER/PROGRAMMER:        Jerry Jia

  INTERFACE:                  void * LobbyClient::Recv()

  RETURNS:                    void

  NOTES:                     Function to receive data from the server.
               The data received from the server is put onto
               the circular buffer.

-------------------------------------------------------------------------------*/
void * LobbyClient::Recv()
{
    int bytesRead = 0;
    char* bp;
    char *message = (char *) malloc(PACKETLEN);
    memset(message, 0, PACKETLEN);
    while(1)
    {
	bp = message;
        int bytesToRead = PACKETLEN;
        while((bytesRead = recv(serverSocket, bp, bytesToRead, 0)) < PACKETLEN)
        {
	    if (bytesRead == 0)
		break;

            if(bytesRead < 0)
            {
                printf("recv() failed with errno: %d\n", errno);
    		free(message);
                return (void *)errno;
            }
	    printf("IN LOBBYCLIENT: RECV Message : %s - BytesRead : %d - BytesToRead: %d\n", bp, bytesRead, bytesToRead);
            bp += bytesRead;
            bytesToRead -= bytesRead;
	    if(bytesToRead == 0)
	    {
		break;
	    }
        }
	printf("Received: %s\n", message);
        // push message to queue
        CBPushBack(&CBPackets, message);
    }
    free(message);
    return NULL;
}
/*------------------------------------------------------------------------------

  FUNCTION:                   Send

  DESIGNER/PROGRAMMER:        Jerry Jia

  INTERFACE:                  int LobbyClient::Send(char * message, int size)

  RETURNS:                    int : If the send completed correctly

  NOTES:                    Wrapper function for TCP send function.
                            Failing to send prints an error message with
                            the data intended to send.

-------------------------------------------------------------------------------*/
int LobbyClient::Send(char * message, int size)
{
    printf("Sending: %s of size: %d\n", message, size);
    if (send(serverSocket, message, size, 0) == -1)
    {
        std::cerr << "send() failed with errno: " << errno << std::endl;
        return errno;
    }
    return 0;
}

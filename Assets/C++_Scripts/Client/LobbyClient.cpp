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

/**********************************************************
Description: Initialize socket, server address to lookup to, and connect to the server.

Progarmmer: Jerry Jia

Parameters:
    name - The host name of the server.
    port - The port number of the server.

Returns: Socket file descriptor.

Revision History:
    Date        By      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
**********************************************************/
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

/**********************************************************
Description: Function to receive data from the server.
             The data received from the server is put onto
             the circular buffer.

Progarmmer: Jerry Jia

Parameters: none

Returns: void

Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
**********************************************************/
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

/**********************************************************
Description: Wrapper function for TCP send function.
             Failing to send prints an error message with
             the data intended to send.

Progarmmer: Jerry Jia

Parameters:
    message - The pointer to the data to be sent to the server
    size - The size of the data to send

Returns: Zero on success, otherwise the error number.

Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
**********************************************************/
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

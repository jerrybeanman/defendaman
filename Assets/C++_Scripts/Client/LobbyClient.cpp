/**********************************************************
Project: Defendaman
Source File: LobbyClient.cpp
Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
Description: Class to handle data transfers for the client while in the lobby using TCP.
**********************************************************/
#include "LobbyClient.h"

using namespace Networking;

/**********************************************************
Description: Initialize socket, server address to lookup to, and connect to the server.
Parameters:
    name - The host name of the server.
    port - The port number of the server.
Returns: Socket file descriptor.
Revision History:
    Date        Author      Description
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
Parameters: none
Returns: void
Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
**********************************************************/
void * LobbyClient::Recv()
{
    int bytesRead;
    while(1)
    {
        int bytesToRead = PACKETLEN;
        char *message = (char *) malloc(PACKETLEN);
        memset(message, 0, PACKETLEN);
        while((bytesRead = recv(serverSocket, message, bytesToRead, 0)) < PACKETLEN)
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

/**********************************************************
Description: Wrapper function for TCP send function. 
             Failing to send prints an error message with 
             the data intended to send.
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
    if (send(serverSocket, message, size, 0) == -1)
    {
        std::cerr << "send() failed with errno: " << errno << std::endl;
        return errno;
    }
    return 0;
}

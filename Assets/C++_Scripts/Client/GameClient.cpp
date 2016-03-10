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

/**********************************************************
Description: Initialize socket, and server address to lookup to.
Parameters: 
    name - The host name of the server.
    port - The port number of the server.
Returns: Socket file descriptor and the server address for future use.
Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
**********************************************************/
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

/**********************************************************
Description: Wrapper function for UDP sendTo function. 
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

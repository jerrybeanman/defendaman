/**********************************************************
Project: Defendaman

Progarmmer: Jerry Jia

Source File: Client.cpp

Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
    Description: The UDP client class for the in-game data transfers.
**********************************************************/
#include "Client.h"
using namespace Networking;

/**********************************************************
Description: Class constructor

Progarmmer: Jerry Jia

Parameters: none

Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
**********************************************************/
Client::Client()
{
    // Initialize the circular buffer
    CBInitialize(&CBPackets, MAXPACKETS, PACKETLEN);

    // allocate space for the one packet of data
    // that is being exposed to unity
    currentData = (char *)malloc(PACKETLEN);
}

/**********************************************************
Description: Initialize the socket address structure by recieving the port number and
either the hostname or an IPV4 address.

Progarmmer: Jerry Jia

Parameters:
    hostname - The host name of the server.
    hostport - The port number of the server.

Returns: Socket file descriptor and the server address for future use.
Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
**********************************************************/
int Client::Init_SockAddr(const char* hostname, short hostport)
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
        return 1;
    }

    memcpy(&addr.sin_addr, host->h_addr, host->h_length);
    //Assign sockaddr_in variable to Client class variable
    serverAddr = addr;
    return 0;
}

/**********************************************************
Description: Wrapper function to report errors.

Programmer: Jerry jia

Parameters:
    error - The error information to report.

Returns: void

Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
**********************************************************/
void Client::fatal(const char* error)
{
    perror(error);
}

/**********************************************************
Description: Get data from the client's circular buffer.

Programmer: Jerry jia, Dylan Blake

Parameters: none

Returns: Character pointer of the data removed from the circular buffer.

Revision History:
    Date        Author      Description
    2016-03-09  Gabriel Lee Added function headers and comments.
**********************************************************/
char* Client::GetData()
{
    if (CBPackets.Count != 0)
    {
        memset(currentData, 0, PACKETLEN);
        CBPop(&CBPackets, currentData);
    }
    else
    {
        strcpy(currentData, "[]");
    }
    return currentData;
}

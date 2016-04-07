/*------------------------------------------------------------------------------

  SOURCE FILE:              Client.cpp

  PROGRAM:                  Defendaman

  FUNCTIONS:                void CBInitialize(CircularBuffer * CBuff, int MaxSize, int ElementSize)
                            void CBFree(CircularBuffer * CBuff)
                            void CBPushBack(CircularBuffer * CBuff, const void *item)
                            void CBPop(CircularBuffer * CBuff, void * item)

  DESIGNER/PROGRAMMER:      Jerry Jia, Martin Minkov

  NOTES:                    The UDP client class for the in-game data transfers.
-------------------------------------------------------------------------------*/
#include "Client.h"
using namespace Networking;
/*------------------------------------------------------------------------------

  FUNCTION:                   Client

  DESIGNER/PROGRAMMER:        Jerry Jia

  Revision History:            2016-03-09  Gabriel Lee
                              Added function headers and comments.

  INTERFACE:                  Client::Client

  NOTES:                      Constructor for the Client object

-------------------------------------------------------------------------------*/
Client::Client()
{
    // Initialize the circular buffer
    CBInitialize(&CBPackets, MAXPACKETS, PACKETLEN);

    // allocate space for the one packet of data
    // that is being exposed to unity
    currentData = (char *)malloc(PACKETLEN);
}
/*------------------------------------------------------------------------------

  FUNCTION:                   Init_SockAddr

  DESIGNER/PROGRAMMER:        Jerry Jia, Martin Minkov

  Revision History:            2016-03-09  Gabriel Lee
                              Added function headers and comments.

  INTERFACE:                  int Client::Init_SockAddr(const char* hostname, short hostport)
                                hostname - The host name of the server.
                                hostport - The port number of the server.

  RETURNS:                    int : Socket file descriptor and the server address for future use.

  NOTES:                      Initialize the socket address structure by
                              recieving the port number either the hostname or
                              an IPV4 address.

-------------------------------------------------------------------------------*/
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
/*------------------------------------------------------------------------------

  FUNCTION:                   fatal

  DESIGNER/PROGRAMMER:        Jerry Jia

  INTERFACE:                  void Client::fatal(const char* error)
                                error - The error information to report.

  RETURNS:                    void

  NOTES:                      Wrapper function to report errors.

-------------------------------------------------------------------------------*/
void Client::fatal(const char* error)
{
    perror(error);
}
/*------------------------------------------------------------------------------

  FUNCTION:                    GetData

  DESIGNER/PROGRAMMER:        Jerry Jia

  Revision History:           2016-03-09  Gabriel Lee
                              Added function headers and comments.

  INTERFACE:                  char* Client::GetData()

  RETURNS:                    char*: Character pointer of the data removed
                                      from the circular buffer.

  NOTES:                       Get data from the client's circular buffer.

-------------------------------------------------------------------------------*/
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

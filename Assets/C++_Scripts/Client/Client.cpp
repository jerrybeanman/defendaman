#include "Client.h"
using namespace Networking;

/*
    Constructor.
*/
Client::Client()
{
  // Initialize the circular buffer
  CBInitialize(&CBPackets, MAXPACKETS, PACKETLEN);
  // allocate space for the one packet of data
  // that is being exposed to unity
  currentData = (char *)malloc(PACKETLEN);
}

/*
    Initialize the socket address structure by recieving the port number and
    either the hostname or an IPV4 address

    @return: socket file descriptor and the server address for future use
*/
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
        printf("EROROROROROROROROROROROROROROROOOOOOOOOOOOOOOOOOOOOOOO\n");
        printf("EROROROROROROROROROROROROROROROOOOOOOOOOOOOOOOOOOOOOOO\n");
        printf("EROROROROROROROROROROROROROROROOOOOOOOOOOOOOOOOOOOOOOO\n");
        printf("EROROROROROROROROROROROROROROROOOOOOOOOOOOOOOOOOOOOOOO\n");
        printf("EROROROROROROROROROROROROROROROOOOOOOOOOOOOOOOOOOOOOOO\n");
        printf("EROROROROROROROROROROROROROROROOOOOOOOOOOOOOOOOOOOOOOO\n");
        return 1;
    }

    memcpy(&addr.sin_addr, host->h_addr, host->h_length);
    //Assign sockaddr_in variable to Client class variable
    serverAddr = addr;
    return 0;
}

void Client::fatal(const char* error)
{
    perror(error);
}

char* Client::GetData()
{
  if (CBPackets.Count != 0)
  {
    memset(currentData, 0, PACKETLEN);
    CBPop(&CBPackets, currentData);
  } else
  {
    strcpy(currentData, "[]");
  }
  return currentData;
}

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
        return 1;
    }

    memcpy(&addr.sin_addr, host->h_addr, host->h_length);
    //Assign sockaddr_in variable to Client class variable
    serverAddr = addr;
    return 0;
}

void * Client::Recv()
{
    int bytesRead;
    while(1)
    {

        int bytesToRead = PACKETLEN;
        char *message = (char *) malloc(PACKETLEN);
        while((bytesRead = recv(serverSocket, message, bytesToRead, 0)) < PACKETLEN)
         {
           printf("Recv\n");
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

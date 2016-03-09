#include "ServerUDP.h"

using namespace Networking;

/*
	Initialize socket, server address to lookup to, and connect to the server

	@return: socket file descriptor
*/
int ServerUDP::InitializeSocket(short port)
{
  int err = -1;

    /* Create a TCP streaming socket */
    if ((_UDPReceivingSocket = socket(AF_INET, SOCK_DGRAM, 0)) == -1 )
    {
        fatal("InitializeSocket: socket() failed\n");
        return _UDPReceivingSocket;
    }

    if (SetSocketOpt() == -1)
    {
        fatal("InitializeSocket: SetSockOpt failed.\n.");
        return -1;
    }

    /* Fill in server address information */
    memset(&_ServerAddress, 0, sizeof(struct sockaddr_in));
    _ServerAddress.sin_family = AF_INET;
    _ServerAddress.sin_port = htons(port);

    /* bind server address to accepting socket */
    if ((err = bind(_UDPReceivingSocket, (struct sockaddr *)&_ServerAddress, sizeof(_ServerAddress))) == -1)
    {
        std::cout << "InitializeSocket: bind() failed with errno " << errno << std::endl;
        return err;
    }

    PrepareSelect();

    return 0;
}

/*
Thread that forever reads in data from all clients.

Programmer: Unknown

Revisions: Vivek Kalia, Tyler Trepanier-Bracken  2016/03/09
              Added in select functionality
*/
int ServerUDP::SetSocketOpt()
{
	// set SO_REUSEADDR so port can be resused imemediately after exit, i.e., after CTRL-c
    int flag = 1;
    if (setsockopt (_UDPReceivingSocket, SOL_SOCKET, SO_REUSEADDR, &flag, sizeof(flag)) == -1)
	{
        fatal("setsockopt");
		    return -1;
	}
	return 0;
}

/*
	Creates a child process to handle incoming messages from the new player.
  Programmer: Tyler Trepanier-Bracken

	@return: child PDI (0 for child process)
*/
void * ServerUDP::CreateClientManager(void * server)
{
    return ((ServerUDP *)server)->Receive();
}

/*
Thread that forever reads in data from all clients.

Programmer: Unknown

Revisions: Vivek Kalia, Tyler Trepanier-Bracken  2016/03/09
              Added in select functionality

*/
void * ServerUDP::Receive()
{
    int err = 0;
    int nready = 0;
    struct sockaddr_in Client;              /* Incoming client's socket address information */
    unsigned ClientLen = sizeof(Client);
    char* buf = (char *)malloc(BUFSIZE);
    fd_set rset;

    fprintf(stderr, "[maxfd:%d]\n", _maxfd);

    while (1)
    {
      rset = _allset;
      nready = select(_maxfd + 1, &rset, NULL, NULL, NULL);

      if(nready > 0 && FD_ISSET(_UDPReceivingSocket, &rset))
      {
        if((err = recvfrom(_UDPReceivingSocket, buf, PACKETLEN, 0, (sockaddr *)&Client, &ClientLen)) <= 0)
        {
            fatal("UDP_Server_Recv: recvfrom() failed\n");
            return 0;
        }
      }
      else
      {
          fprintf(stderr, "ServerUDP: select error.\n");
      }

      std::cout << buf << std::endl;

      //this->ServerUDP::Broadcast(buf);
    }
    /* Unsure where to put this, this will clear up the select stuff
    close(_UDPReceivingSocket);
    FD_CLR(_UDPReceivingSocket, &_allset);
    */
    free(buf);
}

/*
	Sends a message to all the clients
*/
void ServerUDP::Broadcast(char* message)
{
  for(std::vector<int>::size_type i = 0; i != _PlayerList.size(); i++)
  {
    if(sendto(_UDPReceivingSocket, message, PACKETLEN, 0, (sockaddr *)&_PlayerList[i].connection, sizeof(&_PlayerList[i].connection)) == -1)
    {
      std::cerr << "errno: " << errno << std::endl;
      return;
    }
  }
}
/*
  Registers the passed in Player list as a class member to be used in broadcasting UDP packets.
*/
void ServerUDP::SetPlayerList(std::vector<Player> players)
{
  _PlayerList = players;
}

/*
Prepares the PlayerList with 24 invalid players, required to make sure
that the socket is set to -1 if the socket is not being used for the
function SelectRecv.

Programmer: Vivek Kalia, Tyler Trepanier-Bracken
*/
void ServerUDP::PrepareSelect()
{
    Player _bad;

    //Initialize all components to be invalid!
    _bad.socket = -1;
    bzero((char *)&_bad.connection, sizeof(struct sockaddr_in));
    _bad.id = -1;
    memset(_bad.username, 0, sizeof(_bad.username));
    _bad.team = -1;
    _bad.playerClass = -1;
    _bad.isReady = false;

    fprintf(stderr, "[UDP Socket:%d]\n", _UDPReceivingSocket);

    _maxfd = _UDPReceivingSocket;
    _maxi = -1;

    //Initialize the Player list to bad values.
    std::vector<Player> _clients(24, _bad); //TODO Define 24 as a constant variable
    _PlayerList = _clients;

    FD_ZERO(&_allset);
    FD_SET(_UDPReceivingSocket, &_allset);

}

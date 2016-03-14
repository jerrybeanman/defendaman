#include "ServerUDP.h"
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>

using namespace Networking;

/*
	Initialize socket, server address to lookup to, and connect to the server

  Programmer: Gabriel Lee

  Revision:
    March 9, 2016 - Changed the socket to be non-blocking

	@return: socket file descriptor
*/
int ServerUDP::InitializeSocket(short port)
{
  int err = -1;

    /* Create a TCP streaming socket */
    if ((_UDPReceivingSocket = socket(AF_INET, SOCK_DGRAM | SOCK_NONBLOCK, 0)) == -1 )
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
  int numPlayers = 0;
  struct sockaddr_in Client;              /* Incoming client's socket address information */
  unsigned ClientLen = sizeof(Client);
  char* buf = (char *)malloc(BUFSIZE);
  fd_set rset;
  char con[24][7000];
  bool found = false;

  memset(con, 0, sizeof(con));
  fprintf(stderr, "[maxfd:%d]\n", _maxfd);

  while (1)
  {
    rset = _allset;
    nready = select(_maxfd + 1, &rset, NULL, NULL, NULL);

    if(nready > 0 && FD_ISSET(_UDPReceivingSocket, &rset))
    {
      std::cerr << "hello, map size is: " << _PlayerTable.size() << std::endl;
      if((err = recvfrom(_UDPReceivingSocket, buf, PACKETLEN, 0, (sockaddr *)&Client, &ClientLen)) <= 0)
      {
          fatal("UDP_Server_Recv: recvfrom() failed\n");
          //return 0;
      }
      fprintf(stderr, "From host: %s\n", inet_ntoa (Client.sin_addr));

      for(int i = 0; i < 24; i++)
      {
        if(strcmp(con[i], inet_ntoa (Client.sin_addr)) == 0)
        {
          found = true; //(numPlayers - 24)
          break;
        }
        else
        {
          found = false;
        }
      }

      std::cerr << "[Found:" << found << "]" << std::endl;
      if(!found)
      {
        //memcpy(&_PlayerList[numPlayers].connection, &Client, sizeof(Client));
        std::map<int, Player>::iterator it;
        it = _PlayerTable.find(numPlayers-24);
        if(it == _PlayerTable.end())
        {
          fprintf(stderr, "ServerUDP::Receive: Playerlist full.\n");

        }
        else
        {
          memcpy(&it->second.connection, &Client, sizeof(Client));
          sprintf(con[numPlayers++], "%s", inet_ntoa (Client.sin_addr));
        }

      }
      std::cout << buf << std::endl;


      //TODO: Fix this for future implemenation. Need to parse a vector that TCP delivers
      //Issue: this will only broadcast to the SENDING CLIENT only and will break on more than one client.
      Broadcast(buf);
    }
    else
    {
      fprintf(stderr, "ServerUDP: select error.\n");
    }
  }
  free(buf);
  /* Unsure where to put this, this will clear up the select stuff
  close(_UDPReceivingSocket);
  FD_CLR(_UDPReceivingSocket, &_allset);
  */
}

/*
	Sends a message to all the clients

  Revision:
  Date       Author      Description
  2016-03-10 Gabriel Lee Add functionality to add exception to broadcast
*/
void ServerUDP::Broadcast(char* message, sockaddr_in * excpt)
{
  std::cerr << "broadcasting" << std::endl;
  for(std::map<int, Player>::const_iterator it = _PlayerTable.begin(); it != _PlayerTable.end(); ++it)
  {
    if(strcmp(inet_ntoa ((it->second).connection.sin_addr), "0.0.0.0") == 0)
      continue;
    std::cerr << "Break here?" << std::endl;
    if(excpt != NULL && strcmp(inet_ntoa ((it->second).connection.sin_addr), inet_ntoa(excpt->sin_addr)) == 0)
      continue;
    std::cerr << "Break there?" << std::endl;


      fprintf(stderr, "[Addy:%s][port:%d]\n",
        inet_ntoa ((it->second).connection.sin_addr),
        ntohs(it->second.connection.sin_port));
    if(sendto(_UDPReceivingSocket, message, PACKETLEN, 0, (sockaddr *)&((it->second).connection), sizeof(sockaddr_in)) == -1)
    {
      fprintf(stderr, "Failed to send to [%s]\n",
        inet_ntoa ((it->second).connection.sin_addr));
      perror("ServerUDP::Broadcast");

      return;
    }
  }
}
/*
  Registers the passed in Player list as a class member to be used in broadcasting UDP packets.
*/
void ServerUDP::SetPlayerList(std::map<int, Player> players)
{
  _PlayerTable = players;
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

    std::map<int,Player> _clients;

    //Initialize the Player list to bad values.
    for(int x = -24; x < 0; x++)
    {
      _clients[x] = _bad;
    }

    _PlayerTable = _clients;

    FD_ZERO(&_allset);
    FD_SET(_UDPReceivingSocket, &_allset);

}

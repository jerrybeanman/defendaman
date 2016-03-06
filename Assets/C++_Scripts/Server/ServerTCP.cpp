#include "ServerTCP.h"

using namespace Networking;

/*
	Initialize socket, server address to lookup to, and connect to the server

	@return: socket file descriptor
*/
int ServerTCP::InitializeSocket(short port)
{
    int err = -1;

	int optval = 1;	/* set SO_REUSEADDR on a socket to true (1) */

    /* Create a TCP streaming socket */
    if ((_TCPAcceptingSocket = socket(AF_INET, SOCK_STREAM, 0)) == -1 )
    {
        fatal("InitializeSocket: socket() failed\n");
        return _TCPAcceptingSocket;
    }

    /* Allows other sockets to bind() to this port, unless there is an active listening socket bound to the port already. */
	  setsockopt(_TCPAcceptingSocket, SOL_SOCKET, SO_REUSEADDR, &optval, sizeof(optval));

    /* Fill in server address information */
    memset(&_ServerAddress, 0, sizeof(struct sockaddr_in));
    _ServerAddress.sin_family = AF_INET;
    _ServerAddress.sin_port = htons(port);
    _ServerAddress.sin_addr.s_addr = htonl(INADDR_ANY); // Accept connections from any client

    /* bind server address to accepting socket */
    if ((err = bind(_TCPAcceptingSocket, (struct sockaddr *)&_ServerAddress, sizeof(_ServerAddress))) == -1)
    {
        std::cout << "InitializeSocket: bind() failed with errno " << errno << std::endl;
        return err;
    }

    /* Listen for connections */
    listen(_TCPAcceptingSocket, MAXCONNECTIONS);

    return 0;
}

/*
	Calls accept on a player's socket. Sets the returning socket and client address structure to the player.
	Add connected player to the list of players

	@return: id that is assigned to the player
*/
int ServerTCP::Accept(Player * player)
{
    char buf[PACKETLEN];
    unsigned int        ClientLen = sizeof(player->connection);
    printf("before accept\n");
    /* Accepts a connection from the client */
    if ((player->socket = accept(_TCPAcceptingSocket, (struct sockaddr *)&player->connection, &ClientLen)) == -1)
    {
        std::cerr << "Accept() failed with errno" << errno << std::endl;
        return -1;
    }
    printf("After accept\n");
    /* Not the best way to do it since we're using vectors */
    player->id = _PlayerList.size();
    player->isReady = false;

    _PlayerList.push_back(*player);

    sprintf(buf, "Player %lu has joined the lobby\n", _PlayerList.size());
    printf(buf);
    this->ServerTCP::Broadcast(buf);
    newPlayer = *player;
    return player->id;
}


/*
	Creates a child process to handle incoming messages from new player that has just connected to the lobby

	@return: child PDI (0 for child process)
*/
void * ServerTCP::CreateClientManager(void * server)
{
    /* God forbid */
    return ((ServerTCP *)server)->Receive();
}


/*
	Recieves data from child process that is dedicated for each player's socket

	@return: 1 on success, -1 on error, 0 on disconnect
*/
void * ServerTCP::Receive()
{
    Player tmpPlayer = newPlayer;
  	int BytesRead;
    char * buf;						          	/* buffer read from one recv call      	  */

    //JSON segments
    char dataType[30];
    int code;
    char id[30];
    int idValue;
    int requestValue;

  	buf = (char *)malloc(PACKETLEN); 	/* allocates memory 							        */
    while (1)
    {
      	BytesRead = recv (tmpPlayer.socket, buf, PACKETLEN, 0);

      	if(BytesRead < 0) /* recv() failed */
      	{
      		printf("recv() failed with errno: %d", errno);
      		return 0;
      	}
      	if(BytesRead == 0) /* client disconnected */
      	{
      		sprintf(buf, "Player %d has left the lobby \n", tmpPlayer.id + 1);
          printf(buf);
          this->ServerTCP::Broadcast(buf);
          //Remove player from player list
          for (auto i: _PlayerList)
            std::cout << i.id << std::endl;

          _PlayerList.erase(std::remove_if( _PlayerList.begin(), _PlayerList.end(), [&](Player const& p) { return tmpPlayer.id == p.id; }), _PlayerList.end());

          for (auto i: _PlayerList)
            std::cout << i.id << std::endl;

          return 0;
      	}
        std::cout << buf << std::endl;
        /*Parsed  based on json array*/
        //pls
        sscanf(buf, "%*s %*s %d %*s %*s %*s %d %*s %*s %*s %d ", &code, &idValue, &requestValue);
        this->ServerTCP::CheckServerRequest(tmpPlayer.id, code, idValue, requestValue);

      	/* Broadcast echo packet back to all players */
      	this->ServerTCP::Broadcast(buf);
    }
    free(buf);
    return 0;
}

/*
	Sends a message to all the clients

*/
void ServerTCP::Broadcast(char * message)
{
  std::cout << "MESSAGE IN BROADCAST" << message << std::endl;
	for(std::vector<int>::size_type i = 0; i != _PlayerList.size(); i++)
	{
		if(send(_PlayerList[i].socket, message, PACKETLEN, 0) == -1)
		{
			std::cerr << "Broadcast() failed for player id: " << _PlayerList[i].id + 1 << std::endl;
			std::cerr << "errno: " << errno << std::endl;
			return;
		}
	}
}

/*Parses incoming JSON and process request*/
void ServerTCP::CheckServerRequest(int playerId, int code, int idValue, int requestValue)
{
  char * buf;
  buf = (char *)malloc(PACKETLEN); 	/* allocates memory */
  if(code == Networking && idValue == TeamChangeRequest)
  {
    std::cout << "Team change: " << requestValue << std::endl;
    _PlayerList[playerId].team = requestValue;
  } else if (code == Networking && idValue == ReadyRequest)
  {
    std::cout << "Ready change: " << requestValue << std::endl;
    if (requestValue == 0)
    {
      _PlayerList[playerId].isReady = false;
    } else if (requestValue == 1)
    {
      _PlayerList[playerId].isReady =  true;
    }
    if (this->ServerTCP::AllPlayersReady())
    {
      printf("All players are ready\n");
      sprintf(buf, "Game is starting!\n");
      printf(buf);
      this->ServerTCP::Broadcast(buf); // or use flag to ignore all recv messages
    }
  }
  std::cout << "END OF CHECK SERVER REQUEST" << std::endl;
  free(buf);
}
/* Check ready status on all connected players

   @return true if all players are ready, false otherwise
*/
bool ServerTCP::AllPlayersReady()
{
  for(std::vector<int>::size_type i = 0; i != _PlayerList.size(); i++)
	{
    if(!_PlayerList[i].isReady)
		{
      printf("Player %d is not ready\n", _PlayerList[i].id + 1);
			return false;
		} else {
      printf("Player %d is ready\n", _PlayerList[i].id + 1);
    }
	}
  return true;
}
/*
  Returns the registered player list from the game lobby
*/
std::vector<Player> ServerTCP::setPlayerList()
{
  return _PlayerList;
}

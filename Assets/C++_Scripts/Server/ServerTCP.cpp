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

    _PlayerList.push_back(*player);

    sprintf(buf, "Player %d has joined the lobby\n", _PlayerList.size());
    printf(buf);
    //this->ServerTCP::Broadcast(buf);
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
    int team;

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
      		printf("Player %d has left the lobby \n", tmpPlayer.id + 1);
      		return 0;
      	}

        std::cout << buf << std::endl;
        /*Parsed  based on json array*/
        sscanf(buf, "%s %i %s %i %i", dataType, &code, id, &idValue, &team);
        if(code == Networking && idValue == TeamChangeRequest) {
            std::cout << "Team change: " << team << std::endl;
          _PlayerList[tmpPlayer.id].team = team;
      }

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

void ServerTCP::PrintPlayer(Player p)
{
	std::cout << "Recieved Player " << p.id + 1 << " update: " << std::endl;
	std::cout << "	Username: " << p.username << std::endl;
	std::cout << "	Team name:  " << p.team << std::endl;
}

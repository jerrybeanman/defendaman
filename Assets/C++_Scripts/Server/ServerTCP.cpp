#include "ServerTCP.h"

using namespace Networking;

/*
    Initialize socket, server address to lookup to, and connect to the server

    Interface:  int InitializeSocket(short port)
                [port] Port number

    programmer: Jerry Jia

    @return: socket file descriptor
*/
int ServerTCP::InitializeSocket(short port)
{
    int err = -1;

	int optval = 1;	/* set SO_REUSEADDR on a socket to true (1) */

    /* Create a TCP streaming socket */
    if ((_TCPAcceptingSocket = socket(AF_INET, SOCK_STREAM, 0)) == -1 )
    {
        printf("InitializeSocket: socket() failed with errno\n", errno);
        return -1;
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
        printf("InitializeSocket: bind() failed with errno \n", errno);
        return -1;
    }

    /* Listen for connections */
    listen(_TCPAcceptingSocket, MAXCONNECTIONS);

    return 0;
}

/*
    Calls accept on a player's socket. Sets the returning socket and client address structure to the player.
    Add connected player to the list of players

    Interface:  int Accept(Player * player)
                [player] Pointer to a Player structure

    programmer: Jerry Jia

    @return: socket file descriptor
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

    /* Not the best way to do it since we're using vectors, but it'll do for now */
    player->id = _PlayerList.size();

    _PlayerList.push_back(*player);

    sprintf(buf, "Player %d has joined the lobby\n", _PlayerList.size());
    printf(buf);

    /* Sends this to all the clients */
    this->ServerTCP::Broadcast(buf);

    newPlayer = *player;
    return player->id;
}


/*
    Creates a child process to handle incoming messages from new player that has just connected to the lobby

    Interface:  void * CreateClientManager(void * server)
                [server] Pointer to a void, which has to be a Server object

    Programmer: Jerry Jia

    return: child PDI (0 for child process)
*/
void * ServerTCP::CreateClientManager(void * server)
{
    return ((ServerTCP *)server)->Receive();
}


/*
    Recieves data from child process that is dedicated for each player's socket

    Interface:  void * Receive()

    Programmer: Jerry Jia

    @return: Thread execution code
*/
void * ServerTCP::Receive()
{
    Player tmpPlayer = newPlayer;
  	int BytesRead;
    char * buf;						          	/* buffer read from one recv call  */

  	buf = (char *)malloc(PACKETLEN); 	/* allocates memory 					     */
    while (1)
    {
      	BytesRead = recv (tmpPlayer.socket, buf, PACKETLEN, 0);

      	if(BytesRead < 0)  /* recv() failed */
      	{
      		printf("recv() failed with errno: %d", errno);
      		return 0;
      	}
      	if(BytesRead == 0) /* client disconnected */
      	{
          /*
            TODO:: Delete player off vector
          */
      		printf("Player %d has left the lobby \n", tmpPlayer.id);

      		return 0;
      	}

        buf[2] = 0;
        std::cout << buf << std::endl;

      	/* Broadcast echo packet back to all players */
      	this->ServerTCP::Broadcast(buf);
    }
    free(buf);
    return 0;
}


/*
    Sends a message to all the clients

    Interface:  void Broadcast(char * message)

    Programmer: Jerry Jia

    @return: void
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

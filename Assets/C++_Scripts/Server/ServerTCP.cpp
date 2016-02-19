#include "ServerTCP.h"

using namespace Networking;

/*
	Initialize socket, server address to lookup to, and connect to the server

	@return: socket file descriptor
*/
int ServerTCP::InitializeSocket(short port)
{
    int err = -1;

    /* Create a TCP streaming socket */
    if ((_TCPAcceptingSocket = socket(AF_INET, SOCK_STREAM, 0)) == -1 )
    {
        fatal("InitializeSocket: socket() failed\n");
        return _TCPAcceptingSocket;
    }

    /* Fill in server address information */
    memset(&_ServerAddress, 0, sizeof(struct sockaddr_in));
    _ServerAddress.sin_family = AF_INET;
    _ServerAddress.sin_port = htons(port);
    _ServerAddress.sin_addr.s_addr = htonl(INADDR_ANY); // Accept connections from any client

    /* bind server address to accepting socket */
    if ((err = bind(_TCPAcceptingSocket, (struct sockaddr *)&_ServerAddress, sizeof(_ServerAddress))) == -1)
    {
        std::cout << "InitializeSocket: bind() failed with errno\n" << errno << std::endl;
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
    unsigned int        ClientLen = sizeof(player->connection);

    /* Accepts a connection from the client */
    if ((player->socket = accept(_TCPAcceptingSocket, (struct sockaddr *)&player->connection, &ClientLen)) == -1)
    {
        std::cerr << "Accept() failed with errno" << errno << std::endl;
        return -1;
    }

    /* Not the best way to do it since we're using vectors */ 
    player->id = _PlayerList.size();

    _PlayerList.push_back(*player);
    std::cout << "\n-----Player " << _PlayerList.size() << " has joined the lobby-----\n" << std::endl;
    return player->id;
}

/*
	Creates a child process to handle incoming messages from new player that has just connected to the lobby

	@return: child PDI (0 for child process)
*/
int ServerTCP::CreateClientManager(int PlayerID)
{
	int ChildPID;
	if((ChildPID = fork()) == 0)
	{
		this->ServerTCP::Receive(&_PlayerList[PlayerID]);
	}
	return ChildPID;
}


/*	
	Recieves data from child process that is dedicated for each player's socket

	@return: 1 on success, -1 on error, 0 on disconnect
*/
int ServerTCP::Receive(Player * player)
{
	int BytesRead;
    char *              buf = (char *)malloc(PACKETLEN);      /* buffer read from one recv call      */

    while (1)
    {
    	BytesRead = recv (player->socket, buf, PACKETLEN, 0);
    	if(BytesRead < 0)
    	{
    		std::cerr << "recv() failed with errno: \n" << errno << std::endl;
    		return 0;
    	}
    	if(BytesRead == 0)
    	{
    		std::cerr << "Player " << player->id + 1 <<  " has left the lobby\n"<< std::endl;
    		return 0;
    	}

        /* Format of the packet: "username team" */
    	sscanf(buf, "%s %s", player->username, player->team);
       	PrintPlayer(*player);
    }
    free(buf);
    return 1;
}

/*
	Sends a message to all the clients

*/
void ServerTCP::Broadcast(char* message)
{
	return;
}

void ServerTCP::PrintPlayer(Player p)
{
	std::cout << "Recieved Player " << p.id + 1 << " update: " << std::endl;
	std::cout << "	Username: " << p.username << std::endl;
	std::cout << "	Team name:  " << p.team << std::endl;
}

#include "ServerTCP.h"

using namespace Networking;
using namespace json11;

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
    unsigned int        ClientLen = sizeof(player->connection);

    /* Accepts a connection from the client */
    if ((player->socket = accept(_TCPAcceptingSocket, (struct sockaddr *)&player->connection, &ClientLen)) == -1)
    {
        std::cerr << "Accept() failed with errno" << errno << std::endl;
        return -1;
    }

    player->isReady = false;
    player->playerClass = 0;
    player->team = 0;

    //Add player to list
    int id = getPlayerId(inet_ntoa(player->connection.sin_addr));
    player->id = id;
    _PlayerTable.insert(std::pair<int, Player>(id, *player));

    newPlayer = *player;
    return player->id;
}

/*
	Creates a child process to handle incoming messages from new player that has just connected to the lobby

	@return: child PDI (0 for child process)
*/
void * ServerTCP::CreateClientManager(void * server)
{
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

  	buf = (char *)malloc(PACKETLEN); 	/* allocates memory 							        */
    while (1)
    {
      	BytesRead = recv (tmpPlayer.socket, buf, PACKETLEN, 0);

        /* recv() failed */
      	if(BytesRead < 0)
      	{
      		printf("recv() failed with errno: %d", errno);
      		return 0;
      	}
        /* client disconnected */
      	if(BytesRead == 0)
      	{
      		sprintf(buf, "Player %d has left the lobby \n", tmpPlayer.id);
          printf(buf);

          //Send all players that this player has left
          this->ServerTCP::Broadcast(buf);

          //Remove player from player list
          _PlayerTable.erase(tmpPlayer.id);

          return 0;
      	}
        //Handle Data Received
        this->ServerTCP::CheckServerRequest(tmpPlayer, buf);
    }
    free(buf);
    return 0;
}

/*
	Sends a message to all the clients
*/
void ServerTCP::Broadcast(const char * message)
{
  Player tmpPlayer;

  for(const auto &pair : _PlayerTable)
  {
    tmpPlayer = pair.second;
    if(send(tmpPlayer.socket, message, PACKETLEN, 0) == -1)
    {
      std::cerr << "Broadcast() failed for player id: " << pair.first << std::endl;
			std::cerr << "errno: " << errno << std::endl;
			return;
    }
  }
}

/*
	Sends a message to a specific client
*/
void ServerTCP::sendToClient(Player player, const char * message)
{
	if(send(player.socket, message, PACKETLEN, 0) == -1)
	{
		std::cerr << "Broadcast() failed for player id: " << player.id << std::endl;
		std::cerr << "errno: " << errno << std::endl;
		return;
	}
}


/* Parses incoming JSON and process request */
void ServerTCP::CheckServerRequest(Player player, char * buffer)
{
  std::string error;
  Json json = Json::parse(buffer, error).array_items()[0];

  if (json["DataType"].int_value() != Networking)
    return;

  switch(json["ID"].int_value())
  {
    //Player joining team request
    case TeamChangeRequest:
      std::cout << "Team change: " << json["TeamID"].int_value() << std::endl;
      _PlayerTable[player.id].team = json["TeamID"].int_value();
      this->ServerTCP::Broadcast(buffer);
      break;

    //Player joining class request
    case ClassChangeRequest:
      std::cout << "Class change: " << json["ClassID"].int_value() << std::endl;
      _PlayerTable [player.id].playerClass = json["ClassID"].int_value();
      this->ServerTCP::Broadcast(buffer);
      break;

    //Player making a ready request
    case ReadyRequest:
      std::cout << "Ready change: " << (json["Ready"].int_value() ? "ready" : "not ready") << std::endl;
      _PlayerTable[player.id].isReady = (json["Ready"].int_value() ? true : false);
      this->ServerTCP::Broadcast(buffer);

      break;

    //New Player has joined lobby
    case PlayerJoinedLobby:
  	  std::cout << "New Player Change: " << json["UserName"].string_value() << std::endl;
  	  strcpy(_PlayerTable[player.id].username, json["UserName"].string_value().c_str());

  	  //Send player a table of players
  	  sendToClient(player, constructPlayerTable().c_str());

      //Create packet and send to everyone
      this->ServerTCP::Broadcast(UpdateID(_PlayerTable[player.id]).c_str());
      break;
    case PlayerLeftLobby:
      std::cout << "Player: " << json["PlayerID"].int_value() << " has left the lobby" << std::endl;
      _PlayerTable.erase(json["PlayerID"].int_value());
      this->ServerTCP::Broadcast(buffer);
      break;
    case GameStart:
      std::cout << "Player: " << json["PlayerID"].int_value() << " has started the game" << std::endl;
      //All players in lobby are ready
      if (this->ServerTCP::AllPlayersReady())
      {
        this->ServerTCP::Broadcast(buffer);
        this->ServerTCP::Broadcast(generateMapSeed().c_str());
      }
      break;

  }
}

/*
     Takes in a buffer holding the JSON string received from the client and formats the data into
     the variables that are passed in to be used in other functions.
    Example: [{"DataType" : 6, "ID" : 1, "PlayerID" : 0, "TeamID" : 1}]
*/
void ServerTCP::parseServerRequest(char* buffer, int& DataType, int& ID, int& IDValue, std::string& username)
{
  std::string packet(buffer);
  std::string error;
  //Parse buffer as JSON array
  Json json = Json::parse(packet, error).array_items()[0];

  //Parse failed
  if (!error.empty())
  {
    printf("Failed: %s\n", error.c_str());
    return;
  }

  //Parsing data in JSON object
  DataType  = json["DataType"].int_value();
  ID        = json["ID"].int_value();
  IDValue   = json["TeamID"].int_value();         //Check if player is making a team request
  if (IDValue == 0)
    IDValue = json["ClassID"].int_value();      //Check if player is making a class request

  username = json["UserName"].string_value();

}
/* Check ready status on all connected players

   @return true if all players are ready, false otherwise
*/
bool ServerTCP::AllPlayersReady()
{
  Player tmpPlayer;
  for(const auto &pair : _PlayerTable)
  {
    tmpPlayer = pair.second;
    if(tmpPlayer.isReady == false)
    {
      printf("Player %d is not ready\n", tmpPlayer.id);
			return false;
    } else {
      printf("Player %d is ready\n", tmpPlayer.id);
    }
  }
  return true;
}
std::string ServerTCP::constructPlayerTable()
{
	std::string packet = "[{\"DataType\" : 6, \"ID\" : 6, \"LobbyData\" : ";
	for (auto it = _PlayerTable.begin(); it != _PlayerTable.end(); ++it)
	{
		std::string tempUserName((it->second).username);
		packet += "[{PlayerID: " + std::to_string(it->first);
		packet += ", UserName : \"" + tempUserName + "\"";
		packet += ", TeamID : " +  std::to_string((it->second).team);
		packet += ", ClassID : " + std::to_string((it->second).playerClass);
		packet += ", Ready : " + std::to_string(Server::isReadyToInt(it->second));
		packet += "}";
	}
	packet += "]}]";

  std::cout << "THIS IS OUR PACKET THAT WE ARE SENDING" << packet << std::endl;
	return packet;
}
/*
  Returns the registered player list from the game lobby
*/
std::string ServerTCP::UpdateID(const Player& player)
{
    char buf[PACKETLEN];
    std::cout << player.username << std::endl;
    sprintf(buf, "[{\"DataType\" : 6, \"ID\" : 4, \"PlayerID\" : %d, \"UserName\" : \"%s\"}]", player.id, player.username);
    std::string temp(buf);
    std::cout << "IN UPDATE ID: " << temp << std::endl;
    return temp;
}

std::string ServerTCP::generateMapSeed(){
	int mapSeed;
	srand (time (NULL));
	mapSeed = rand ();
	std::string packet = "{\"DataType\" : 3, \"ID\" : 0, \"Seed\" : " + std::to_string(mapSeed) +"}";
	return packet;
}

std::map<int, Player> ServerTCP::getPlayerTable()
{
  return _PlayerTable;
}

int ServerTCP::getPlayerId(std::string ipString)
{
  std::size_t index = ipString.find_last_of(".");
  return stoi(ipString.substr(index+1));
}

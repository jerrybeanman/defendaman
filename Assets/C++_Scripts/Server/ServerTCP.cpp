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
    char buf[PACKETLEN];
    unsigned int        ClientLen = sizeof(player->connection);

    /* Accepts a connection from the client */
    if ((player->socket = accept(_TCPAcceptingSocket, (struct sockaddr *)&player->connection, &ClientLen)) == -1)
    {
        std::cerr << "Accept() failed with errno" << errno << std::endl;
        return -1;
    }

    /* Not the best way to do it since we're using vectors */
    player->id = _PlayerList.size();
    player->isReady = false;
    player->playerClass = 0;
    player->team = 0;

    //Add player to list
    _PlayerList.push_back(*player);

    //Broadcast to all players the new player ID
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
          _PlayerList.erase(std::remove_if( _PlayerList.begin(), _PlayerList.end(), [&](Player const& p) { return tmpPlayer.id == p.id; }), _PlayerList.end());

          return 0;
      	}
        /* Data received */
        std::cout << "Data Received: " << buf << std::endl;
        //Handle Data Received
        this->ServerTCP::CheckServerRequest(tmpPlayer, buf);
      	/* Broadcast echo packet back to all players */
        //TODO - Send ID of new player to all players
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
			std::cerr << "Broadcast() failed for player id: " << _PlayerList[i].id << std::endl;
			std::cerr << "errno: " << errno << std::endl;
			return;
		}
	}
}
/*
	Sends a message to a specific client
*/
void ServerTCP::sendToClient(Player player, char * message)
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
  char * buf;
  buf = (char *)malloc(PACKETLEN);

  //variables needed to hold json values
  int code, idValue, requestValue;
  std::string username;


  //Parse JSON buffer
  parseServerRequest(buffer, code, idValue, requestValue, username);

  if (code != Networking)
    return;

  switch(idValue)
  {
    //Player joining team request
    case TeamChangeRequest:
      std::cout << "Team change: " << requestValue << std::endl;
      _PlayerList[player.id].team = requestValue;
      break;

    //Player joining class request
    case ClassChangeRequest:
	  std::cout << "Class change: " << requestValue << std::endl;
      _PlayerList[player.id].playerClass= requestValue;
      break;

    //Player making a ready request
    case ReadyRequest:
      std::cout << "Ready change: " << requestValue << std::endl;
      //Player not ready
      if (requestValue == 0)
      {
        _PlayerList[player.id].isReady = false;
      }
      //Player is ready
      else if (requestValue == 1)
      {
        _PlayerList[player.id].isReady =  true;
      }
      //All players in lobby are ready
      if (this->ServerTCP::AllPlayersReady())
      {
        printf("All players are ready\n");
        sprintf(buf, "Game is starting!\n");
        printf(buf);
        this->ServerTCP::Broadcast(buf);
      }
      break;

    //New Player has joined lobby
    case PlayerJoinedLobby:
	  char* message = (char*)malloc(PACKETLEN);
	  std::cout << "New Player Change: " << username << std::endl;
	  strcpy(_PlayerList[player.id].username, username.c_str());
	  strcpy(message, constructPlayerTable().c_str());

	  //Send player a table of players
	  sendToClient(player, message);
      //Create packet and send to every1
        //We're developers
      this->ServerTCP::Broadcast((char *)UpdateID(_PlayerList[player.id]).c_str());
      free(message);
      break;
  }
  free(buf);
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
  DataType = json["DataType"].int_value();
  ID = json["ID"].int_value();
  IDValue = json["TeamID"].int_value();         //Check if player is making a team request
  if (IDValue == 0)
    IDValue = json["ClassID"].int_value();      //Check if player is making a class request

  username = json["UserName"].string_value();

}
/* Check ready status on all connected players

   @return true if all players are ready, false otherwise
*/
bool ServerTCP::AllPlayersReady()
{
  for(std::vector<int>::size_type i = 0; i != _PlayerList.size(); i++)
	{
    if(_PlayerList[i].isReady == false)
	{
      printf("Player %d is not ready\n", _PlayerList[i].id);
			return false;
	} else {
      	printf("Player %d is ready\n", _PlayerList[i].id);
    	}
	}
  return true;
}
std::string ServerTCP::constructPlayerTable()
{
	std::string packet = "[{\"DataType\" : 6, \"ID\" : 6, \"LobbyData\" : ";
	for (std::vector<int>::size_type i = 0; i != _PlayerList.size(); i++)
	{
		std::string tempUserName(_PlayerList[i].username);
		packet += "[{PlayerID: " + std::to_string(_PlayerList[i].id);
		packet += ", UserName : \"" + tempUserName + "\"";
		packet += ", TeamID : " +  std::to_string(_PlayerList[i].team);
		packet += ", ClassID : " + std::to_string(_PlayerList[i].playerClass);
		packet += ", Ready : " + std::to_string(Server::isReadyToInt(_PlayerList[i]));
		packet += "}";
	}
	packet += "]}]";

  std::cout << "THIS IS OUR PACKET THAT WE ARE SENDING" << packet << std::endl;
	return packet;
}
/*
  Returns the registered player list from the game lobby
*/
std::vector<Player> ServerTCP::setPlayerList()
{
  return _PlayerList;
}
std::string ServerTCP::UpdateID(const Player& player)
{
   char buf[PACKETLEN];
    std::cout << player.username << std::endl;
   sprintf(buf, "[{\"DataType\" : 6, \"ID\" : 4, \"PlayerID\" : %d, \"UserName\" : \"%s\"}]", player.id, player.username);
   std::string temp(buf);
    std::cout << "IN UPDATE ID: " << temp << std::endl;
   return temp;
}
char *  ServerTCP::UpdatePlayerId(std::string packet, const Player& tmpPlayer) {
    auto playerIdIndex = packet.find_last_of("\"PlayerID\" : ");
    if(playerIdIndex != std::string::npos) {
            packet.replace(playerIdIndex, 1, std::to_string(tmpPlayer.id));
     } else {
         std::cerr << "No player id" << std::endl;
     }
   std::cerr << (char *) packet.c_str() << std::endl;
   return (char *)packet.c_str();
}

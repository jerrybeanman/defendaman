/*------------------------------------------------------------------------------

  SOURCE FILE: ServerTCP

  PROGRAM: Server

  FUNCTIONS:
          int ServerTCP::InitializeSocket(short port)
          int ServerTCP::Accept(Player * player)
          void * ServerTCP::CreateClientManager(void * server)
          void * ServerTCP::Receive()
          void ServerTCP::Broadcast(const char* message, sockaddr_in * excpt)
          void ServerTCP::sendToClient(Player player, const char * message)
          void ServerTCP::CheckServerRequest(Player player, char * buffer)
          bool ServerTCP::AllPlayersReady()
          std::string ServerTCP::constructPlayerTable()
          std::string ServerTCP::UpdateID(const Player& player)
          std::string ServerTCP::generateMapSeed()
          std::map<int, Player> ServerTCP::getPlayerTable()
          int ServerTCP::getPlayerId(std::string ipString)

  DESIGNER/PROGRAMMER: Network team / Scott, Jerry, Dylan,
                                    Martin, Gabriella Chueng, Dhivya Manohar

  NOTES: The TCP Class to handle all TCP data from the game in both the lobby and
  in the game.

-------------------------------------------------------------------------------*/
#include "ServerTCP.h"

using namespace Networking;
using namespace json11;
extern std::map<int, Player>           _PlayerTable;
/*------------------------------------------------------------------------------

  FUNCTION:                   InitializeSocket

  DESIGNER/PROGRAMMER:        Jerry Jia

  INTERFACE:                  int ServerUDP::InitializeSocket(short port)

  RETURNS:                    -1 on failure, 0 on success

  NOTES:                      Initialize server socket and address

-------------------------------------------------------------------------------*/
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
/*------------------------------------------------------------------------------

  FUNCTION:                   Accept

  DESIGNER/PROGRAMMER:        Jerry Jia, Martin Minkov

  REVISIONS:

  INTERFACE:                  int ServerTCP::Accept(Player * player)

  RETURNS:                    int : If the accept was sucessful

  NOTES:
-------------------------------------------------------------------------------*/

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
/*------------------------------------------------------------------------------

  FUNCTION:                   CreateClientManager

  DESIGNER/PROGRAMMER:        Jerry Jia

  REVISIONS:

  INTERFACE:                  void * ServerTCP::CreateClientManager(void * server)

  RETURNS:                    int : Pointer to the receive function

  NOTES:
-------------------------------------------------------------------------------*/

void * ServerTCP::CreateClientManager(void * server)
{
    return ((ServerTCP *)server)->Receive();
}
/*------------------------------------------------------------------------------

  FUNCTION:                   Receive

  DESIGNER/PROGRAMMER:        Jerry Jia, Martin Minkov, Scott Plummer

  REVISIONS:

  INTERFACE:                  void * ServerTCP::Receive()

  RETURNS:                    thread return value

  NOTES:
-------------------------------------------------------------------------------*/
void * ServerTCP::Receive()
{
    Player tmpPlayer = newPlayer;
  	int BytesRead;
    char * buf;						          	/* buffer read from one recv call      	  */

  	buf = (char *)malloc(PACKETLEN); 	/* allocates memory 							        */
    memset(buf, 0, PACKETLEN);
    while (1)
    {
      int bytesToRead = PACKETLEN;
      char *bp = buf;
       while((BytesRead = recv(tmpPlayer.socket, bp, bytesToRead, 0)) < PACKETLEN)
       {
         if (BytesRead == 0)
            break;

         bytesToRead -= BytesRead;
         bp += BytesRead;
         if(bytesToRead == 0) {
           break;
         }
       }

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
          printf("%s", buf);

          //Send all players that this player has left
          this->ServerTCP::Broadcast(buf);

          //Remove player from player list
          close(_PlayerTable[tmpPlayer.id].socket);
          _PlayerTable.erase(tmpPlayer.id);

          return 0;
      	}
        //Handle Data Received
        this->ServerTCP::CheckServerRequest(tmpPlayer, buf);
        //memset(buf, 0, PACKETLEN);
    }
    free(buf);
    return 0;
}
/*------------------------------------------------------------------------------

  FUNCTION:                   Broadcast

  DESIGNER/PROGRAMMER:        Jerry Jia, Gabriella Chueng, Scott Plummer

  REVISIONS:

  INTERFACE:                 void Broadcast(const char* message, sockaddr_in * excpt)

  RETURNS:                    void

  NOTES:
-------------------------------------------------------------------------------*/
void ServerTCP::Broadcast(const char* message, sockaddr_in * excpt)
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
/*------------------------------------------------------------------------------

  FUNCTION:                   sendToClient

  DESIGNER/PROGRAMMER:        Martin Minkov, Scott Plummer

  REVISIONS:

  INTERFACE:                  void ServerTCP::sendToClient(Player player, const char * message)

  RETURNS:                    void

  NOTES:                      Initializes the UDP socket and sock addr in struct

-------------------------------------------------------------------------------*/
void ServerTCP::sendToClient(Player player, const char * message)
{
	if(send(player.socket, message, PACKETLEN, 0) == -1)
	{
		std::cerr << "Broadcast() failed for player id: " << player.id << std::endl;
		std::cerr << "errno: " << errno << std::endl;
		return;
	}
}
/*------------------------------------------------------------------------------

  FUNCTION:                   CheckServerRequest

  DESIGNER/PROGRAMMER:        Jerry Jia, Martin Minkov, Scott Plummer, Dylan Blake

  REVISIONS:

  INTERFACE:                  void ServerTCP::CheckServerRequest(Player player, char * buffer)

  RETURNS:                    void

  NOTES:

-------------------------------------------------------------------------------*/
void ServerTCP::CheckServerRequest(Player player, char * buffer)
{
  std::string error;
  Json json = Json::parse(buffer, error).array_items()[0];

  if (json["DataType"].int_value() != Networking) {
    this->ServerTCP::Broadcast(buffer);
    return;
  }

  switch(json["ID"].int_value())
  {
    //Player joining team request
    case TeamChangeRequest:
      std::cout << "Team change: " << json[TeamID].int_value() << std::endl;
      _PlayerTable[player.id].team = json[TeamID].int_value();
      this->ServerTCP::Broadcast(buffer);
      break;

    //Player joining class request
    case ClassChangeRequest:
      std::cout << "Class change: " << json[ClassID].int_value() << std::endl;
      _PlayerTable [player.id].playerClass = json[ClassID].int_value();
      this->ServerTCP::Broadcast(buffer);
      break;

    //Player making a ready request
    case ReadyRequest:
      std::cout << "Ready change: " << (json[Ready].int_value() ? "ready" : "not ready") << std::endl;
      _PlayerTable[player.id].isReady = (json[Ready].int_value() ? true : false);
      this->ServerTCP::Broadcast(buffer);
      break;

    //New Player has joined lobby
    case PlayerJoinedLobby:
  	  std::cout << "New Player Change: " << json[UserName].string_value() << std::endl;
  	  strcpy(_PlayerTable[player.id].username, json[UserName].string_value().c_str());

  	  //Send player a table of players
  	  sendToClient(player, constructPlayerTable().c_str());

      //Create packet and send to everyone
      this->ServerTCP::Broadcast(UpdateID(_PlayerTable[player.id]).c_str());
      break;

    case PlayerLeftLobby:
      std::cout << "Player: " << json[PlayerID].int_value() << " has left the lobby" << std::endl;
      _PlayerTable.erase(json[PlayerID].int_value());
      this->ServerTCP::Broadcast(buffer);
      break;

    case GameStart:
      std::cout << "Player: " << json[PlayerID].int_value() << " has started the game" << std::endl;
      //All players in lobby are ready
      if (this->ServerTCP::AllPlayersReady())
      {
        this->ServerTCP::Broadcast(buffer);
        this->ServerTCP::Broadcast(generateMapSeed().c_str());
      }
      break;
    case GameEnd: //Currently allows any player to annouce the end of the game.
      std::cout << "Player: " << json[PlayerID].int_value() << " has ended the game" << std::endl;
      close(_UDPReceivingSocket);
      gameRunning = false;
      break;

    default:
      this->ServerTCP::Broadcast(buffer);
      break;
  }
}

/*------------------------------------------------------------------------------

  FUNCTION:                   AllPlayersReady

  DESIGNER/PROGRAMMER:        Gabriella Cheung

  REVISIONS:

  INTERFACE:                  bool ServerTCP::AllPlayersReady()

  RETURNS:                    if all the players were ready

  NOTES:

-------------------------------------------------------------------------------*/
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
/*------------------------------------------------------------------------------

  FUNCTION:                   constructPlayerTable

  DESIGNER/PROGRAMMER:        Martin Minkov, Scott Plummer, Jerry Jia

  REVISIONS:

  INTERFACE:                  std::string ServerTCP::constructPlayerTable()

  RETURNS:                    player table packet

  NOTES:
-------------------------------------------------------------------------------*/
std::string ServerTCP::constructPlayerTable()
{
	std::string packet = "[{\"DataType\" : 6, \"ID\" : 6, \"LobbyData\" : [";
	for (auto it = _PlayerTable.begin(); it != _PlayerTable.end();)
	{
		std::string tempUserName((it->second).username);
		packet += "{";
    packet += "\"PlayerID\" : " + std::to_string(it->first);
		packet += ",  \"UserName\" : \"" + tempUserName + "\"";
		packet += ", \"TeamID\": " +  std::to_string((it->second).team);
		packet += ", \"ClassID\" : " + std::to_string((it->second).playerClass);
		packet += ", \"Ready\" : " + std::to_string(Server::isReadyToInt(it->second));
		packet += (++it == _PlayerTable.end() ? "}" : "},");
	}
	packet +=    "]";
  packet += "}]";

	return packet;
}
/*------------------------------------------------------------------------------

  FUNCTION:                   UpdateID

  DESIGNER/PROGRAMMER:        Martin Minkov, Scott Plummer

  REVISIONS:

  INTERFACE:                  std::string ServerTCP::UpdateID(const Player& player)

  RETURNS:                    the players updated id

  NOTES:
-------------------------------------------------------------------------------*/
std::string ServerTCP::UpdateID(const Player& player)
{
   char buf[PACKETLEN];
   sprintf(buf, "[{\"DataType\" : 6, \"ID\" : 4, \"PlayerID\" : %d, \"UserName\" : \"%s\"}]", player.id, player.username);
   std::string temp(buf);
   return temp;
}
/*------------------------------------------------------------------------------

  FUNCTION:                   generateMapSeed

  DESIGNER/PROGRAMMER:        Dhivya Manohar

  REVISIONS:

  INTERFACE:                  std::string ServerTCP::generateMapSeed()

  RETURNS:                    The map seed

  NOTES:
-------------------------------------------------------------------------------*/

std::string ServerTCP::generateMapSeed()
{
	int mapSeed;
	srand (time (NULL));
	mapSeed = rand ();
	std::string packet = "[{\"DataType\" : 3, \"ID\" : 0, \"Seed\" : " + std::to_string(mapSeed) +"}]";
	return packet;
}
/*------------------------------------------------------------------------------

  FUNCTION:                   getPlayerTable

  DESIGNER/PROGRAMMER:        Gabriella Cheung

  REVISIONS:

  INTERFACE:                  std::map<int, Player> ServerTCP::getPlayerTable()

  RETURNS:                    the playerTable

  NOTES:
-------------------------------------------------------------------------------*/
std::map<int, Player> ServerTCP::getPlayerTable()
{
  return _PlayerTable;
}
/*------------------------------------------------------------------------------

  FUNCTION:                   getPlayerId

  DESIGNER/PROGRAMMER:        Gabriella Cheung

  REVISIONS:                  March 9, 2016 - Changed the socket be non-blocking

  INTERFACE:                  int ServerTCP::getPlayerId(std::string ipString)

  RETURNS:                    client ip code for player table

  NOTES:
-------------------------------------------------------------------------------*/
int ServerTCP::getPlayerId(std::string ipString)
{
  std::size_t index = ipString.find_last_of(".");
  return stoi(ipString.substr(index+1));
}

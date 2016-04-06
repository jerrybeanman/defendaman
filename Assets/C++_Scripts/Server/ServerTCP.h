#ifndef SERVER_TCP
#define SERVER_TCP
#include <sstream>      // std::istringstream
#include <arpa/inet.h>
#include <signal.h>
#include "Server.h"
#include "../Unity_Plugin/json11.hpp"

#define PlayerID    "PlayerID"
#define TeamID      "TeamID"
#define ClassID     "ClassID"
#define Ready       "Ready"
#define StartGame   "StartGame"
#define UserName    "UserName"

#define TEAMCODE 6

namespace Networking
{
	class ServerTCP : public Server
	{
		public:

			ServerTCP() {}
			~ServerTCP(){}

    	int InitializeSocket(short port) override;

      int Accept(Player * player);

      static void * CreateClientManager(void * server);

      void * Receive() override;

      void Broadcast(const char * message, sockaddr_in * excpt = NULL) override;

      void parseServerRequest(char* buffer, int& DataType, int& ID, int& IDValue, std::string& username);

			void CheckServerRequest(Player player, char * buffer);

			bool AllPlayersReady();

			std::string generateMapSeed();

			int getPlayerId(std::string ipString);

			std::map<int, Player> getPlayerTable();

    	std::string constructPlayerTable();

    	void sendToClient(Player player, const char * message);

    	std::string UpdateID(const Player& player);

    	void ShutDownGameServer(void);

		private:
			Player newPlayer;
			//Enum for the networking team to determine the type of message requested.
			enum DataType { Networking = 6 };
			enum LobbyCode
              {
                TeamChangeRequest   = 1,
                ClassChangeRequest  = 2,
                ReadyRequest        = 3,
                PlayerJoinedLobby   = 4,
                GameStart           = 5,
                UpdatePlayerList    = 6,
                PlayerLeftLobby     = 7,
								ChooseAman 				  = 8,
                GameEnd             = 10

              };
	    };
}

#endif

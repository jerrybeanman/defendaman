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

//TODO: Implement this instead of Networking enum
#define TEAMCODE 6

namespace Networking
{
	class ServerTCP : public Server
	{
		public:

			ServerTCP(int writeDes, int readDes) : Server(writeDes, readDes) {}
			~ServerTCP(){}

            int InitializeSocket(short port) override;

            int Accept(Player * player);

            static void * CreateClientManager(void * server);

            void * Receive() override;

            void Broadcast(const char * message, sockaddr_in * excpt = NULL) override;

			void PrepareSelect() override;

			int SetSocketOpt() override;

			void parseServerRequest(char* buffer, int& DataType, int& ID, int& IDValue, std::string& username);

			void CheckServerRequest(Player player, char * buffer);

			bool AllPlayersReady();

			std::string generateMapSeed();

			int getPlayerId(std::string ipString);

			std::map<int, Player> getPlayerTable();

            std::string constructPlayerTable();

            void sendToClient(Player player, const char * message);

            std::string UpdateID(const Player& player);

      /* Shuts down the game server */
      void ShutDownGameServer(void);

      void RestartServer(void); // PLACEHOLDER FOR RESTARTING THE TCP LOBBY

		private:
			Player newPlayer;
            int socketPair[2];
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
        GameEnd             = 8

      };
	};
}

#endif

#ifndef SERVER_UDP
#define SERVER_UDP
#include <sstream>      // std::istringstream
#include "Server.h"

namespace Networking
{
	class ServerUDP : public Server
	{
		public:
			ServerUDP() {};
			~ServerUDP() {}
    	/*
      Initialize socket, server address to lookup to, and connect to the server

      @return: socket file descriptor
      */
      int InitializeSocket(short port) override;

      /*
          Calls accept on a player's socket. Sets the returning socket and client address structure to the player.
          Add connected player to the list of players

          @return: socket file descriptor
      */
      void * Receive() override;

       /*
          Sends a message to all the clients
      */
      void Broadcast(const char* message, sockaddr_in * excpt = NULL) override;
			
			void SetPlayerList(std::map<int, Player> players);

			static void * CreateClientManager(void * server);

			void PrepareSelect() override;

			int SetSocketOpt() override;

	};
}

#endif

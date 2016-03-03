#ifndef SERVER_UDP
#define SERVER_UDP
#include <sstream>      // std::istringstream
#include "Server.h"
#include "ServerTCP.h"

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
            int InitializeSocket(short port);

            /*
                 Calls accept on a player's socket. Sets the returning socket and client address structure to the player.
                Add connected player to the list of players

                @return: socket file descriptor
            */
            int Receive();
	           /*
                Sends a message to all the clients
            */
            void Broadcast(char* message);

        private:
            /* List of players currently connected to the server */
            std::vector<Player>             _PlayerList;
	};
}

#endif

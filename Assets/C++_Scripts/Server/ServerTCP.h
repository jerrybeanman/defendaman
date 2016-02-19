#ifndef SERVER_TCP
#define SERVER_TCP
#include "Server.h"

namespace Networking
{
	class ServerTCP : public Server
	{
		public:
			ServerTCP(){}
			~ServerTCP(){}
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
            int Accept(Player * player);

            /*
                Creates a child process to handle incoming messages from new player that has just connected to the lobby

                @return: child PDI (0 for child process)
            */
            int CreateClientManager(int PlayerID);
            
           /*   
                Recieves data from child process that is dedicated for each player's socket

                @return: 1 on success, -1 on error, 0 on disconnect
            */
            int Receive(Player * player);

	        /*
                Sends a message to all the clients

            */
            virtual void Broadcast(char* message);

            void PrintPlayer(Player p);
        private:

            /* Team ONE - Player connections and info. */
            std::vector<Player>             _PlayerList;
	};
}

#endif
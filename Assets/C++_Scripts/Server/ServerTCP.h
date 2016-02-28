#ifndef SERVER_TCP
#define SERVER_TCP
#include <sstream>      // std::istringstream
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
                
                Interface:  int InitializeSocket(short port)
                            [port] Port number 

                programmer: Jerry Jia

                @return: socket file descriptor
            */
            int InitializeSocket(short port);

            /*
                Calls accept on a player's socket. Sets the returning socket and client address structure to the player.
                Add connected player to the list of players

                Interface:  int Accept(Player * player)
                            [player] Pointer to a Player structure

                programmer: Jerry Jia      

                return: socket file descriptor
            */
            int Accept(Player * player);

            /*
                Creates a child process to handle incoming messages from new player that has just connected to the lobby

                Interface:  void * CreateClientManager(void * server)
                            [server] Pointer to a void, which has to be a Server object

                Programmer: Jerry Jia       

                return: child PDI (0 for child process)
            */
            static void * CreateClientManager(void * server);

           /*
                Recieves data from child process that is dedicated for each player's socket

                Interface:  void * Receive()

                Programmer: Jerry Jia      

                @return: Thread execution code 
            */
          	void * Receive();

	        /*
                Sends a message to all the clients

                Interface:  void Broadcast(char * message)

                Programmer: Jerry Jia

                @return: void
            */
            void Broadcast(char * message);

        private:
            /* The recent player that has just been accepted */
			Player newPlayer;

			/* List of players currently connected to the server */
			std::vector<Player>             _PlayerList;

	};
}

#endif

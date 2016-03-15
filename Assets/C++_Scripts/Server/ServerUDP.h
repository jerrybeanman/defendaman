#ifndef SERVER_UDP
#define SERVER_UDP
#include <sstream>      // std::istringstream
#include "Server.h"

namespace Networking
{
	class ServerUDP : public Server
	{
		public:

			ServerUDP(int writeDes, int readDes) : Server(writeDes, readDes) {}
			~ServerUDP() {}

            int InitializeSocket(short port) override;

            void * Receive() override;

            void Broadcast(const char* message, sockaddr_in * excpt = NULL) override;
                    
            void SetPlayerList(std::map<int, Player> players);

            static void * CreateClientManager(void * server);

            void PrepareSelect() override;

            int SetSocketOpt() override;

	};
}

#endif

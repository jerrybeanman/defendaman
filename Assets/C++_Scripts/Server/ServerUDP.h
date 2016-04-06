#ifndef SERVER_UDP
#define SERVER_UDP
#include <sstream>      // std::istringstream
#include <signal.h>
#include "Server.h"

namespace Networking
{
	class ServerUDP : public Server
	{
		public:
	     ServerUDP() {}
		  ~ServerUDP() {}

			int InitializeSocket(short port) override;

      void * Receive() override;

			void Broadcast(const char* message, sockaddr_in * excpt = NULL) override;

      static void * CreateClientManager(void * server);

      void PrepareSelect();

      int SetSocketOpt();
	};
}

#endif

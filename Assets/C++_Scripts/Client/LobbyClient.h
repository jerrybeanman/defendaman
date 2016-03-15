#ifndef LOBBYCLIENT_H_
#define LOBBYCLIENT_H_
#include "Client.h"

namespace Networking
{
    class LobbyClient : public Client
    {
        public:
          //LobbyClient() {};

          //~LobbyClient() {};

          int Init_Client_Socket(const char* name, short port) override;

          void * Recv() override;

          int Send(char * message, int size) override;
    };
}
#endif

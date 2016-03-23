#ifndef GAMECLIENT_H_
#define GAMECLIENT_H_
#include "Client.h"

namespace Networking
{
    class GameClient : public Client
    {
        public:
          GameClient() {};
          ~GameClient() {};

          int Init_Client_Socket(const char* name, short port) override;

          void * Recv() override;

          int Send(char * message, int size) override;

          char* GetData();
    };
}
#endif

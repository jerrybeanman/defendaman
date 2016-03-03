#include "LobbyClient.h"
#include "GameClient.h"

using namespace Networking;

int main()
{
    //Change to either Lobby for TCP or Game for UDP
    Client * c = new LobbyClient();
    std::string teamRequest;
    char buf[32];
    int bytesRead;
    //Connect to requested IP address and port
    c->Init_Client_Socket("192.168.0.17", 7000);

    //PLEASE CHANGE YOUR SERVER'S IP HERE!
    while (getline(std::cin, teamRequest))
    {
      std::cout << "READ IN: " << teamRequest << std::endl;
      c->Send((char *)teamRequest.c_str(), teamRequest.size() + 1);
      c->Recv();
      printf(c->GetData());
    }
    return 0;
}

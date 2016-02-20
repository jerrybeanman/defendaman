#include "Server.h"
#include "ServerTCP.h"

using namespace Networking;


int main()
{
  int rc;
  ServerTCP TestServer;

  if((rc = TestServer.InitializeSocket(7000)) != 0)
  {
    std::cerr << "TCP Server initialization failed." << std::endl;
    return -1;
  }

  std::cerr << "TCP Server initialized." << std::endl;

  while(1) 
  {
      int PlayerID;
      struct Player player;
      if ((PlayerID = TestServer.Accept(&player)) == -1)
      {
        std::cerr << "rip.\n" << std::endl;
      }
      if(TestServer.CreateClientManager(PlayerID) == 0)
        break;
  }


  return 0;
}

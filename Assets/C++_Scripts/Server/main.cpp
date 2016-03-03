
#include "Server.h"
#include "ServerTCP.h"
#include "ServerUDP.h"

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
      /* assign it into the player object if we want to manipulate the thread */
      pthread_t readThread;

      int playerID;
      struct Player player;
      if ((playerID = TestServer.Accept(&player)) == -1)
      {
        std::cerr << "rip.\n" << std::endl;
      }

      /* Creates the thread to handle new clients */
      if(pthread_create(&readThread, NULL, &ServerTCP::CreateClientManager, (void *) &TestServer) < 0)
      {
        std::cerr << "thread creation failed" << std::endl;
      }
  }


  return 0;
}

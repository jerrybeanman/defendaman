/*------------------------------------------------------------------------------

  SOURCE FILE:                main.cpp

  PROGRAM:                    Server

  DESIGNER/PROGRAMMER:        Martin Minkov, Jerry Jia, Scott Plummer

  NOTES:                      Handles initializing the UDP and TCP sockets
                              and creating client and game threads.

-------------------------------------------------------------------------------*/
#include "Server.h"
#include "ServerTCP.h"
#include "ServerUDP.h"
using namespace Networking;

int rc;

int main()
{
  pthread_t udpThread;
  ServerTCP serverTCP;
  ServerUDP serverUDP;

  std::cerr << "UDP Server initialized." << std::endl;

  if((rc = serverTCP.InitializeSocket(7000)) != 0)
  {
    std::cerr << "TCP Server initialization failed." << std::endl;
    return -1;
  }

  std::cerr << "TCP Server initialized." << std::endl;

  while(1)
  {
      //assign it into the player object if we want to manipulate the thread
      pthread_t readThread;

      int playerID;
      struct Player player;
      if ((playerID = serverTCP.Accept(&player)) == -1)
      {
        std::cerr << "rip.\n" << std::endl;
      }

      /* Creates the thread to handle new clients */
      if(pthread_create(&readThread, NULL, &ServerTCP::CreateClientManager, (void *) &serverTCP) < 0)
      {
        std::cerr << "thread creation failed" << std::endl;
      }
      if (gameRunning == false)
      {
        if((rc = serverUDP.InitializeSocket(8000)) != 0)
        {
            std::cerr << "UDP Server initialization failed." << std::endl;
            return 1;
        }
        std::cout << "UDP thread creation" << std::endl;
        if(pthread_create(&udpThread, NULL, &ServerUDP::CreateClientManager, (void *) &serverUDP) < 0)
        {
            std::cerr << "thread creation failed" << std::endl;
        }
        gameRunning = true;
      }
  }
	return 0;
}

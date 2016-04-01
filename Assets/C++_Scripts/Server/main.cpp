#include "Server.h"
#include "ServerTCP.h"
#include "ServerUDP.h"
using namespace Networking;
std::map<int, Player>     _PlayerTable;
std::vector<std::string>  _Connections;
int rc;

int main()
{
  pthread_t udpThread;
  ServerTCP serverTCP;
  ServerUDP serverUDP;

  serverTCP.SetPlayerList(&_PlayerTable);
  serverUDP.SetPlayerList(&_PlayerTable);

  serverTCP.SetConnectionList(&_Connections);
  serverUDP.SetConnectionList(&_Connections);

  if((rc = serverUDP.InitializeSocket(8000)) != 0)
  {
      std::cerr << "UDP Server initialization failed." << std::endl;
      return 1;
  }
  std::cerr << "UDP Server initialized." << std::endl;
  if(pthread_create(&udpThread, NULL, &ServerUDP::CreateClientManager, (void *) &serverUDP) < 0)
  {
      std::cerr << "thread creation failed" << std::endl;
  }

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

      std::cerr << "Map size is: " << _PlayerTable.size() << std::endl;
      for(std::vector<std::string>::const_iterator it = _Connections.begin(); it != _Connections.end(); ++it)
      {
        std::cerr << "Player:" << *it << std::endl;
      }
  }
	return 0;
}

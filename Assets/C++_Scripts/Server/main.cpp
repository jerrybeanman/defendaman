
#include "Server.h"
#include "ServerTCP.h"
#include "ServerUDP.h"

using namespace Networking;

int main()
{
	int rc;
	ServerTCP TestServer;
	ServerUDP UDPServer;
/*
  if((rc = TestServer.InitializeSocket(7000)) != 0)
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
      if ((playerID = TestServer.Accept(&player)) == -1)
      {
        std::cerr << "rip.\n" << std::endl;
      }

      // Creates the thread to handle new clients
      if(pthread_create(&readThread, NULL, &ServerTCP::CreateClientManager, (void *) &TestServer) < 0)
      {
        std::cerr << "thread creation failed" << std::endl;
      }
  }
*/
/*
typedef struct Player
{
    int            socket;
    sockaddr_in    connection;
    int            id;
    char           username[32];
    int            team;
    int            playerClass;
    bool           isReady;
} Player;
*/
	/*Player p;

	p.socket = -1;
	p.isReady = false;
	p.id = 50;
	sprintf(p.username, "%s", "Jerry's bad");
	p.team = 1;
	p.playerClass = 1;

	players.push_back(p);*/
	//players.push_back(p);
/*
auto it = find(connections.begin(), connections.end(), inet_ntoa (Client.sin_addr));
	if(it ==  connections.end())
	{
		std::string temp(inet_ntoa (Client.sin_addr));		
		connections.push_back(temp);
		Player p;
		memcpy(&p.connection, &Client, sizeof(Client));
		_PlayerList.push_back(p);
	}
	std::cout << buf << std::endl;
*/

	if((rc = UDPServer.InitializeSocket(8000)) != 0)
	{
	  std::cerr << "UDP Server initialization failed." << std::endl;
	  return -1;
	}

	std::cerr << "UDP Server initialized." << std::endl;

	//UDPServer.SetPlayerList(players);


	//assign it into the player object if we want to manipulate the thread
	pthread_t readThread;

	// Creates the thread to handle new clients
	if(pthread_create(&readThread, NULL, &ServerUDP::CreateClientManager, (void *) &UDPServer) < 0)
	{
	std::cerr << "thread creation failed" << std::endl;
	}


	while(1)
	{
		//do nothing forever  
		;
	}

	return 0;
}

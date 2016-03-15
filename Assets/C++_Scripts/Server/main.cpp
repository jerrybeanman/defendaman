#include "Server.h"
#include "ServerTCP.h"
#include "ServerUDP.h"
using namespace Networking;

int rc;

void TestUDP(void); //Only called when UDPTEST is defined from the terminal

int main()
{
    //Socket Descriptors
    int fd[2];
    pid_t UDPChild;
	pthread_t udpThread;

    if (socketpair(AF_UNIX, SOCK_STREAM, 0, fd) < 0)
    {
        perror("Failed to create socket pairs"); 
        return 1;
    }
    ServerTCP serverTCP(fd[0], fd[1]);
    ServerUDP serverUDP(fd[0], fd[1]);

    if ((UDPChild = fork()) == -1)
    {
        perror ("Creating UDP process failed");
        return 1;
    }
    if (UDPChild == 0)
    {
        char buf[PACKETLEN];
        //Call some function
        if (read(fd[1], buf, PACKETLEN) < 0)
            perror("reading stream message");
        else
        {
            if((rc = serverUDP.InitializeSocket(8000)) != 0)
		    {
		    	std::cerr << "UDP Server initialization failed." << std::endl;
		    	return 1;
	    	}
	    	std::cerr << "UDP Server initialized." << std::endl;
            serverUDP.SetPlayerList(serverTCP.getPlayerTable());
            if(pthread_create(&udpThread, NULL, &ServerUDP::CreateClientManager, (void *) &serverUDP) < 0)
	    	{
	    		std::cerr << "thread creation failed" << std::endl;
	    	}
         }
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
  }
	return 0;
}

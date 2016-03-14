#include "Server.h"
#include "ServerTCP.h"
#include "ServerUDP.h"
using namespace Networking;

ServerTCP serverTCP;
ServerUDP serverUDP;
int rc;

void StartUDP(int signo);

void TestUDP(void); //Only called when UDPTEST is defined from the terminal

int main()
{

	#ifdef UDPTEST
		TestUDP(); //UDP testing ground ignoring TCP implementation
	#endif

	// Set signal handler for case
  // when game is ready to be started
  struct sigaction SigController;
  SigController.sa_handler = StartUDP;
  SigController.sa_flags = 0;
  sigemptyset( &SigController.sa_mask );
  sigaction(SIGTERM, &SigController, NULL );

  if((rc = serverTCP.InitializeSocket(7000)) != 0)
  {
    std::cerr << "TCP Server initialization failed." << std::endl;
    return -1;
  }

  std::cerr << "TCP Server initialized." << std::endl;

  while(1)
  {
      //assign it into the player object if we want to manipulate the thread
      pthread_t readThread, udpThread;

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

	/*
	Assuming that this thread goes on forever until the signal is caught
	where it starts the StartUDP function which actually begins the UDP.
	UDP will end when quit is done.
	*/

	return 0;
}

void StartUDP(int signo)
{
    std::string s;
    std::cout << "UDP server starting.." << std::endl;

		if((rc = UDPServer.InitializeSocket(8000)) != 0)
		{
			std::cerr << "UDP Server initialization failed." << std::endl;
			return -1;
		}

		std::cerr << "UDP Server initialized." << std::endl;

		// Creates the thread to handle new clients
		if(pthread_create(&udpThread, NULL, &ServerUDP::CreateClientManager, (void *) &UDPServer) < 0)
		{
			std::cerr << "thread creation failed" << std::endl;
		}

    while(getline(std::cin, s))
    {
			std::cout << "Command [" << s << "] acknowledged." << std::endl;
			if (s1.find(s2) != std::string::npos)
			{
        std::cout << "quitting" << '\n';
				break;
      }

    }

		exit(0);
}

//Called only when UDPTEST is defined
void TestUDP()
{
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
	if(pthread_create(&readThread, NULL, &ServerUDP::CreateClientManager, (void *) &serverUDP) < 0)
	{
	  std::cerr << "thread creation failed" << std::endl;
	}

	while(1)
	{
		//do nothing forever
		;
	}

	exit(0);
}

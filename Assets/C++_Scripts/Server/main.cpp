#include "Server.h"
#include "ServerTCP.h"
#include "ServerUDP.h"
using namespace Networking;

ServerTCP serverTCP;
ServerUDP serverUDP;

void StartUDP(int signo);

void * testThread(void * t)
{
    std::string s;
    getline(std::cin, s);
    kill(getpid(), SIGINT);
    return 0;
}

int main()
{
  // Set signal handler for case
  // when game is ready to be started
  struct sigaction SigController;
  SigController.sa_handler = StartUDP;
  SigController.sa_flags = 0;
  sigemptyset( &SigController.sa_mask );
  sigaction(SIGINT, &SigController, NULL );

  int rc;

  if((rc = serverTCP.InitializeSocket(7000)) != 0)
  {
    std::cerr << "TCP Server initialization failed." << std::endl;
    return -1;
  }
  pthread_t testThrea;
  pthread_create(&testThrea, NULL, &testThread ,0);

  std::cerr << "TCP Server initialized." << std::endl;

  while(1)
  {
      /* assign it into the player object if we want to manipulate the thread */
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

void StartUDP(int signo)
{
    std::string s;
    std::cout << "UDP server starting.." << std::endl;
    while(getline(std::cin, s))
    {
      std::cout << s <<std::endl;
    }
}
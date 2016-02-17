#include "Server.h"

using namespace Networking;

int main()
{
  int rc;
  std::string incPacket;

  std::cerr << "Go!" << std::endl;
  Server s;
  rc = s.Init_TCP_Server_Socket("", 5150);

  if(rc != 0)
  {
    std::cerr << "TCP Server initialization failed." << std::endl;
    return -1;
  }

  std::cerr << "TCP Server initialized." << std::endl;
  while(1) {
    if (s.TCP_Server_Accept() == 0)
    {
      std::cerr << "Client accepted." << std::endl;
    }
  }

/*
  Server udpServer;
  int udpSocket;
  udpSocket = udpServer.Init_UDP_Server_Socket("UDP server", 8000);
  if (udpSocket != 0)
  {
    std::cerr << "Initializing UDP socket failed." << std::endl;
    return -1;
  }

  while(1)
  {
    incPacket = udpServer.UDP_Server_Recv();
    if (!incPacket.empty())
    {
      std::cerr << "UDP packet received: " << incPacket << std::endl;
      udpServer.UDP_Server_Send(incPacket.c_str());
    }
  }
*/




  return 0;
}

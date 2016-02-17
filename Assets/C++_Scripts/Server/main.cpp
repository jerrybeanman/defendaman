#include "Server.h"

using namespace Networking;

int main()
{
  int rc;

  std::cerr << "Go!" << std::endl;
  Server s;
  rc = s.Init_TCP_Server_Socket("bananas", 5150);

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




  return 0;
}

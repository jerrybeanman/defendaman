#include "Client.h"

using namespace Networking;

int main()
{
    Client * c = new Client();
    std::string teamRequest;
    char buf[32];
    int bytesRead;
    //Connect to requested IP address and port
    c->Init_TCP_Client_Socket("192.168.0.4", 7000);

    //PLEASE CHANGE YOUR SERVER'S IP HERE!
    while (getline(std::cin, teamRequest))
    {
       c->Send((char *)teamRequest.c_str(), teamRequest.size() + 1);
       c->Recv(buf, PACKETLEN, &bytesRead);
       printf(c->GetData());
    }
    return 0;
}

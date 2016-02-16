#include "Client.h"

using namespace Networking;

int main()
{
    Client* c = new Client();
    std::string teamRequest;

    //Connect to requested IP address and port
    c->Init_TCP_Client_Socket("127.0.0.1", 7000);
    //PLEASE CHANGE YOUR SERVER'S IP HERE!
    while (getline(std::cin, teamRequest))
    {
       c->sendTeamRequest(teamRequest);
    } 
    return 0;
}

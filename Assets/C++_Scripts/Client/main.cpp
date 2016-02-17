#include "Client.h"

using namespace Networking;

int main()
{
    Client* c = new Client();
    std::string teamRequest;

    //Connect to requested IP address and port
<<<<<<< HEAD
    c->Init_TCP_Client_Socket("127.0.0.1", 7000);

    //PLEASE CHANGE YOUR SERVER'S IP HERE!
=======
>>>>>>> dcafbc81ee77e8b772bb2f2e21b55e8272d8bf0e
    c->Init_TCP_Client_Socket("127.0.0.1", 5150);
 
    while (getline(std::cin, teamRequest))
    {
       c->sendTeamRequest(teamRequest);
    } 
    return 0;
}

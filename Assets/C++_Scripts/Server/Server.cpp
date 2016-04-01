#include "Server.h"

using namespace Networking;
void Server::fatal(const char* error)
{
    std::cerr << error << std::endl;
}
int Server::isReadyToInt(Player player)
{
    if (player.isReady == true )
        return 1;

    if (player.isReady == false)
        return 0;

    //Checking failed
    return -1;
}

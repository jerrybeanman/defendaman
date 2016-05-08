/*------------------------------------------------------------------------------

  SOURCE FILE:                Server.cpp

  PROGRAM:                    Server

  FUNCTIONS:                  void Server::fatal(const char* error)
                              int Server::isReadyToInt(Player player)

  DESIGNER/PROGRAMMER:        Martin Minkov

  NOTES:                      The base class which ServerTCP and ServerUDP
                              are based off of.

-------------------------------------------------------------------------------*/
#include "Server.h"

using namespace Networking;

bool gameRunning = false;

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

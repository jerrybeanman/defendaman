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

/*
  Registers the passed in Player list as a class member to be used in broadcasting UDP packets.

  @Tyler Trepanier
*/
void Server::SetPlayerList(std::map<int, Player>* players)
{
  _PlayerTable = players;
}

/*
  Registers the passed in Player list as a class member to be used in broadcasting UDP packets.

  @Tyler Trepanier
*/
void Server::SetConnectionList(std::vector<std::string>* connections)
{
  _Connections = connections;
}

/**
 * [ServerTCP::getPlayerId description]
 * @author ???
 * @date   2016-03-11
 * @param  ipString   [description]
 * @return            [description]
 */
int Server::getPlayerId(std::string ipString)
{
  std::size_t index = ipString.find_last_of(".");
  return stoi(ipString.substr(index+1));
}

#include "plugin.h"

extern "C" int gettwo()
{
  int i;
  if((i = fork()) == 0)
  {
	while(1){}
  }
  return 2;
}


extern "C" PlayerStuff getPlayerStuff()
{
	PlayerStuff _playerStuff;
	_playerStuff.Dexterity = 5;
	_playerStuff.MaxHealth = 100;
	_playerStuff.IsHit = false;
	return _playerStuff;
}
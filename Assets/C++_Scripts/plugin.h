// basic file operations
#include <iostream>
#include <fstream>
#include <stdexcept>
#include <sys/types.h>
#include <unistd.h>
using namespace std;

extern "C"
{
	extern int gettwo();
}

/* testing out different primitive data types*/
extern "C" struct PlayerStuff
{
	int Dexterity;
	float MaxHealth;
	bool IsHit;
};
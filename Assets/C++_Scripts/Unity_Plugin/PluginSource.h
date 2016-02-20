// basic file operations
#include <iostream>
#include <fstream>
#include <stdexcept>
#include <sys/types.h>
#include <unistd.h>
#include <cstring>
#include "JSONParser.h"

using namespace std;

#define UP "W"
#define DOWN "S"
#define LEFT "A"
#define RIGHT "D"
#define LEFTCLICK "LC"
#define RIGHTCLICK "RC"

extern "C"
{
	extern char * receiveData();
}

/* testing out different primitive data types*/
extern "C" struct PlayerStuff
{
	int Dexterity;
	float MaxHealth;
	bool IsHit;
};

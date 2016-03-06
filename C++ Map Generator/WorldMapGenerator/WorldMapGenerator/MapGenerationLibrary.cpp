#define STRICT
#include <stdio.h>
#include <string>
#include "Map.h"

extern "C" const char * GenerateMap (int seed) {
	Map *map = new Map (50, 50, 24);
	map->mapSeed = seed;
	map->buildMapBase ();
	map->createBaseScenery ();
	map->joinMaps (map->getMapBase (), map->getMapTemp ());
	map->createSpawnPoints (map->getMapBase (), 2);
	return (map->ConvertToJSONString (map->getMapBase ()).c_str ());
}

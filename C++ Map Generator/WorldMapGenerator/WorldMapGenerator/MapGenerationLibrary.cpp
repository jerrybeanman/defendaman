#define STRICT
#include <stdio.h>
#include <string>
#include <string.h>
#include "Map.h"

char endMap[50000];

extern "C" const char * GenerateMap (int seed) {
	Map *map = new Map (100, 100, 44);
	map->randomizeSeed (seed);
	map->buildMapBase ();
	map->createBaseScenery ();
	map->joinMaps (map->getMapBase (), map->getMapTemp ());

	//first int is 1:N chance of creat
	map->createTopScenery (10);
	map->createResources (map->getMapBase (), map->getMapScenery(), 50);
	map->createSpawnPoints (map->getMapBase (), 2);
	strcpy_s (endMap, map->ConvertToJSONString ().c_str ());
	printf ("%d", strlen(endMap));
	return (endMap);
}
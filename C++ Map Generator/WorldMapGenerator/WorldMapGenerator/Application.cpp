#define STRICT
#include "Application.h"
#include <stdio.h>
#include <string>


void drawMap (int ** mapArray, Map map) {
	for (size_t x = 0; x < map.getMapWidth (); x++) {
		printf ("\n ");
		for (size_t y = 0; y < map.getMapHeight (); y++)
			if (map.isBaseWall(x,y))
				printf ("  ");
			else
				printf ("%d ", mapArray[x][y] - map.baseSceneryDefault);
	}
}

int main () {
	char c;
	Map *map = new Map (50, 50, 24);
	do {
		map->randomizeSeed ();
		map->buildMapBase ();
		map->createBaseScenery ();
		map->joinMaps (map->getMapBase (), map->getMapTemp ());
		map->createSpawnPoints (map->getMapBase (), 2);
		printf("%s", map->ConvertToJSONString (map->getMapBase ()).c_str());
		//drawMap (map->getMapBase (), *map);
		printf ("\n\nEnter 'q' to quit, any other key will regernate.\n\n");
		c = getchar ();
		Map* map2 = new Map (100, 100, 44);
		map = map2;
	} while (c != 'q');
	return 0;
}



const char * GenerateMap (int seed) {
	Map *map = new Map (50, 50, 24);
	map->mapSeed = seed;
	map->buildMapBase ();
	map->createBaseScenery ();
	map->joinMaps (map->getMapBase (), map->getMapTemp ());
	map->createSpawnPoints (map->getMapBase (), 2);
	return (map->ConvertToJSONString (map->getMapBase ()).c_str ());
}

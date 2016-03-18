#define STRICT
#include "Application.h"
#include <stdio.h>
#include <string>
#include <iostream>
#include <fstream>


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
	Map *map = new Map (100, 100, 24);
	do {
		map->randomizeSeed();
		map->buildMapBase ();
		map->createBaseScenery ();
		map->joinMaps (map->getMapBase (), map->getMapTemp ());
		map->createTopScenery (12);
		map->createSpawnPoints (map->getMapBase (), 2);
		//printf("%s", map->ConvertToJSONString ().c_str());
		std::ofstream myfile;
		myfile.open ("output.txt");
		myfile << map->ConvertToJSONString ().c_str ();
		myfile.close ();
		//drawMap (map->getMapBase (), *map);
		printf ("\n\nEnter 'q' to quit, any other key will regernate.\n\n");
		c = getchar ();
		Map* map2 = new Map (10, 10, 24);
		map = map2;
	} while (c != 'q');
	return 0;
}

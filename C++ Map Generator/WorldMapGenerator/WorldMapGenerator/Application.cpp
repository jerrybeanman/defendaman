#define STRICT
#include "Application.h"
#include <stdio.h>
#include <string>
#include <iostream>
#include <fstream>

/*--------------------------------------------------------------------------------------------  
--  SOURCE:          Application.cpp
--  
--  PROGRAM:         Testing File
--  
--  FUNCTIONS:       void drawMap (int ** mapArray, Map map);
--                   
--                   int main ();
--  
--  DATE:            Testing File
--  
--  DESIGNERS:       Jaegar Sarauer
--  
--  REVISIONS:       NONE
--  
--  PROGRAMMERS:     Jaegar Sarauer
--  
--  NOTES:           This file utilizes the Map.cpp library to create a map and display it in command
--                   prompt for testing/display.
------------------------------------------------------------------------------------------*/


/*--------------------------------------------------------------------------------------------  
--  FUNCTION:        drawMap
--  
--  DATE:            April 13th, 2016
--  
--  DESIGNERS:       Jaegar Sarauer
--  
--  REVISIONS:       NONE
--  
--  PROGRAMMERS:     Jaegar Sarauer
--  
--  INTERFACE:       void drawMap (int ** mapArray, Map map);
--  
--  RETURNS:         void
--  
--  NOTES:           This function takes in a map layer and the map object and creates a map out
--                   of ascii characters in command prompt. The Map object is needed to determine
--                   the size of the map and what tiles indicate as what.
------------------------------------------------------------------------------------------*/
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


/*--------------------------------------------------------------------------------------------  
--  FUNCTION:        main
--  
--  DATE:            April 13th, 2016
--  
--  DESIGNERS:       Jaegar Sarauer
--  
--  REVISIONS:       NONE
--  
--  PROGRAMMERS:     Jaegar Sarauer
--  
--  INTERFACE:       int main ();
--  
--  RETURNS:         int
--  
--  NOTES:           This function constructs a map and draws it in command prompt.
------------------------------------------------------------------------------------------*/
int main () {
	char c;
	Map *map = new Map (100, 100, 44);
	do {
		map->randomizeSeed();
		map->buildMapBase ();
		map->createBaseScenery ();
		map->joinMaps (map->getMapBase (), map->getMapTemp ());
		map->createTopScenery (12);
		map->createResources (map->getMapBase (), map->getMapScenery(), 50);
		map->createSpawnPoints (map->getMapBase (), 2);
		//printf("%s", map->ConvertToJSONString ().c_str());
		std::ofstream myfile;
		myfile.open ("output.txt");
		myfile << map->ConvertToJSONString ().c_str ();
		myfile.close ();
		drawMap (map->getMapBase (), *map);
		printf ("\n\nEnter 'q' to quit, any other key will regernate.\n\n");
		c = getchar ();
		Map* map2 = new Map (10, 10, 24);
		map = map2;
	} while (c != 'q');
	return 0;
}
/*
char endMap[100000];

const char * GenerateMap (int seed) {
	Map *map = new Map (100, 100, 44);
	map->randomizeSeed (seed);
	map->buildMapBase ();
	map->createBaseScenery ();
	map->joinMaps (map->getMapBase (), map->getMapTemp ());

	//first int is 1:N chance of creat
	map->createTopScenery (10);
	map->createResources (map->getMapBase (), map->getMapScenery (), 50);
	map->createSpawnPoints (map->getMapBase (), 2);
	strcpy_s (endMap, map->ConvertToJSONString ().c_str ());
	printf ("%d", strlen (endMap));
	return (endMap);
}

int main () {
	char c;
	do {
		GenerateMap (1000);
		c = getchar ();
	} while (c != 'q');
	return 0;
}

*/

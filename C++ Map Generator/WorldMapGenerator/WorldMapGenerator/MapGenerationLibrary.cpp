#define STRICT
#include <stdio.h>
#include <string>
#include <string.h>
#include "Map.h"

char endMap[50000];


/*--------------------------------------------------------------------------------------------  
--  FUNCTION:        GenerateMap
--  
--  DATE:            April 13th, 2016
--  
--  DESIGNERS:       Jaegar Sarauer
--  
--  REVISIONS:       NONE
--  
--  PROGRAMMERS:     Jaegar Sarauer
--  
--  INTERFACE:       extern "C" const char * GenerateMap (int seed)
--                   	int seed = The seed to build the randomness of the map off of.
--  
--  RETURNS:         extern "C" const char * = String in JSON format indicating the map.
--  
--  NOTES:           This function is used to create a map and return a JSON string based off of a 
--                   map seed, allowing multiple clients to create the same map without passing the 
--                   data of the actual map, as long as they have just the single integer which represents
--                   the seed.
--                   More parameters may be added for more customization of the map, however
--                   support for this was never added on the game side so there was no need.
------------------------------------------------------------------------------------------*/
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
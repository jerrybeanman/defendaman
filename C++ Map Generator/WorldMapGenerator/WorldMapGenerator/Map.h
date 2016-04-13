#pragma once
#define _CRT_SECURE_NO_WARNINGS

#include <cstdlib>
#include <time.h> 
#include <cmath>
#include <stdio.h>
#include <string>
#include <deque>
#include <vector>
#include "AStarPoint.h"


/*--------------------------------------------------------------------------------------------  
--  SOURCE:          Map
--  
--  PROGRAM:         class Map
--  
--  FUNCTIONS:       int getMapWidth ();
--                   
--                   int getMapHeight ();
--                   
--                   int ** getMapBase ();
--                   
--                   int ** getMapScenery ();
--                   
--                   int ** getMapTemp ();
--                   
--                   void buildMapBase ();
--                   
--                   int randomizeSeed ();
--                   
--                   int randomizeSeed (int seed);
--                   
--                   int getMapSeed ();
--                   
--                   void smoothMap ();
--                   
--                   int getNeighbourConditional (int xPos, int yPos, int (Map::*condition)(int, int, int, int));
--                   int baseWallConditional (int neighXPos, int neighYPos, int myXPos, int myYPos);
--                   int surroundingHeightsCount (int neighXPos, int neighYPos, int myXPos, int myYPos);
--                   
--                   void createRandomBaseWalls ();
--                   
--                   bool isBaseWall (int xPos, int yPos);
--                   
--                   void resetMap (int ** map, const int value);
--                   
--                   void createBaseScenery ();
--                   
--                   void createTopScenery (int sceneryChance);
--                   
--                   void drawMap (int ** mapArray);
--                   
--                   void joinMaps (int ** baseMap, int ** topMap);
--                   
--                   std::string ConvertToJSONString ();
--                   
--                   std::string intToString (int n);
--                   
--                   void createSpawnPoints (int ** map, int teams);
--                   
--                   bool validateSpawns (int ** map, int ** spawnPoints, int teams);
--                   
--                   std::deque<AStarPoint> aStarPath (int startX, int startY, int endX, int endY);
--                   
--                   void createResources (int ** mapCheck, int ** mapApply, int resourceAmount);
--  
--  DATE:            class Map
--  
--  DESIGNERS:       Jaegar Sarauer
--  
--  REVISIONS:       NONE
--  
--  PROGRAMMERS:     Jaegar Sarauer
--  
--  NOTES:           This Map class is a library which allows creation of a random map of specified
--                   size which may be based off of an initial seed which allows the creation of several
--                   maps locally to match other clients without communicating more than the inital
--                   seed.
--                   The library handles creating basic walkable/non-walkable tiles smoothed to
--                   create obstacles and caves, scenery and resource spawning, elevation heights,
--                   A* pathfinding to test possible access to points, spawn points, and exporting the
--                   map in JSON.
------------------------------------------------------------------------------------------*/
class Map {
	public:
	Map (int w = 50, int h = 50, int wallChance = 50) : mapWidth (w), 
														mapHeight (h), 
														baseWallChance(wallChance) {
		mapBase = new int*[mapWidth];
		for (int i = 0; i < mapWidth; ++i)
			mapBase[i] = new int[mapHeight];

		mapScenery = new int*[mapWidth];
		for (int i = 0; i < mapWidth; ++i)
			mapScenery[i] = new int[mapHeight];

		tempLayer = new int*[mapWidth];
		for (int i = 0; i < mapWidth; ++i)
			tempLayer[i] = new int[mapHeight];

		resetMap (tempLayer, -1);
	};

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        getMapWidth
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       int Map::getMapWidth()
	--  
	--  RETURNS:         int = The amount of tiles which make up the width of the board.
	--  
	--  NOTES:           This function is a getter for the amount of tiles which stretch across 
	--                   the width of the map.
	------------------------------------------------------------------------------------------*/
	int getMapWidth ();
	
	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        getMapHeight
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       int Map::getMapHeight()
	--  
	--  RETURNS:         int = The amount of tiles which make up the height of the board.
	--  
	--  NOTES:           This function is a getter for the amount of tiles which stretch across 
	--                   the height of the map.
	------------------------------------------------------------------------------------------*/
	int getMapHeight ();

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        getMapBase
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       int ** Map::getMapBase()
	--  
	--  RETURNS:         int ** = A 2D array of ids which make up the map.
	--  
	--  NOTES:           This function is a getter for the 2d array of ids which indicate which each tile on
	--                   the map represents.
	------------------------------------------------------------------------------------------*/
	int ** getMapBase ();

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        getMapScenery
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       int** Map::getMapScenery()
	--  
	--  RETURNS:         int ** = A 2D array of ids which make up the map on the scenery layer.
	--  
	--  NOTES:           This function is a getter for the 2d array of ids which indicate which each tile on
	--                   the scenery layer of the map represents. These tiles will overlay the base of the
	--                   map with just images, they have no effect on the gameplay.
	------------------------------------------------------------------------------------------*/
	int ** getMapScenery ();

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        getMapTemp
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       int** Map::getMapTemp()
	--  
	--  RETURNS:         int ** = A 2D array of ids which make up a temporary map.
	--  
	--  NOTES:           This function is a getter for the 2d array of ids which indicate which each tile on
	--                   the temporary layer. The temporary layer is used for applying calculations to a 
	--                   copy of a final map, without changing the actual map. It is intended that this map
	--                   is applied to another map to apply any changes.
	------------------------------------------------------------------------------------------*/
	int ** getMapTemp ();

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        buildMapBase
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       int Map::buildMapBase()
	--  
	--  RETURNS:         VOID
	--  
	--  NOTES:           This function will add wall or ground tiles at random on the base map
	--                   then attempt to smooth them out. The for loop executes 5 times, however
	--                   this can be changed to give the map a smoother or rougher style.
	------------------------------------------------------------------------------------------*/
	void buildMapBase ();

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        randomizeSeed
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       int Map::randomizeSeed()
	--  
	--  RETURNS:         int = The seed which all rand() functions will be randomized off of.
	--  
	--  NOTES:           This function calls srand, which randomizes the seed in order to allow 
	--                   rand() calls to all be based on the same seed.
	------------------------------------------------------------------------------------------*/
	int randomizeSeed ();

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        randomizeSeed
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       int Map::randomizeSeed(int seed)
	--                   	seed = The seed to assign as the seed for rand() calls.
	--  
	--  RETURNS:         int = The seed which all rand() functions will be randomized off of.
	--  
	--  NOTES:           This function calls srand with a set seed as an integer, to ensure all
	--                   clients will have the same result from rand() calls, as they can potentially
	--                   share the same seed.
	--                   It is inteded that in order to syncronize the clients with the same map, that
	--                   each client gets the same seed from the server when generating the map.
	------------------------------------------------------------------------------------------*/
	int randomizeSeed (int seed);

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        getMapSeed
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       int Map::getMapSeed()
	--  
	--  RETURNS:         int = The seed which all rand() functions will be randomized off of.
	--  
	--  NOTES:           This function returns the value of the seed which all rand() functions 
	--                   will be using.
	------------------------------------------------------------------------------------------*/
	int getMapSeed ();

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        smoothMap
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       int Map::smoothMap()
	--  
	--  RETURNS:         VOID
	--  
	--  NOTES:           This function will check all neighbouring ID's around each tile on the
	--                   map base and determine if it should become a wall or a walkable ground
	--                   tile.
	--                   The purpose of this function is to be called several times until the map begins
	--                   to form what seems like small mountains and fields.
	------------------------------------------------------------------------------------------*/
	void smoothMap ();

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        getNeighbourConditional
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       int Map::getNeighbourConditional (int xPos, int yPos, int (Map::*condition)(int, int, int, int))
	--                   	int xPos = The x position of the tile on the map.
	--                   	int yPos = The y position of the tile on the map.
	--                   	int (Map::*condition)(int, int, int, int)
	--                   		= The comparison function to compare neighbouring
	--                   		  tiles to.
	--  
	--  RETURNS:         int = The amount of successful comparisons.
	--  
	--  NOTES:           This function checks all tiles around a specified tile against a conditional
	--                   check. 
	--                   The purpose of this function is for easily checking all tiles of any map, 
	--                   against a comparison function for neighboor tiles.
	--                   Example: Detecting how many wall tiles are around the current tile.
	------------------------------------------------------------------------------------------*/
	int getNeighbourConditional (int xPos, int yPos, int (Map::*condition)(int, int, int, int));
	
	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        baseWallConditional
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       int Map::baseWallConditional (int neighXPos, int neighYPos, int myXPos, int myYPos)
	--                   	int neighXPos = X position of the neighbooring tile.
	--                   	int neighYPos = Y position of the neighbooring tile.
	--                   	int myXPos = X position of the checking tile.
	--                   	int myYPos = Y position of the checking tile.
	--  
	--  RETURNS:         int = The result of the calculation.
	--  
	--  NOTES:           This function is a conditional function to be used in the Map::getNeighbourConditional
	--                   function. 
	--                   This function will ensure comparisons will be within the bounds of the map, and
	--                   the neighboor isn't the same tile as the comparison function.
	--                   It will return a one if the neighboor is a wall tile, or if the neighboor is out of
	--                   bounds of the map. It will return 0 if it is another walkable tile.
	------------------------------------------------------------------------------------------*/
	int baseWallConditional (int neighXPos, int neighYPos, int myXPos, int myYPos);
	
	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        surroundingHeightsCount
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       int Map::surroundingHeightsCount (int neighXPos, int neighYPos, int myXPos, int myYPos) 
	--                   	int neighXPos = X position of the neighbooring tile.
	--                   	int neighYPos = Y position of the neighbooring tile.
	--                   	int myXPos = X position of the checking tile.
	--                   	int myYPos = Y position of the checking tile.
	--  
	--  RETURNS:         int = The result of the calculation.
	--  
	--  NOTES:           This function is a conditional function to be used in the Map::getNeighbourConditional
	--                   function. 
	--                   This function will ensure comparisons will be within the bounds of the map, and
	--                   the neighboor isn't the same tile as the comparison function.
	--                   It will return a one if the neighboor height is higher than the current tile, and
	--                   return 0 if it isn't.
	------------------------------------------------------------------------------------------*/
	int surroundingHeightsCount (int neighXPos, int neighYPos, int myXPos, int myYPos);
	
	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        createRandomBaseWalls
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       void Map::createRandomBaseWalls ()
	--  
	--  RETURNS:         void
	--  
	--  NOTES:           Creates the base layer of the map. It will create a border of walls around the
	--                   outer edge, and add walls at random to the map. The remaining tiles will be
	--                   walkable ground.
	--                   The chance of walls spawning is controlled by int baseWallChance, which
	--                   must range from 0 to 100, where 100 is a 100% chance of a wall spawning.
	--                   Chaning this value will affect the amount of mountains and obsticles which the
	--                   map will consist of after other generation is applied to them.
	------------------------------------------------------------------------------------------*/
	void createRandomBaseWalls ();

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        isBaseWall
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       bool Map::isBaseWall (int xPos, int yPos)
	--                   	int xPos = The x position of the tile on the map.
	--                   	int yPos = The y position of the tile on the map.
	--  
	--  RETURNS:         bool = Is this tile a wall or a ground tile.
	--  
	--  NOTES:           The purpose of this function is to determine if a specific tile on the map base
	--                   is a wall or a ground tile after layers of elevation have been applied to the map.
	--                   The reason for this function is to check the range of id's which make up the walls
	--                   and grass, and determine the tile based off of the types of ids. 
	--                   Essentially a wrapper function in case the id's which make up the map change,
	--                   this function will be the only thing that'll have to change for wall/ground comparisons.
	------------------------------------------------------------------------------------------*/
	bool isBaseWall (int xPos, int yPos);

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        resetMap
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       void Map::resetMap (int ** map, const int value)
	--                   	int ** map = The map to reset.
	--                   	const int value = The value to set to each id of the map (usually -1).
	--  
	--  RETURNS:         void
	--  
	--  NOTES:           This function will reset the map which is passed in.
	--                   All id's of the map will be replaced with the value of the second parameter.
	------------------------------------------------------------------------------------------*/
	void resetMap (int ** map, const int value);

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        createBaseScenery
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       void Map::createBaseScenery ()
	--  
	--  RETURNS:         void
	--  
	--  NOTES:           This will create a set amount if points to originate grass, and a size of
	--                   origination. then fan out with a random diffusion level until it meets its
	--                   same value. Imagine an elevation map with the id's of the tile meaning
	--                   the height.
	--                   The purpose of this is to give the effects of different colored grass on the
	--                   walkable portions of the map.
	------------------------------------------------------------------------------------------*/
	void createBaseScenery ();

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        createTopScenery
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       void Map::createTopScenery (int sceneryChance)
	--                   	int sceneryChance = The chance of spawning scenery.
	--                   		The chance of spawning is 1:sceneryChance + 1.
	--  
	--  RETURNS:         void
	--  
	--  NOTES:           The function will loop through the base layer of the map and check for 
	--                   walkable tiles. All walkable tiles will have a 1:sceneryChance + 1 in spawning
	--                   a scenery object on the scenery layer of the map (unless there is a neighbooring
	--                   scenery object).
	------------------------------------------------------------------------------------------*/
	void createTopScenery (int sceneryChance);

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        drawMap
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       void Map::drawMap (int ** mapArray)
	--                   	int ** mapArray = A 2D array of ints which represent tile id's of a map.
	--  
	--  RETURNS:         void
	--  
	--  NOTES:           This function will draw a map layer (passed in as a parameter) as ascii characters
	--                   which are represented from the tile id's of the map array.
	--                   Mainly used for debugging.
	------------------------------------------------------------------------------------------*/
	void drawMap (int ** mapArray);

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        joinMaps
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       void Map::joinMaps (int ** baseMap, int ** topMap)
	--                   	int ** baseMap = 2D map that will inherit the changes.
	--                   	int ** topMap = 2D map that will be applied to the baseMap.
	--  
	--  RETURNS:         void
	--  
	--  NOTES:           This function will join the id's of the map tiles of two map layers by adding
	--                   the id values for the corrosponding coordinates together.
	------------------------------------------------------------------------------------------*/
	void joinMaps (int ** baseMap, int ** topMap);

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        ConvertToJSONString
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       std::string Map::ConvertToJSONString ()
	--  
	--  RETURNS:         std::string = A JSON string formatted to send as a packet.
	--  
	--  NOTES:           This function parses all necessary data into a JSON string which can be read
	--                   by the recieving client to populate the map layers, scenery objects, and map width
	--                   and height.
	--                   The parsed data is created as one string to be sent by the server.
	------------------------------------------------------------------------------------------*/
	std::string ConvertToJSONString ();

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        intToString
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       std::string Map::intToString (int n)
	--                   	int n = The int to convert.
	--  
	--  RETURNS:         std::string = The integer in string format.
	--  
	--  NOTES:           This function converts an integer to a string and returns the string.
	------------------------------------------------------------------------------------------*/
	std::string intToString (int n);

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        createSpawnPoints
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       void Map::createSpawnPoints (int ** map, int teams)
	--                   	int ** map = The map to check for available spawn points.
	--                   	int teams = The amount of spawn points to create.
	--  
	--  RETURNS:         void
	--  
	--  NOTES:           This function creates spawn points at random places on the map. The spawn
	--                   points are then tested with A* pathfinding in order to find a path to all other
	--                   spawn points. The A* testing is to ensure all spawn points can reach eachother
	--                   in order for the game to be playable.
	--                   Along with testing to see if a path to other spawns are possible, distance from
	--                   each spawn point is also checked to ensure spawn points aren't right next to
	--                   eachother.
	--                   This function is not constraint tested as it should be, and adding too many points
	--                   may create an infinate loop in attempting to create spawn points which pass
	--                   the constraints.
	------------------------------------------------------------------------------------------*/
	void createSpawnPoints (int ** map, int teams);

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        validateSpawns
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       bool Map::validateSpawns (int ** map, int ** spawnPoints, int teams)
	--                   	int ** map = The map to check for walls and free space.
	--                   	int ** spawnPoints = A list of x and y points indicating spawn points.
	--                   	int teams = The amount of teams to check.
	--                   	
	--  
	--  RETURNS:         bool = Are all spawns valid?
	--  
	--  NOTES:           This function will attempt to caluclate if the passed in spawn points are able to
	--                   access eachother determined using A* pathfinding to ensure this is a playable map.
	--                   All untested paths from one spawn point to another is tested with A*, if the
	--                   A* path has nodes in it, then the path was successful and this function will
	--                   return true.
	------------------------------------------------------------------------------------------*/
	bool validateSpawns (int ** map, int ** spawnPoints, int teams);

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        aStarPath
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       std::deque<AStarPoint> Map::aStarPath(int startX, int startY, int endX, int endY)
	--                   	int startX = The start point on the X axis.
	--                   	int startY = The start point on the Y axis.
	--                   	int endX = The end point on the X axis.
	--                   	int endY = The end point on the Y axis.
	--  
	--  RETURNS:         std::deque<AStarPoint> = A list of AStarPoints (Nodes) on path travel histroy.
	--  
	--  NOTES:           This function computes the A* pathfinding algorithm to attempt to find a path from
	--                   startX,Y to endX,Y position. 
	--                   The function works by adding the start position to the open list of points, checking
	--                   the closest point in the open list, adding a neghboor to the open list which has the
	--                   best movement score, and adding the current tested point to the closed list.
	--                   Points will be updated if a better path has been found while testing, and the closed
	--                   list will have a more direct path.
	--                   The pathfinding ends when a node successfully reaches the end point, or the open
	--                   list is completely empty (meaning there are no points left to move to). When there
	--                   are no points left to move to, a list of 0 nodes is returned meaning the pathfinding
	--                   failed to find a successful path.
	------------------------------------------------------------------------------------------*/
	std::deque<AStarPoint> aStarPath (int startX, int startY, int endX, int endY);

	/*--------------------------------------------------------------------------------------------  
	--  FUNCTION:        createResources
	--  
	--  DATE:            April 7th, 2016
	--  
	--  DESIGNERS:       Jaegar Sarauer
	--  
	--  REVISIONS:       NONE
	--  
	--  PROGRAMMERS:     Jaegar Sarauer
	--  
	--  INTERFACE:       void Map::createResources (int ** mapCheck, int ** mapApply, int resourceAmount)
	--                   	int ** mapCheck = The map to check for free spots.
	--                   	int ** mapApply = The map to apply the resources to.
	--                   	int resourceAmount = The amount of resources to attempt to create.
	--  
	--  RETURNS:         void
	--  
	--  NOTES:           This function will attempt to create resources and populate them into the 
	--                   mapResources vector to store the x and y positions of where the resources
	--                   should spawn after the map is created.
	--                   Resources are used in the map as an interactable object which drop items for
	--                   the players to use as a currency to build buildings, buy items for boosting the
	--                   player, and purchasing upgrades on player weapons.
	--                   Resources are not generated to the map as they may spawn in different areas
	--                   after they have been destroyed by a player, thus making them not an actual part
	--                   of the map. 
	--                   Resources attempt to spawn at a random place on the map. If the spot is not 
	--                   at the spot of a wall or where another resource is already, it will be created.
	--                   If not, the attempt to spawn this iteration of a resource is decreased by one,
	--                   to a maximum attempt of 5 times.
	--                   Resources are meant to be spawned on the client side of the game with the
	--                   data generated here.
	------------------------------------------------------------------------------------------*/
	void createResources (int ** mapCheck, int ** mapApply, int resourceAmount);

	~Map () {
		delete[] mapBase;
		delete[] tempLayer;
	}


	//values for what the map id represenets
	const int baseWallDefault = 0;
	const int baseWallMax = 99;
	const int baseSceneryDefault = 100;
	const int baseSceneryMax = 199;
	const int spawnPointID = 200;
	const int sceneryDefault = 201;
	const int sceneryMax = 214;
	int mapSeed;

	private:
	int mapWidth;
	int mapHeight;
	int ** mapBase;
	int ** mapScenery;
	std::vector<std::pair<int, int>> mapResources;
	int ** tempLayer;
	int baseWallChance;
	int * objectIDList;

	//typedef int (Map::*baseWall) (int neighXPos, int neighYPos, int myXPos, int myYPos);
	//baseWall baseWallCondition;
	
};
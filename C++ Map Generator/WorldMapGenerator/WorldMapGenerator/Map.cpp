#include "Map.h"

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
int Map::getMapWidth () {
	return mapWidth;
};

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
int Map::getMapHeight () {
	return mapHeight;
};


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
int ** Map::getMapBase () {
	return mapBase;
}

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
int ** Map::getMapScenery () {
	return mapScenery;
}

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
int** Map::getMapTemp () {
	return tempLayer;
}

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
int Map::getMapSeed () {
	return mapSeed;
}

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
int Map::randomizeSeed () {
	srand (time (NULL));
	mapSeed = rand ();
	return mapSeed;
}

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
int Map::randomizeSeed (int seed) {
	srand (seed);
	mapSeed = rand ();
	return mapSeed;
}

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
void Map::smoothMap () {
	int thisNeighbours;
	for (int x = 0; x < mapWidth; x++)
		for (int y = 0; y < mapHeight; y++) {
			thisNeighbours = getNeighbourConditional (x, y, &Map::baseWallConditional);
			if (thisNeighbours < 4)
				mapBase[x][y] = baseSceneryDefault;
			else if (thisNeighbours > 4)
				mapBase[x][y] = baseWallDefault;
		}
}


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
void Map::buildMapBase () {
	createRandomBaseWalls ();

	//increase this to increase smoothing amount, 5 looks good though.
	for (int i = 0; i < 5; i++)
		smoothMap ();
}

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
int Map::getNeighbourConditional (int xPos, int yPos, int (Map::*condition)(int, int, int, int)) {
	int neighbourCount = 0;
	for (int x = xPos - 1; x <= xPos + 1; x++)
		for (int y = yPos - 1; y <= yPos + 1; y++)
			neighbourCount += (this->*condition) (x, y, xPos, yPos);
	return neighbourCount;
}

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
void Map::createRandomBaseWalls () {
	for (int x = 0; x < mapWidth; x++)
		for (int y = 0; y < mapHeight; y++) {
			if (x == 0 || x == mapWidth - 1 || y == 0 || y == mapHeight - 1) {
				mapBase[x][y] = baseWallDefault;
				continue;
			}
			mapBase[x][y] = (rand() % 100 <= baseWallChance) ? baseWallDefault : baseSceneryDefault;
		}
}


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
bool Map::isBaseWall (int xPos, int yPos) {
	return mapBase[xPos][yPos] <= baseWallMax;
}


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
void Map::resetMap (int ** map, const int value) {
	for (int x = 0; x < mapWidth; x++)
		for (int y = 0; y < mapHeight; y++)
			map[x][y] = value;
}

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
void Map::createBaseScenery () {
	//how many origins of grass there will be
	int origins = 3;

	//size of the initial dot 
	int size = (rand () % 4) + 1;

	//amount of random paths to walk out from origin to make origin point
	int walks = 0;

	int tmpSize = size;
	while (tmpSize > 0)
		walks += 4 * tmpSize--;

	//every n cycles, the value of the grass will decrease
	int diffusion = 2;

	//this 2d array is a result of one cycle, it is saved to the tmp array after each process
	int ** localTemp = new int*[mapWidth];
	for (int i = 0; i < mapWidth; ++i)
		localTemp[i] = new int[mapHeight];
	resetMap (localTemp, 0);

	//When a -1 is read as a value anywhere on the map, it isnt done elevation calculations
	bool flagIsNull = true;

	for (int i = 0; i < origins; i++) {
		//pick a random point to place the grass origin
		int x = rand () % mapWidth;
		int y = rand () % mapHeight;
		tempLayer[x][y] = 1;

	}

	while (flagIsNull) {
		flagIsNull = false;
		for (int x = 0; x < mapWidth; x++)
			for (int y = 0; y < mapHeight; y++)
				if (getNeighbourConditional (x, y, &Map::surroundingHeightsCount) >= localTemp[x][y]) {
					localTemp[x][y] += (rand () % 10 <= getNeighbourConditional (x, y, &Map::surroundingHeightsCount)) ? 1 : 0;
				}

		for (int x = 0; x < mapWidth; x++)
			for (int y = 0; y < mapHeight; y++) {
				tempLayer[x][y] += localTemp[x][y];
				if (tempLayer[x][y] == -1)
					flagIsNull = true;
			}
		//drawMap (tempLayer);
		resetMap (localTemp, 0);
	}



	delete[] localTemp;
	

}


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
void Map::createTopScenery (int sceneryChance) {
	resetMap (mapScenery, -1);

	for (int x = 0; x < mapWidth; x++)
		for (int y = 0; y < mapHeight; y++) {
			if (mapBase[x][y] > baseWallMax)
				if (rand () % sceneryChance == 0)
					if (x - 1 >= 0 && x + 1 < mapWidth && y - 1 >= 0 && y + 1 < mapHeight)
						for (int tX = x - 1; tX <= x + 1; tX++) {
							for (int tY = y - 1; tY <= y + 1; tY++) {
								if (mapScenery[tX][tY] != -1)
									break;
								if (tX == x + 1 && tY == y + 1)
									mapScenery[x][y] = rand () % (sceneryMax - sceneryDefault);
							}
						}
		}
}

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
int Map::surroundingHeightsCount (int neighXPos, int neighYPos, int myXPos, int myYPos) {
	//neighbour testing within the bounds of the map
	if (neighXPos >= 0 && neighXPos < mapWidth && neighYPos >= 0 && neighYPos < mapHeight) {
		//make sure were not selecting the spot that was passed to us
		if (neighXPos != myXPos || neighYPos != myYPos) {
			if (tempLayer[neighXPos][neighYPos] >= tempLayer[myXPos][myYPos])
				return 1;
		}
	}
	return 0;
}


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
int Map::baseWallConditional (int neighXPos, int neighYPos, int myXPos, int myYPos) {
	//neighbour testing within the bounds of the map
	if (neighXPos >= 0 && neighXPos < mapWidth && neighYPos >= 0 && neighYPos < mapHeight) {
		//make sure were not selecting the spot that was passed to us
		if (neighXPos != myXPos || neighYPos != myYPos) {
			if (isBaseWall (neighXPos, neighYPos))
				return 1;
		}
	} else {
		return 1;
	}
	return 0;
}

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
void Map::drawMap (int ** mapArray) {
	printf ("\n\n -------------------------------------------------------------- \n\n");
	for (size_t x = 0; x < getMapWidth (); x++) {
		printf ("\n ");
		for (size_t y = 0; y < getMapHeight (); y++) {
			printf ("%-4d ", mapArray[x][y]);
		}
		/*if (map.isBaseWall(x,y))
		printf ("W ");
		else
		printf ("  ");*/
	}
}


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
void Map::joinMaps (int ** baseMap, int ** topMap) {
	for (int x = 0; x < mapWidth; x++)
		for (int y = 0; y < mapHeight; y++)
			if (baseMap[x][y] >= baseSceneryDefault)
				baseMap[x][y] += topMap[x][y];

}


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
void Map::createResources (int ** mapCheck, int ** mapApply, int resourceAmount) {
	int tmpX, tmpY, tries = 0;
	for (int i = 0; i < resourceAmount; i++) {
		while (tries++ < 5) {
			tmpX = rand () % mapWidth;
			tmpY = rand () % mapHeight;
			if (mapCheck[tmpX][tmpY] >= baseSceneryDefault && mapCheck[tmpX][tmpY] <= baseSceneryMax) {
				for (auto it = mapResources.begin (); it != mapResources.end (); it++)
					if (it->first == tmpX && it->second == tmpY)
						goto FAIL;
				mapResources.push_back (std::pair<int, int> (tmpX, tmpY));
				goto NEXT;
			}
			FAIL: tries = tries;
		}
		NEXT:tries = 0;
	}
}

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
std::string Map::ConvertToJSONString () {
	std::string JSONString ("[");
	JSONString.append ("{");
	JSONString.append ("DataType:3,");
	JSONString.append ("ID:0,");
	JSONString.append ("\"mapWidth\":" + intToString (mapWidth) + ",");
	JSONString.append ("\"mapHeight\":" + intToString (mapHeight) + ",");
	JSONString.append ("\"mapIDs\":[");
	for (int x = 0; x < mapWidth; x++) {
		JSONString += "[";
		for (int y = 0; y < mapHeight; y++) {
			JSONString += intToString(mapBase[x][y]);
			if (y < mapWidth - 1)
				JSONString += ",";
		}
		JSONString += "]";
		if (x < mapHeight - 1)
			JSONString += ",";
	}
	JSONString += "],";
	JSONString.append ("\"mapSceneryIDs\":[");
	for (int x = 0; x < mapWidth; x++) {
		JSONString += "[";
		for (int y = 0; y < mapHeight; y++) {
			JSONString += intToString (mapScenery[x][y]);
			if (y < mapWidth - 1)
				JSONString += ",";
		}
		JSONString += "]";
		if (x < mapHeight - 1)
			JSONString += ",";
	}
	JSONString += "],";
	JSONString.append ("\"mapResources\":[");
	for (auto it = mapResources.begin (); it != mapResources.end (); it++) {
		JSONString += "[";
		JSONString += intToString (it->first) + "," + intToString (it->second);
		JSONString += "]";
		if (it != mapResources.end() - 1)
			JSONString += ",";
	}
	JSONString += "]";
	JSONString += "}";
	JSONString += "]";
	return JSONString;
}


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
void Map::createSpawnPoints (int ** map, int teams) {
	int ** spawnPoints = new int*[2];
	spawnPoints[0] = new int[teams];
	spawnPoints[1] = new int[teams];
	do {
		for (int i = 0; i < teams; i++) {
			//pick a place to place the generation spawn
			int x = rand () % mapWidth;
			int y = rand () % mapHeight;
			if (map[x][y] >= baseSceneryDefault && map[x][y] <= baseSceneryMax) {
				spawnPoints[0][i] = x;
				spawnPoints[1][i] = y;
			} else {
				//placing a spot has failed, retry
				i--;
				continue;
			}
		}
	} while (!validateSpawns (map, spawnPoints, teams));
	for (int i = 0; i < teams; i++) {
		mapScenery[spawnPoints[0][i]][spawnPoints[1][i]] = spawnPointID;
	}
}

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
bool Map::validateSpawns (int ** map, int ** spawnPoints, int teams) {
	const int moveCost = 10;
	const int diagonalMoveCost = 14;

	//for each team that exists
	for (int i = 0; i < teams; i++) {
		//the x and y position of the team currently path finding from
		int thisTeamX = spawnPoints[0][i];
		int thisTeamY = spawnPoints[1][i];
		//for the team that is testing, find a path to all of the other teams
		for (int j = i + 1; j < teams; j++) {
			//the x and y position of the team currently path finding to
			int goalTeamX = spawnPoints[0][j];
			int goalTeamY = spawnPoints[1][j];

			//find the heuristic values for all points on the map to the goal
			if (aStarPath (thisTeamX, thisTeamY, goalTeamX, goalTeamY).size () == 0)
				return false;
		}
	}
	return true;
}

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
std::deque<AStarPoint> Map::aStarPath(int startX, int startY, int endX, int endY) {
	const int vertical = 10;
	const int diagonal = 14;
	// The set of nodes already evaluated.
	std::deque<AStarPoint> closedSet;
	// The set of currently discovered nodes still to be evaluated.
	// Initially, only the start node is known.
	std::deque<AStarPoint> openSet;

	//this set is returned if the path is impossible
	std::deque<AStarPoint> wrongSet;

	int tmpF, tmpG, tmpH;
	int totalLength = std::abs (startX - startX) + std::abs (startY - startY);
	int lengths = std::abs (std::abs (startX - startX) - std::abs (startY - startY));
	tmpG = (lengths * vertical) + (((totalLength - lengths) / 2) * diagonal);
	totalLength = std::abs (startX - endX) + std::abs (startY - endY);
	lengths = std::abs (std::abs (startX - endX) - std::abs (startY - endY));
	tmpH = (lengths * vertical) + (((totalLength - lengths) / 2) * diagonal);
	tmpF = tmpG + tmpH;

	//if the distance from one point to another is 25% of the shortest
	//of width or height of the map, it is regarded as too close and is 
	//returned as a bad path
	if (tmpF < vertical * std::fmin (mapWidth, mapHeight) / 4)
		return wrongSet;

	openSet.push_back (AStarPoint(startX, startY, tmpF, tmpG, tmpH));

	AStarPoint *inOperation = new AStarPoint(0,0);
	AStarPoint *tmpPoints[9];// = (AStarPoint*)malloc (sizeof (AStarPoint) * 8);

	while (openSet.size () > 0) {
		int lowestF = mapHeight * mapWidth * diagonal;
		int index = 0, smallest = -1;
		for (auto it = openSet.begin (); it != openSet.end (); it++) {
			if (it->F < lowestF) {
				lowestF = it->F;
				smallest = index;
			}
			index++;
		}

		if (smallest != -1) {
			closedSet.push_back (openSet[smallest]);
			*inOperation = closedSet.back ();
			openSet.erase (openSet.begin () + smallest);
		}

		int tmpIndex = 0;
		for (int locX = inOperation->X - 1; locX <= inOperation->X + 1; locX++) {
			for (int locY = inOperation->Y - 1; locY <= inOperation->Y + 1; locY++) {
				if (locX == inOperation->X && locY == inOperation->Y || (mapBase[locX][locY] >= baseWallDefault && mapBase[locX][locY] <= baseWallMax)) {
					tmpPoints[tmpIndex++] = new AStarPoint (-1, -1);
					continue;
				}

				if (locX >= 0 && locX < mapWidth && locY >= 0 && locY < mapHeight) {
					int totalLength = std::abs (startX - locX) + std::abs (startY - locY);
					int lengths = std::abs (std::abs (startX - locX) - std::abs (startY - locY));
					tmpG = (lengths * vertical) + (((totalLength - lengths) / 2) * diagonal);
					totalLength = std::abs (locX - endX) + std::abs (locY - endY);
					lengths = std::abs (std::abs (locX - endX) - std::abs (locY - endY));
					tmpH = (lengths * vertical) + (((totalLength - lengths) / 2) * diagonal);
					tmpF = tmpG + tmpH;
					tmpPoints[tmpIndex++] = new AStarPoint (locX, locY, tmpF, tmpG, tmpH, inOperation);
					if (locX == endX && locY == endY) {
						closedSet.push_back (*tmpPoints[--tmpIndex]);
						//delete[] tmpPoints;
						return closedSet;
					}
				} else {
					tmpPoints[tmpIndex++] = new AStarPoint (-1, -1);
				}
			}
		}
		tmpIndex = 0;
		for (int i = 0; i < 8; i++) {
				if (tmpPoints[i]->X >= 0 && tmpPoints[i]->X < mapWidth && tmpPoints[i]->Y >= 0 && tmpPoints[i]->Y < mapHeight) {

					for (auto it = openSet.begin (); it != openSet.end (); it++) {
						if (it->X == tmpPoints[tmpIndex]->X && it->Y == tmpPoints[tmpIndex]->Y) {
							if (it->F <= tmpPoints[tmpIndex]->F) {
								goto END;
							} else {
								it->F = tmpPoints[tmpIndex]->F;
								it->X = tmpPoints[tmpIndex]->X;
								it->Y = tmpPoints[tmpIndex]->Y;
								it->parent = tmpPoints[tmpIndex]->parent;
								goto END;
							}
						}
					}
					for (auto itt = closedSet.begin (); itt != closedSet.end (); itt++) {
						if (itt->X == tmpPoints[tmpIndex]->X && itt->Y == tmpPoints[tmpIndex]->Y) {
							if (itt->F <= tmpPoints[tmpIndex]->F) {
								goto END;
							} else {
								itt->F = tmpPoints[tmpIndex]->F;
								itt->X = tmpPoints[tmpIndex]->X;
								itt->Y = tmpPoints[tmpIndex]->Y;
								itt->parent = tmpPoints[tmpIndex]->parent;
								goto END;
							}
						}
					}
					openSet.push_back (*tmpPoints[tmpIndex]);
					
				}
				END:tmpIndex++;
		}
	}
	//delete[] tmpPoints;
	return wrongSet;
}


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
std::string Map::intToString (int n) {
	char temp[16];
	sprintf (temp, "%d", n);
	return std::string (temp);
}
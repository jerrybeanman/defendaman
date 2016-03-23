#include "Map.h"

int Map::getMapWidth () {
	return mapWidth;
};

int Map::getMapHeight () {
	return mapHeight;
};

int** Map::getMapBase () {
	return mapBase;
}

int ** Map::getMapScenery () {
	return mapScenery;
}

int** Map::getMapTemp () {
	return tempLayer;
}

int Map::getMapSeed () {
	return mapSeed;
}

int Map::randomizeSeed () {
	srand (time (NULL));
	mapSeed = rand ();
	return mapSeed;
}

int Map::randomizeSeed (int seed) {
	srand (seed);
	mapSeed = rand ();
	return mapSeed;
}

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



void Map::buildMapBase () {
	createRandomBaseWalls ();

	//increase this to increase smoothing amount, 5 looks good though.
	for (int i = 0; i < 5; i++)
		smoothMap ();
}

int Map::getNeighbourConditional (int xPos, int yPos, int (Map::*condition)(int, int, int, int)) {
	int neighbourCount = 0;
	for (int x = xPos - 1; x <= xPos + 1; x++)
		for (int y = yPos - 1; y <= yPos + 1; y++)
			neighbourCount += (this->*condition) (x, y, xPos, yPos);
	return neighbourCount;
}

/*
Creates a 2D grid based on the int mapSize.
It assigns indexes to each spot on the array, which will be
used to create actual map tiles.
*/
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


/*
Function tells us if the object id at this position is a collidable wall,
or if its a visual asset.
*/
bool Map::isBaseWall (int xPos, int yPos) {
	return mapBase[xPos][yPos] <= baseWallMax;
}

void Map::resetMap (int ** map, const int value) {
	for (int x = 0; x < mapWidth; x++)
		for (int y = 0; y < mapHeight; y++)
			map[x][y] = value;
}

/*
This will create a set amount if points to originate grass, and a size of
origination. then fan out with a random diffusion level until it meets its
same value.
(Imagine an elevation map).
*/
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

void Map::joinMaps (int ** baseMap, int ** topMap) {
	for (int x = 0; x < mapWidth; x++)
		for (int y = 0; y < mapHeight; y++)
			if (baseMap[x][y] >= baseSceneryDefault)
				baseMap[x][y] += topMap[x][y];

}

//... requests a list of 0 or more parameters of pairs, which contain <string, void*>
std::string Map::ConvertToJSONString () {
	std::string JSONString ("[");
	JSONString.append ("{ ");
	JSONString.append ("DataType : 3, ");
	JSONString.append ("ID : 0, ");
	JSONString.append ("\"mapWidth\" : " + intToString (mapWidth) + ", ");
	JSONString.append ("\"mapHeight\" : " + intToString (mapHeight) + ", ");
	JSONString.append ("\"mapIDs\" : [");
	for (int x = 0; x < mapWidth; x++) {
		JSONString += "[";
		for (int y = 0; y < mapHeight; y++) {
			JSONString += intToString(mapBase[x][y]);
			if (y < mapWidth - 1)
				JSONString += ", ";
		}
		JSONString += "]";
		if (x < mapHeight - 1)
			JSONString += ", ";
	}
	JSONString += "], ";
	JSONString.append ("\"mapSceneryIDs\" : [");
	for (int x = 0; x < mapWidth; x++) {
		JSONString += "[";
		for (int y = 0; y < mapHeight; y++) {
			JSONString += intToString (mapScenery[x][y]);
			if (y < mapWidth - 1)
				JSONString += ", ";
		}
		JSONString += "]";
		if (x < mapHeight - 1)
			JSONString += ", ";
	}
	JSONString += "]";
	JSONString += "}";
	JSONString += "]";
	return JSONString;
}

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
		map[spawnPoints[0][i]][spawnPoints[1][i]] = spawnPointID;
	}
}

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
	if (tmpF < vertical * std::fmin (mapWidth, mapHeight) / 3)
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
			/*find the node with the least f on the open list, call it "q"
			pop q off the open list
			generate q's 8 successors and set their parents to q
			for each successor
				if successor is the goal, stop the search
					successor.g = q.g + distance between successor and q
					successor.h = distance from goal to successor
					successor.f = successor.g + successor.h

					if a node with the same position as successor is in the OPEN list \
						which has a lower f than successor, skip this successor
						if a node with the same position as successor is in the CLOSED list \
							which has a lower f than successor, skip this successor
							otherwise, add the node to the open list
							end
							push q on the closed list
							end
}

void aStarFinal (cameFrom, current)
total_path : = [current]
	while current in cameFrom.Keys :
		current : = cameFrom[current]
		total_path.append (current)
		return total_path
		*/



std::string Map::intToString (int n) {
	char temp[16];
	sprintf (temp, "%d", n);
	return std::string (temp);
}
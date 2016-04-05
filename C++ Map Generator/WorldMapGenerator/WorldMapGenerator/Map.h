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

	int getMapWidth ();

	int getMapHeight ();

	int ** getMapBase ();

	int ** getMapScenery ();

	int ** getMapTemp ();

	void buildMapBase ();

	int randomizeSeed ();

	int randomizeSeed (int seed);

	int getMapSeed ();

	void smoothMap ();

	int getNeighbourConditional (int xPos, int yPos, int (Map::*condition)(int, int, int, int));
	int baseWallConditional (int neighXPos, int neighYPos, int myXPos, int myYPos);
	int surroundingHeightsCount (int neighXPos, int neighYPos, int myXPos, int myYPos);

	void createRandomBaseWalls ();

	bool isBaseWall (int xPos, int yPos);

	void resetMap (int ** map, const int value);

	void createBaseScenery ();

	void createTopScenery (int sceneryChance);

	void drawMap (int ** mapArray);

	void joinMaps (int ** baseMap, int ** topMap);

	std::string ConvertToJSONString ();

	std::string intToString (int n);

	void createSpawnPoints (int ** map, int teams);

	bool validateSpawns (int ** map, int ** spawnPoints, int teams);

	std::deque<AStarPoint> aStarPath (int startX, int startY, int endX, int endY);

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
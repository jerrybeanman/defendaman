#pragma once
#define _CRT_SECURE_NO_WARNINGS

#include <cstdlib>
#include <time.h> 
#include <cmath>
#include <stdio.h>
#include <string>

class Map {
	public:
	Map (int w = 50, int h = 50, int wallChance = 50) : mapWidth (w), 
														mapHeight (h), 
														baseWallChance(wallChance) {
		mapBase = new int*[mapWidth];
		for (int i = 0; i < mapWidth; ++i)
			mapBase[i] = new int[mapHeight];

		tempLayer = new int*[mapWidth];
		for (int i = 0; i < mapWidth; ++i)
			tempLayer[i] = new int[mapHeight];

		/*
		This is a list of object ids that will be possible to be generated at the beginning of the game.
		Expect these ID's to start at 0 and go to the end of the list. The top value, of the baseSceneryMax
		will be added to them.
		This will cause index 0 of the objectIDList to start at 199, and increasing from there.
		*/
		objectIDList = new int[1];
		for (int i = 1; i <= 1; i++)
		objectIDList[i - 1] = i + baseSceneryMax; //Spawn point for a team.

		resetMap (tempLayer, -1);
	};

	int getMapWidth ();

	int getMapHeight ();

	int ** getMapBase ();

	int ** getMapTemp ();

	void buildMapBase ();

	int randomizeSeed ();

	int getMapSeed ();

	void smoothMap ();

	int getNeighbourConditional (int xPos, int yPos, int (Map::*condition)(int, int, int, int));
	int baseWallConditional (int neighXPos, int neighYPos, int myXPos, int myYPos);
	int surroundingHeightsCount (int neighXPos, int neighYPos, int myXPos, int myYPos);

	void createRandomBaseWalls ();

	bool isBaseWall (int xPos, int yPos);

	void resetMap (int ** map, const int value);

	void createBaseScenery ();

	void drawMap (int ** mapArray);

	void joinMaps (int ** baseMap, int ** topMap);

	std::string ConvertToJSONString (int ** map);

	std::string intToString (int n);

	void createSpawnPoints (int ** map, int teams);

	void validateSpawns (int ** map, int ** spawnPoints, int teams);

	~Map () {
		delete[] mapBase;
		delete[] tempLayer;
	}


	//values for what the map id represenets
	const int baseWallDefault = 0;
	const int baseWallMax = 99;
	const int baseSceneryDefault = 100;
	const int baseSceneryMax = 199;

	private:
	int mapWidth;
	int mapHeight;
	int mapSeed;
	int ** mapBase;
	int ** tempLayer;
	int baseWallChance;
	int * objectIDList;

	//typedef int (Map::*baseWall) (int neighXPos, int neighYPos, int myXPos, int myYPos);
	//baseWall baseWallCondition;
	
};
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {


	//MapSprites mp = GameObject.AddComponent<MapSprites> as MapSprites;
	//public MapSprites mp;
	public GameObject tile;
	public GameObject obstacle;
	public List<Sprite> mapSprites;
    /*
    The amount of blocks that are drawn along the width of the map.
    */
    
	public int mapWidth;

    /*
    The amount of blocks that are drawn along the height of the map.
    */
     public int mapHeight;
    
    /*
    The size of each tile that make up the map.
    */
    public int mapTileSize;

    /*
    A 2d grid of tiles which make up the map.
    */
    static public int[,] tileGrid;

    /*
    Should the map be generated with a random seed?
    */
    public bool randomSeed;

    /*
    The seed to use to randomize the map
    */
    public int seed;

    /*
    The chance that there will be a wall randomly created
    */
    [Range(0, 100)]
    public int wallCreationChance;

	// Use this for initialization
	void Start () {
		Debug.Log("run pls");
        tileGrid = new int[mapWidth, mapHeight];
        createTileGrid(tileGrid);
        for (int i = 0; i < 5; i++)
            smoothGrid();
		placeSprites ();
        //we would draw heres if we don't use gizmos, which auto draw by unity library
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void smoothGrid() {
        int thisNeighbours;
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++) {
                thisNeighbours = getNeighbouringEdges(x, y);
                if (thisNeighbours < 4)
                    tileGrid[x, y] = 0;
                else if (thisNeighbours > 4)
                    tileGrid[x, y] = 1;
            }
    }

    int getNeighbouringEdges(int xPos, int yPos) {
        int neighbourCount = 0;
        for (int x = xPos - 1; x <= xPos + 1; x++)
            for (int y = yPos - 1; y <= yPos + 1; y++)
                //neighbour testing within the bounds of the map
                if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight) {
                    //make sure were not selecting the spot that was passed to us
                    if (x != xPos || y != yPos) {
                        neighbourCount += tileGrid[x, y];
                    }
                } else {
                    neighbourCount++;
                }

        return neighbourCount;
    }

    /*
    Creates a 2D grid based on the int mapSize.
    It assigns indexes to each spot on the array, which will be
    used to create actual map tiles.
    */
    void createTileGrid(int[,] grid) {
		int rand;
        if (randomSeed)
            seed = (int)System.DateTime.Now.Ticks;

        Debug.Log(seed);

        System.Random randomHash = new System.Random(seed.GetHashCode());

        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++) {
                if (x == 0 || x == mapWidth - 1 || y == 0 || y == mapHeight - 1) {
                    grid[x, y] = 1;
                    continue;
                }
			rand=randomHash.Next(0, 100);
                grid[x, y] = (randomHash.Next(0, 100) <= wallCreationChance) ? 1 : 0;
            }
    }

	/*------------------------------------------------------------------------------------------------------------------
	-- FUNCTION: placeSprites
	--
	-- DATE: February 19, 2016
	--
	-- REVISIONS: N/A
	--
	-- DESIGNER: Thomas Yu
	--
	-- PROGRAMMER: Thomas Yu
	--
	-- INTERFACE: void placeSprites()
	--
	-- RETURNS: void.
	--
	-- NOTES:
	-- This function places sprites on the terrain. Currently it checks the value of the 2d array and if the array is 0, it randomly generates
	-- one of two types of grass. If the array value is 1, it randomly generates oneof two types of water. This will be refactored so that all
	-- there will be a different value in the array for each sprite. It also generates a 2collision box on the water tiles so thatr objects
	-- cannot enter it.
----------------------------------------------------------------------------------------------------------------------*/
	void placeSprites(){
		int randomnum=0;
		if (tileGrid == null)
			return;
		for (int x = 0; x < mapWidth; x++) {
			for (int y = 0; y < mapHeight; y++) {
				randomnum=(int)Random.Range(0,2);
				//If the 2D array is land
				if (tileGrid [x, y] == 1)
				{
					//Generate grass 1
					if(randomnum==0)
						tile.GetComponent<SpriteRenderer>().sprite = mapSprites [0];
					else
						//Generate Grass 2
						tile.GetComponent<SpriteRenderer>().sprite = mapSprites [1];
					//Place tile on terrain.
					Instantiate (tile, new Vector3 (x,y), Quaternion.identity);
				}
				//If the 2D array is water
				else {
					//Generate water1
					if(randomnum==0)
					{
						obstacle.GetComponent<SpriteRenderer>().sprite = mapSprites [2];
					}
					//Generate water2
					else 
						obstacle.GetComponent<SpriteRenderer>().sprite = mapSprites [3];
					//Place tile on terrain.
					Instantiate (obstacle, new Vector3 (x,y), Quaternion.identity);
				}
			}

		}

	}
	/*
	void OnDrawGizmos() {
			if (tileGrid == null)
				return;
			for (int x = 0; x < mapWidth; x++)
			for (int y = 0; y < mapHeight; y++) {
				Gizmos.color = (tileGrid[x, y] == 1) ? Color.black : Color.white;
				Vector3 pos = new Vector3(-mapWidth / 2 + x + .5f, 0, -mapHeight / 2 + y + .5f);
				Gizmos.DrawCube(pos, Vector3.one);
			}
		}
		*/
		/*
    void OnDrawGizmos() {
		SpriteRenderer tile;

        if (tileGrid == null)
            return;
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++) {
			if(tileGrid[x, y] == 1)
				tile.sprite = mp.mapSprites[0];
			else 
				tile.sprite = mp.mapSprites[1];
			Instantiate(tile,new Vector3(-mapWidth / 2 + x + .5f, 0, -mapHeight / 2 + y + .5f), Quaternion.identity);
                Gizmos.color = (tileGrid[x, y] == 1) ? Color.black : Color.white;
                Vector3 pos = new Vector3(-mapWidth / 2 + x + .5f, 0, -mapHeight / 2 + y + .5f);
                Gizmos.DrawCube(pos, Vector3.one);
            }
    }
    */

}

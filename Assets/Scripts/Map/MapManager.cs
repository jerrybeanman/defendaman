using UnityEngine;
using System.Collections;
using SimpleJSON;
using System;
using System.Collections.Generic;

/*-----------------------------------------------------------------------------
-- MapManager.cs - Script attached to MapManager game object responsible for
--                 handling map events such as map instantiation, resource 
--                 management, and object pooling.
--
-- FUNCTIONS:
--      void Start()
--      void Update()
--      void HandleEvent(int id, JSONClass message)
--      void CreateMap(JSONClass message)
--      void InstantiateResources(JSONClass message)
--      void DrawMap()
--      void ProcessResourceTakenEvent(JSONClass message)
--      void ProcessResourceDepletedEvent(JSONClass message)
--      void ProcessResourceRespawnEvent(JSONClass message)
--      IEnumerator ExplodeAndDestroy(GameObject go)
--      IEnumerator RespawnAfterTime(GameObject go, int seconds, int amount)
--      void InstantiatePool()
--      void CheckObjectPool()
--
-- DATE:		February 16, 2016
-- REVISIONS:	March 31 - Add resource event handling
--              April 2  - Refactor to use static constants from Globals.cs 
-- DESIGNER:	Jaegar Sarauer, Krystle Bulalakaw, Thomas Yu
-- PROGRAMMER:  Jaegar Sarauer, Krystle Bulalakaw, Thomas Yu
-----------------------------------------------------------------------------*/
public class MapManager : MonoBehaviour {
    // How many walls will line the outside to not give an image of an empty world around the edges 
    const int OUTERWALL_THICKNESS = 24; // (Camera size + gunnerclass.cs max zoom out)/2
    const int RESPAWN_TIME = 30;        //  in seconds
    const int RESOURCE_Z = -2;
    const int RESOURCE_HP = 5;
    const int TOTAL_GOLD = 100;
    const int GOLD_TO_DROP = TOTAL_GOLD / RESOURCE_HP;

    // Map update event types
    public enum EventType 
    {
        CREATE_MAP = 0,
        RESOURCE_TAKEN = 1,
        RESOURCE_DEPLETED = 2,
        RESOURCE_RESPAWN = 3
    };

    // Map values
    private int[,] _map;
    private int[,] _mapScenery;
    private int _mapWidth;
    private int _mapHeight;

    // Map tiles and scenery
    public GameObject _tile;
    public GameObject _scenery;
    public GameObject _obstacle;
    public List<Sprite> _mapSolids;
    public List<Sprite> _mapWalkable;
    public List<Sprite> _mapSceneryObjects;

    // Resource Objects, Game Objects, Sprites
    private List<GameObject> _mapResources = new List<GameObject>();
    public GameObject mapResource;
    public List<Sprite> _resourceSprites;

    //variables used for buildings
    public List<GameObject> buildingsCreated;
    public List<Vector2>  wallList;
    public List<Vector2>  ArmoryList;
    Vector3 lastFramePosition;
    Vector3 dragStartPosition;
    int overlayFlag = 1;

    // Object Pooling
    private GameObject[] _pooledObjects;
    public Camera mainCamera;
    public Vector3 cameraPosition;
    public static float cameraDistance;
    public float frustumHeight, frustumWidth;

    // Sound components
    public AudioSource audioSource;
    public AudioClip audioExplode;


    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION:    Start
    -- DATE:        February 16, 2016
    -- REVISIONS:   N/A
    -- DESIGNER:    Krystle Bulalakaw
    -- PROGRAMMER:  Krystle Bulalakaw
    -- INTERFACE:   Start(void)
    -- RETURNS:     void.
    -- NOTES:
    -- Initializes the game object.
    ----------------------------------------------------------------------------------------------------------------------*/
    void Start() {
        // Add sound component for explosion
        audioSource = (AudioSource) gameObject.AddComponent<AudioSource>();
        audioExplode = Resources.Load ("Music/Tree/pop") as AudioClip;
    }

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION:    Update
    -- DATE:        February 16, 2016
    -- REVISIONS:   N/A
    -- DESIGNER:    Krystle Bulalakaw
    -- PROGRAMMER:  Krystle Bulalakaw
    -- INTERFACE:   Update(void)
    -- RETURNS:     void.
    -- NOTES:
    -- Called once per frame.
    -- Check the objects currently in view and update object pool.
    ----------------------------------------------------------------------------------------------------------------------*/
    void Update() {
        CheckObjectPool ();
    }

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION:   HandleEvent
    -- DATE:       February 16, 2016
    -- REVISIONS:  March 31 - Add RESOURCE_TAKEN event processing
    --             April 6  - Remove events that are not handled by MapManager
    -- DESIGNER:   Jaegar Sarauer, Thomas Yu, Krystle Bulalakaw
    -- PROGRAMMER: Jaegar Sarauer, Thomas Yu, Krystle Bulalakaw
    -- INTERFACE:  void HandleEvent(int id, JSONClass message)
    --				  int id                id of received event, i.e. its event type 
    --				  JSONClass message	    JSON message for map creation 
    -- RETURNS:    void.
    -- NOTES:
	-- Execute the appropriate action given a received event.
    ----------------------------------------------------------------------------------------------------------------------*/
    public void HandleEvent(int id, JSONClass message) {
        switch ((EventType)id) {
            case EventType.CREATE_MAP:
                CreateMap(message);
                DrawMap();
                InstantiatePool();
                InstantiateResources(message);
                break;
            case EventType.RESOURCE_TAKEN:
                ProcessResourceTakenEvent(message);
                break;
            case EventType.RESOURCE_DEPLETED:
                ProcessResourceDepletedEvent(message);
                break;
            case EventType.RESOURCE_RESPAWN:
                ProcessResourceRespawnEvent(message);
                break;
        }
    }

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	CreateMap
    -- DATE: 		February 16, 2016
    -- REVISIONS: 	April 2, 2016  -  Use static string variables
    -- DESIGNER: 	Jaegar Sarauer, Krystle Bulalakaw
    -- PROGRAMMER: 	Jaegar Sarauer, Thomas Yu, Krystle Bulalakaw
    -- INTERFACE: 	CreateMap(JSONClass message)
    --					JSONClass message	the message in JSON for map creation, containing map data
    -- RETURNS: 	void.
    -- NOTES:
    -- Parses the JSON message and initializes map objects.
	-- Render map sprites to scene given a 2D map array. 
	-- Instantiates a Resource game object and adds it to the list.
    ----------------------------------------------------------------------------------------------------------------------*/
    private void CreateMap(JSONClass message) {
        print (message.ToString ());
        _mapWidth = message[NetworkKeyString.MapWidth].AsInt;
        _mapHeight = message[NetworkKeyString.MapHeight].AsInt;
        _map = new int[_mapWidth + (OUTERWALL_THICKNESS * 2), _mapHeight + (OUTERWALL_THICKNESS * 2)];
        _mapScenery = new int[_mapWidth, _mapHeight];

        JSONArray mapArrays = message[NetworkKeyString.MapIds].AsArray;

        for (int x = 0; x < _mapWidth + (OUTERWALL_THICKNESS * 2); x++) {
            JSONArray mapX = mapArrays[x - OUTERWALL_THICKNESS].AsArray;
            for (int y = 0; y < _mapHeight + (OUTERWALL_THICKNESS * 2); y++)
                if (x >= OUTERWALL_THICKNESS && x < _mapWidth + OUTERWALL_THICKNESS && y >= OUTERWALL_THICKNESS && y < _mapHeight + OUTERWALL_THICKNESS)
                    _map[x, y] = mapX[y - OUTERWALL_THICKNESS].AsInt;
                else
                    _map[x, y] = 0; //outer wall edges
        }

        JSONArray mapSceneryArrays = message[NetworkKeyString.MapScenery].AsArray;
        
        for (int x = 0; x < _mapWidth; x++) {
            JSONArray mapSceneryX = mapSceneryArrays[x].AsArray;
            for (int y = 0; y < _mapHeight; y++)
                _mapScenery[x, y] = mapSceneryX[y].AsInt;
        }
    }

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION:   InstantiateResources
    -- DATE:       March 31, 2016
    -- REVISIONS:  April 1 - Prevent resources from spawning in walls
    -- DESIGNER:   Krystle Bulalakaw
    -- PROGRAMMER: Krystle Bulalakaw, Jaegar Sarauer
    -- INTERFACE:  void InstantiateResources()
    -- RETURNS:    void.
    -- NOTES:
    -- Instantiates resource objects and adds them to list.
    -- Creates resource game objects on the map. It goes through the list of resources from the JSON message and 
    -- instantiates them based on their X and Y positions, which were pre-generated and assigned by the server. 
    -- The sprite is set randomly from a range of sprites (assigned in Unity Editor).
    ----------------------------------------------------------------------------------------------------------------------*/
    private void InstantiateResources(JSONClass message) {
        JSONArray resources = message[NetworkKeyString.MapResources].AsArray;

        for (int i = 0; i < resources.Count; i++) {
            GameObject temp = Instantiate(mapResource, new Vector3(resources[i][0].AsInt + OUTERWALL_THICKNESS,
                resources[i][1].AsInt + OUTERWALL_THICKNESS, RESOURCE_Z), Quaternion.identity) as GameObject;
            mapResource.GetComponent<SpriteRenderer>().sprite = _resourceSprites[(UnityEngine.Random.Range(0, _resourceSprites.Count))];
            temp.GetComponent<Resource>().x = resources[i][0].AsInt + OUTERWALL_THICKNESS;
            temp.GetComponent<Resource>().y = resources[i][1].AsInt + OUTERWALL_THICKNESS;
            temp.GetComponent<Resource>().hp = RESOURCE_HP;
            _mapResources.Add(temp);
        }
    }

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: DrawMap
    -- DATE: February 19, 2016
    -- REVISIONS: N/A
    -- DESIGNER: Thomas Yu
    -- PROGRAMMER: Thomas Yu, Jaegar Sarauer
    -- INTERFACE: void DrawMap()
    -- RETURNS: void.
    -- NOTES:
    -- This function places sprites on the terrain. Currently it checks the value of the 2d array and if the array is 0, 
    -- it randomly generates one of two types of grass. If the array value is 1, it randomly generates oneof two types of 
    -- water. This will be refactored so that all will be a different value in the array for each sprite. It also 
    -- generates a 2collision box on the water tiles so thatr objects enter it.
    ----------------------------------------------------------------------------------------------------------------------*/
    private void DrawMap() {
        if (_map == null) {
            print ("[DEBUG-map] _map value was null");
            return;
        }

        for (int x = 0; x < _mapWidth + (OUTERWALL_THICKNESS * 2); x++)
            for (int y = 0; y < _mapHeight + (OUTERWALL_THICKNESS * 2); y++) {
                //If the 2D array is land
                if (_map[x, y] >= 0 && _map[x, y] < 100) {
                    _obstacle.GetComponent<SpriteRenderer>().sprite = _mapSolids[_map[x, y] % _mapSolids.Count];
                    Instantiate(_obstacle, new Vector3(x, y, -2), Quaternion.identity);
                } else if (_map[x, y] >= 100 && _map[x, y] < 200) {
                    _tile.GetComponent<SpriteRenderer>().sprite = _mapWalkable[(_map[x, y] - 100) % _mapWalkable.Count];
                    Instantiate(_tile, new Vector3(x, y), Quaternion.identity);
                }	
            }

        for (int x = OUTERWALL_THICKNESS; x < _mapWidth + OUTERWALL_THICKNESS; x++)
            for (int y = OUTERWALL_THICKNESS; y < _mapHeight + OUTERWALL_THICKNESS; y++) {
                if (_mapScenery[x - OUTERWALL_THICKNESS, y - OUTERWALL_THICKNESS] >= 200 && _mapScenery[x - OUTERWALL_THICKNESS, y - OUTERWALL_THICKNESS] <= 201) {
                    GameData.TeamSpawnPoints.Add(new Pair<int, int>(x, y));
                }
                if (_mapScenery[x - OUTERWALL_THICKNESS, y - OUTERWALL_THICKNESS] != -1) {
                    _scenery.GetComponent<SpriteRenderer>().sprite = _mapSceneryObjects[(_mapScenery[x - OUTERWALL_THICKNESS, y - OUTERWALL_THICKNESS]) % _mapSceneryObjects.Count];
                    Instantiate(_scenery, new Vector3(x, y, -1), Quaternion.identity);
                }
            }
    }

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: ProcessResourceTakenEvent
    -- DATE: March 31, 2016
    -- REVISIONS: N/A
    -- DESIGNER: Jaegar Sarauer, Krystle Bulalakaw
    -- PROGRAMMER: Krystle Bulalakaw
    -- INTERFACE: void ProcessResourceTakenEvent(JSONClass message)
    --                                JSONClass message - the message received from the server
    -- RETURNS: void.
    -- NOTES:
    -- This function processes the message received from the server that a resource was taken.
    -- Using the X, Y, and Amount parsed from the message, find the Resource in the list and decrease its amount.
    ----------------------------------------------------------------------------------------------------------------------*/
    public void ProcessResourceTakenEvent(JSONClass message) {
		int xPos = message[NetworkKeyString.XPos].AsInt;
		int yPos = message[NetworkKeyString.YPos].AsInt;
		int amount = message[NetworkKeyString.Amount].AsInt;
        int playerId = message[NetworkKeyString.PlayerID].AsInt;		
		// Find the Resource in the list with these X and Y positions
		GameObject temp = _mapResources.Find(go => 
		                                     go.GetComponent<Resource>().x == xPos &&
		                                     go.GetComponent<Resource>().y == yPos);

        temp.GetComponent<Resource>().DecreaseHp(1, playerId);
	}

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: ProcessResourceDepletedEvent
    -- DATE: April 1, 2016
    -- REVISIONS: N/A
    -- DESIGNER: Jaegar Sarauer, Krystle Bulalakaw
    -- PROGRAMMER: Krystle Bulalakaw
    -- INTERFACE: void ProcessResourceDepletedEvent(JSONClass message)
    --                                JSONClass message - the message received from the server
    -- RETURNS: void.
    -- NOTES:
    -- This function processes the message received from the server that a resource was depleted.
    -- Using the X and Y parsed from the message, find the Resource in the list and explode/deactivate it.
    ----------------------------------------------------------------------------------------------------------------------*/
    public void ProcessResourceDepletedEvent(JSONClass message) {
		int xPos = message[NetworkKeyString.XPos].AsInt;
		int yPos = message[NetworkKeyString.YPos].AsInt;
        int playerId = message[NetworkKeyString.PlayerID].AsInt;

        // Find the Resource in the list with these X and Y positions
        GameObject temp = _mapResources.Find(go => 
		                                     go.GetComponent<Resource>().x == xPos &&
		                                     go.GetComponent<Resource>().y == yPos);

        // Only the player who killed the tree tells the server to create gold
        if (GameData.MyPlayer.PlayerID == playerId)
        {
            Debug.Log("me drops the gold");
            for (int i = 0; i < (TOTAL_GOLD / GOLD_TO_DROP); i++)
            {
                temp.GetComponent<Resource>().DropGold(GOLD_TO_DROP);
            }
        }

        // Explode and deactivate it
        StartCoroutine(ExplodeAndDestroy(temp));
	}

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: ProcessResourceRespawnEvent
    -- DATE: April 1, 2016
    -- REVISIONS: N/A
    -- DESIGNER: Jaegar Sarauer, Krystle Bulalakaw
    -- PROGRAMMER: Krystle Bulalakaw
    -- INTERFACE: void ProcessResourceRespawnEvent(JSONClass message)
    --                                JSONClass message - the message received from the server
    -- RETURNS: void.
    -- NOTES:
    -- This function processes the message received from the server to respawn a resource.
    -- Using the X and Y parsed from the message, respawn a resource after some time.
    ----------------------------------------------------------------------------------------------------------------------*/
    public void ProcessResourceRespawnEvent(JSONClass message) {
        int xPos = message[NetworkKeyString.XPos].AsInt;
        int yPos = message[NetworkKeyString.YPos].AsInt;

        // Find the Resource in the list with these X and Y positions
        GameObject temp = _mapResources.Find(go => 
		                                     go.GetComponent<Resource>().x == xPos &&
		                                     go.GetComponent<Resource>().y == yPos);
        // Respawn it
        StartCoroutine(RespawnAfterTime(temp, RESPAWN_TIME, RESOURCE_HP));
	}

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	ExplodeAndDestroy
    -- DATE: 		March 30, 2016
    -- REVISIONS: 	N/A
    -- DESIGNER:  	Krystle Bulalakaw
    -- PROGRAMMER: 	Krystle Bulalakaw
    -- INTERFACE: 	ExplodeAndDestory()
    -- RETURNS: 	IEnumerator - generic enumerator used to run a coroutine alongside Update()
    -- NOTES:
    -- Plays the explosion animation, waits for animation to complete, then deactivates the gameobject.
    ----------------------------------------------------------------------------------------------------------------------*/
    IEnumerator ExplodeAndDestroy(GameObject go) {
        go.GetComponent<Resource>().animator.SetTrigger("Depleted");
        audioSource.PlayOneShot (audioExplode);
        yield return new WaitForSeconds(0.267f); // animation length
        go.SetActive(false);
	}

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: IEnumerator RespawnAfterTime
    -- DATE: March 31, 2016
    -- REVISIONS: N/A
    -- DESIGNER: Jaegar Sarauer, Krystle Bulalakaw
    -- PROGRAMMER: Krystle Bulalakaw
    -- INTERFACE: IEnumerator RespawnAfterTime(GameObject go, int seconds, int amount)
    -                   GameObject go - Resource Game Object instance
    --                  int seconds   - number of seconds to wait before respawning resource
    --                  int amount    - amount of resource to respawn
    -- RETURNS: void.
    -- NOTES:
    -- Waits some number of seconds then sets a random Tree sprite, replenishes the resource amount, and activates
    -- the Game Object instance.
    ----------------------------------------------------------------------------------------------------------------------*/
	IEnumerator RespawnAfterTime(GameObject go, int seconds, int amount) {
        yield return new WaitForSeconds(seconds);

        go.GetComponent<SpriteRenderer>().sprite = _resourceSprites[(UnityEngine.Random.Range(0, _resourceSprites.Count))];
        go.GetComponent<Resource>().hp = amount;
        go.SetActive(true);
	}

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION:    Update
    -- DATE:        February 16, 2016
    -- REVISIONS:   N/A
    -- DESIGNER:    Krystle Bulalakaw
    -- PROGRAMMER:  Krystle Bulalakaw
    -- INTERFACE:   Update(void)
    -- RETURNS:     void.
    -- NOTES:
    -- Finds the camera distance used to calculate the camera view frustum.
    -- Creates the list of pooled objects and deactivate them.
    -- Performs initial object pool check.
    ----------------------------------------------------------------------------------------------------------------------*/
    private void InstantiatePool()
    {
        cameraDistance = -mainCamera.orthographicSize;
        // Find all objects with the tag "Tile" and add them to the arraylist.
        _pooledObjects = GameObject.FindGameObjectsWithTag("Tile");
        print("pooledObjects size: " + _pooledObjects.Length);

        // Deactivate all game objects on start.
        for (int i = 0; i < _pooledObjects.Length; i++)
        {
            _pooledObjects[i].SetActive(false);
        }

        // Initial check to see which game objects are in camera view.
        CheckObjectPool();
    }

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION:   CheckObjectPool
    -- DATE:       February 16, 2016
    -- REVISIONS:  N/A
    -- DESIGNER:   Jaegar Sarauer, Krystle Bulalakaw
    -- PROGRAMMER: Krystle Bulalakaw
    -- INTERFACE:  void CheckObjectPool()
    -- RETURNS:    void.
    -- NOTES:
    -- Render map tiles which are within the camera's view frustum.
    -- Iterate through the list of pooled objects. If the object is view, set it to active. Else, set it to inactive.
    ----------------------------------------------------------------------------------------------------------------------*/
    private void CheckObjectPool()
    {
        if (GameData.GameStart) {
            cameraPosition = mainCamera.GetComponent<Transform>().position;
            frustumHeight = 2.0f * cameraDistance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            frustumWidth = frustumHeight * mainCamera.aspect;

            if (_pooledObjects != null) {
                for (int i = 0; i < _pooledObjects.Length; i++) {
                    var pooledTransform = _pooledObjects[i].GetComponent<Transform>();
                    if ((pooledTransform.position.x > cameraPosition.x + frustumWidth) 
                        && (pooledTransform.position.x < cameraPosition.x - frustumWidth)
                        && (pooledTransform.position.y > cameraPosition.y + frustumHeight)
                        && (pooledTransform.position.y < cameraPosition.y - frustumHeight)) {
                        _pooledObjects[i].SetActive(true);
                    } else {
                        _pooledObjects[i].SetActive(false);
                    }
                }
            }
        }
    }
}

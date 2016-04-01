using UnityEngine;
using System.Collections;
using SimpleJSON;
using System;
using System.Collections.Generic;

public class MapManager : MonoBehaviour {
    // 
    // VARIABLE DECLARATIONS
    //
    /* Constant string variables for building/parsing JSON. */
    private const string ID = "id";
    private const string DATA = "data";
    private const string MAP = "map";
    private const string RESOURCE = "resource";
    private const string ITEM = "item";
    private const string BUILDING = "building";
    private const string TRANSACTION = "transaction";
    private const string INCR = "incr";
    private const string DECR = "decr";
    /* How many walls will line the outside to not give an image of an empty world around the edges */
    const int OUTERWALL_THICKNESS = 24; //(Camera size + gunnerclass.cs max zoom out)/2
    /* A set of constant map update event types. */
    public enum EventType 
    {
        CREATE_MAP = 0,
        RESOURCE_TAKEN = 1,
        RESOURCE_DEPLETED = 2,
        ITEM_DROPPED = 3,
        ITEM_PICKEDUP = 4,
        BUILDING_HIT = 5,
        BUILDING_DSTR = 6,
    };
    /* ID of map update event, i.e. its event type. */
    private EventType _id;
    /* 2D string array containing map values. */
    private string _string_map;
    /* 2D int array containing map values. */
    private int[,] _map;
    /* 2D int array containing map scenery values. */
    private int[,] _mapScenery;
    /*Map width*/
    private int _mapWidth;
    /*Map height*/
    private int _mapHeight;
    /* Ancillary data about the map update event. */
    private string _data;

    //MapSprites mp = GameObject.AddComponent<MapSprites> as MapSprites;
    //public MapSprites mp;
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

    //
    // METHOD DEFINITIONS
    //
    /**
	 * Use this for initialization.
	 */
    void Start() {
    }

	/*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	Update
    -- DATE: 		February 16, 2016
    -- REVISIONS: 	N/A
    -- DESIGNER:  	Krystle Bulalakaw
    -- PROGRAMMER: 	Krystle Bulalakaw
    -- INTERFACE: 	Update(void)
    -- RETURNS: 	void.
    -- NOTES:
    -- Called once per frame.
    -- Check the objects currently in view and update object pool.
    -- Checks the list of resources and respawns after some time them if they are depleted.
    ----------------------------------------------------------------------------------------------------------------------*/
	void Update() {
		check_object_pool ();
		RespawnDepletedObjectsLoop();
	}
	
	/**
	 * Deserialize JSON string into 2D int array.
	 */
	private int[,] parse_string_map(string map) {
		// _map = json.deserialization<int>(map)
		return _map;
	}
	
	/* Serialize 2D int array into JSON string. */
	private string parse_int_map(int[][] map) {
		return _string_map;
	}
	
	/**
     * Find the camera distance used to calculate the camera view frustum.
     * Create the list of pooled objects and deactivate them.
     * Perform initial object pool check.
     */
    private void instantiate_pool() {
        cameraDistance = -mainCamera.orthographicSize;
        // Find all objects with the tag "Tile" and add them to the arraylist.
        _pooledObjects = GameObject.FindGameObjectsWithTag("Tile");
        print("pooledObjects size: " + _pooledObjects.Length);

        // Deactivate all game objects on start.
        for (int i = 0; i < _pooledObjects.Length; i++) {
            _pooledObjects[i].SetActive(false);
        }

        // Initial check to see which game objects are in camera view.
        check_object_pool();
    }

    /**
	 * Utilizes object pooling to only render map tiles which are within the camera's
	 * view frustum.
	 * 
	 * Iterate through the list of pooled objects. If the object is in the camera, set it to 
	 * active. Else, set it to inactive.
	 */
    private void check_object_pool() {
		if (GameData.GameStart) {
	        cameraPosition = mainCamera.GetComponent<Transform>().position;
	        frustumHeight = 2.0f * cameraDistance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
	        frustumWidth = frustumHeight * mainCamera.aspect;

            if (_pooledObjects != null) {
	            for (int i = 0; i < _pooledObjects.Length; i++)
                {
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

	/*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: handle_event
    -- DATE: February 16, 2016
    -- REVISIONS: March 31 - Add RESOURCE_TAKEN event processing
    -- DESIGNER: Jaegar Sarauer, Krystle Bulalakaw
    -- PROGRAMMER: Jaegar Sarauer, Thomas Yu, Krystle Bulalakaw
    -- INTERFACE: void handle_event(int id, JSONClass message)
    --				int id				id of received event, i.e. its event type 
    --				JSONClass message	the message in JSON for map creation 
    -- RETURNS: void.
    -- NOTES:
	-- Execute the appropriate action given a received event.
    ----------------------------------------------------------------------------------------------------------------------*/
    public void handle_event(int id, JSONClass message) {
        switch ((EventType)id) {
            case EventType.CREATE_MAP:
                create_map(message);
                draw_map();
                instantiate_pool();
                break;
            case EventType.RESOURCE_TAKEN:
                ProcessResourceTakenEvent(message);
                break;
            case EventType.RESOURCE_DEPLETED:
				// TODO: trigger depleted animation
                break;
            case EventType.ITEM_DROPPED:
                break;
            case EventType.ITEM_PICKEDUP:
                break;
            case EventType.BUILDING_HIT:
                break;
            case EventType.BUILDING_DSTR:
                break;
        }
    }

	/*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	create_map
    -- DATE: 		February 16, 2016
    -- REVISIONS: 	N/A
    --it  DESIGNER: 	Jaegar Sarauer, Krystle Bulalakaw
    -- PROGRAMMER: 	Jaegar Sarauer, Thomas Yu, Krystle Bulalakaw
    -- INTERFACE: 	create_map(JSONClass message)
    --					JSONClass message	the message in JSON for map creation, containing map data
    -- RETURNS: 	void.
    -- NOTES:
    -- Parses the JSON message and initializes map objects.
	-- Render map sprites to scene given a 2D map array. 
	-- Instantiates a Resource game object and adds it to the list.
    ----------------------------------------------------------------------------------------------------------------------*/
    private void create_map(JSONClass message) {
		print (message.ToString ());
        _mapWidth = message["mapWidth"].AsInt;
        _mapHeight = message["mapHeight"].AsInt;
        _map = new int[_mapWidth  + (OUTERWALL_THICKNESS * 2), _mapHeight + (OUTERWALL_THICKNESS * 2)];
        _mapScenery = new int[_mapWidth, _mapHeight];

        JSONArray mapArrays = message["mapIDs"].AsArray;

        for (int x = 0; x < _mapWidth + (OUTERWALL_THICKNESS * 2); x++) {
            JSONArray mapX = mapArrays[x - OUTERWALL_THICKNESS].AsArray;
            for (int y = 0; y < _mapHeight + (OUTERWALL_THICKNESS * 2); y++)
                if (x >= OUTERWALL_THICKNESS && x < _mapWidth + OUTERWALL_THICKNESS && y >= OUTERWALL_THICKNESS && y < _mapHeight + OUTERWALL_THICKNESS)
                    _map[x, y] = mapX[y - OUTERWALL_THICKNESS].AsInt;
                else
                    _map[x, y] = 0; //outer wall edges
        }

        JSONArray mapSceneryArrays = message["mapSceneryIDs"].AsArray;
        
        for (int x = 0; x < _mapWidth; x++) {
            JSONArray mapSceneryX = mapSceneryArrays[x].AsArray;
            for (int y = 0; y < _mapHeight; y++)
                _mapScenery[x, y] = mapSceneryX[y].AsInt;
        }

		// Instantiates resource objects and adds them to list.
		// Creates resource game objects on the map. It goes through the list of resources from the JSON message and instantiates them based on 
		// their X and Y positions, which were pre-generated and assigned by the server. 
		// The sprite is set randomly from a range of sprites (assigned in Unity Editor).
        JSONArray resources = message["mapResources"].AsArray;
        for (int i = 0; i < resources.Count; i++) {
			GameObject temp = Instantiate(mapResource, new Vector3(resources[i][0].AsInt, resources[i][1].AsInt, -2), Quaternion.identity) as GameObject;
			mapResource.GetComponent<SpriteRenderer>().sprite = _resourceSprites[(UnityEngine.Random.Range(0, _resourceSprites.Count))];
			temp.GetComponent<Resource>().x = resources[i][0].AsInt;
			temp.GetComponent<Resource>().y = resources[i][1].AsInt;
			temp.GetComponent<Resource>().amount = 5;
			Debug.Log ("Adding resource at (" + temp.GetComponent<Resource>().x + ", " + temp.GetComponent<Resource>().y + ") Amount: " + temp.GetComponent<Resource>().amount);
			_mapResources.Add (temp);
		}
	}

   
    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: draw_map
    --
    -- DATE: February 19, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Thomas Yu
    --
    -- PROGRAMMER: Thomas Yu, Jaegar Sarauer
    --
    -- INTERFACE: void draw_map()
    --
    -- RETURNS: void.
    --
    -- NOTES:
    -- This function places sprites on the terrain. Currently it checks the value of the 2d array and if the array is 0, 
    -- it randomly generates one of two types of grass. If the array value is 1, it randomly generates oneof two types of 
    -- water. This will be refactored so that all will be a different value in the array for each sprite. It also 
    -- generates a 2collision box on the water tiles so thatr objects enter it.
    ----------------------------------------------------------------------------------------------------------------------*/
    private void draw_map() {
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
    --
    -- DATE: March 31, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Jaegar Sarauer, Krystle Bulalakaw
    --
    -- PROGRAMMER: Krystle Bulalakaw
    --
    -- INTERFACE: void ProcessResourceTakenEvent(JSONClass message)
    --							JSONClass message - the message received from the server
    --
    -- RETURNS: void.
    --
    -- NOTES:
    -- This function processes the message received from the server that a resource was taken.
    -- Using the X, Y, and Amount parsed from the message, find the Resource in the list and decrease its amount.
    ----------------------------------------------------------------------------------------------------------------------*/
	public void ProcessResourceTakenEvent(JSONClass message) {
		int xPos = message[NetworkKeyString.XPos].AsInt;
		int yPos = message[NetworkKeyString.YPos].AsInt;
		int amount = message["ResourceAmountTaken"].AsInt;
		
		// Find the Resource in the list with these X and Y positions
		GameObject temp = _mapResources.Find(go => 
		                                     go.GetComponent<Resource>().x == xPos &&
		                                     go.GetComponent<Resource>().y == yPos);
		// Decrease its amount
		temp.GetComponent<Resource>().DecreaseAmount(amount);
	}

	/*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: RespawnDepletedObjectsLoop
    --
    -- DATE: March 31, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Jaegar Sarauer, Krystle Bulalakaw
    --
    -- PROGRAMMER: Krystle Bulalakaw
    --
    -- INTERFACE: void RespawnDepletedObjectsLoop()
    --
    -- RETURNS: void.
    --
    -- NOTES:
    -- Iterates through the list of map resources and checks if it is depleted. If it is, start a coroutine to respawn
    -- some amount of it after a number of seconds.
    ----------------------------------------------------------------------------------------------------------------------*/
	public void RespawnDepletedObjectsLoop() {
		int seconds = 30;
		int amount = 5;

		for (int i = 0; i < _mapResources.Count; i++) {
			Resource res = _mapResources[i].GetComponent<Resource>();
			if (res.depleted) {
				StartCoroutine(RespawnAfterTime(_mapResources[i], seconds, amount));
			}
		}
	}

	/*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: IEnumerator RespawnAfterTime
    --
    -- DATE: March 31, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Jaegar Sarauer, Krystle Bulalakaw
    --
    -- PROGRAMMER: Krystle Bulalakaw
    --
    -- INTERFACE: IEnumerator RespawnAfterTime(GameObject go, int seconds, int amount)
    -                   GameObject go - Resource Game Object instance
    --                  int seconds   - number of seconds to wait before respawning resource
    --                  int amount    - amount of resource to respawn
    --
    -- RETURNS: void.
    --
    -- NOTES:
    -- Waits some number of seconds then sets a random Tree sprite, replenishes the resource amount, and activates
    -- the Game Object instance.
    ----------------------------------------------------------------------------------------------------------------------*/
	IEnumerator RespawnAfterTime(GameObject go, int seconds, int amount) {
		yield return new WaitForSeconds(seconds);

		go.GetComponent<SpriteRenderer>().sprite = _resourceSprites[(UnityEngine.Random.Range(0, _resourceSprites.Count))];
		go.GetComponent<Resource>().amount = amount;
		go.GetComponent<Resource>().depleted = false;
		go.SetActive(true);
	}
}

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
    /* A set of constant map update event types. */
    enum EventType {
        CREATE_MAP = 0,
        RESOUCE_TAKEN = 1,
        RESOURCE_DEPLETED = 2,
        ITEM_DROPPED = 3,
        ITEM_PICKEDUP = 4,
        BUILDING_HIT = 5,
        BUILDING_DSTR = 6
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

    // Object Pooling
    private GameObject[] _pooledObjects;
    public Camera mainCamera;
    public Vector3 cameraPosition;
    public float cameraDistance;
    public float frustumHeight, frustumWidth;

    //
    // METHOD DEFINITIONS
    //
    /**
	 * Use this for initialization.
	 */
    void Start() {
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
	 * active. Else, set it to unactive.
	 */
    private void check_object_pool() {
		if (GameData.GameStart) {
	        cameraPosition = mainCamera.GetComponent<Transform>().position;
	        frustumHeight = 2.0f * cameraDistance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
	        frustumWidth = frustumHeight * mainCamera.aspect;

	        if (_pooledObjects != null) {
	            for (int i = 0; i < _pooledObjects.Length; i++) {
	                if ((_pooledObjects[i].GetComponent<Transform>().position.x > cameraPosition.x + frustumWidth)
	                    && (_pooledObjects[i].GetComponent<Transform>().position.x < cameraPosition.x - frustumWidth)
	                    && (_pooledObjects[i].GetComponent<Transform>().position.y > cameraPosition.y + frustumHeight)
	                    && (_pooledObjects[i].GetComponent<Transform>().position.y < cameraPosition.y - frustumHeight)) {
	                    _pooledObjects[i].SetActive(true);
	                } else {
	                    _pooledObjects[i].SetActive(false);
	                }
	            }
	        }
		}
    }

    /**
	 * Check the object pool in camera view and update their activation every second.
	 */
    void Update() {
        check_object_pool();

    }

    /**
	 * Decode the received message and handle its event.
	 * @param message 	received JSON message from NetworkingManager
	 */
    /*public void receive_message(JSONClass message, int id) {
        var data = message["data"].ToString();
        var data2 = message["data2"].AsInt;
        var data3 = message["data3"].AsFloat;
        _id = (EventType)id;
        _data = message[DATA];
        //decode_message(message);
        handle_event(_map, _id, _data);
	}*/

    /**
	 * Set the MapManager's private variables based on the parsed JSON string.
	 * A message's "data" component can be broken down into its event type, 
	 * the terrain/building type, and the terrain/building property values.
	 * @param message 	received JSON message from NetworkingManager
	 */
    //private void decode_message(JSONClass msgObject) {
    //	_string_map = msgObject[MAP];
    //	_data = msgObject[DATA];
    //}

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
	 * Execute the appropriate action given a received event.
	 * @param map 	2d int array of map values
	 * @param id 	id of received event, i.e. its event type 
	 * @param data 	ancillary data of map event
	 */
    public void handle_event(int id, JSONClass message) {
        // Enums are not ints in C# :(
        switch ((EventType)id) {
            case EventType.CREATE_MAP:
                create_map(message);
                draw_map();
                instantiate_pool();
                break;
            case EventType.RESOUCE_TAKEN:
                break;
            case EventType.RESOURCE_DEPLETED:
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

    /**
	 * Render map sprites to scene given a 2d map array. 
	 * @param map 	2d int array of map values
	 */
    private void create_map(JSONClass message) {
        _mapWidth = message["mapWidth"].AsInt;
        _mapHeight = message["mapHeight"].AsInt;
        _map = new int[_mapWidth, _mapHeight];
        _mapScenery = new int[_mapWidth, _mapHeight];

        JSONArray mapArrays = message["mapIDs"].AsArray;

        for (int x = 0; x < _mapWidth; x++) {
            JSONArray mapX = mapArrays[x].AsArray;
            for (int y = 0; y < _mapHeight; y++)
                _map[x, y] = mapX[y].AsInt;
        }


        JSONArray mapSceneryArrays = message["mapSceneryIDs"].AsArray;
        

        for (int x = 0; x < _mapWidth; x++) {
            JSONArray mapSceneryX = mapSceneryArrays[x].AsArray;
            for (int y = 0; y < _mapHeight; y++)
                _mapScenery[x, y] = mapSceneryX[y].AsInt;
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
    -- PROGRAMMER: Thomas Yu, Jaegar Sarauer
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
    private void draw_map() {
        if (_map == null)
            return;
        for (int x = 0; x < _mapWidth; x++)
            for (int y = 0; y < _mapHeight; y++) {
                //If the 2D array is land
                if (_map[x, y] >= 0 && _map[x, y] < 100) {
                    _obstacle.GetComponent<SpriteRenderer>().sprite = _mapSolids[_map[x, y] % _mapSolids.Count];
                    Instantiate(_obstacle, new Vector3(x, y, -1), Quaternion.identity);
                } else if (_map[x, y] >= 100 && _map[x, y] < 200) {
                    _tile.GetComponent<SpriteRenderer>().sprite = _mapWalkable[(_map[x, y] - 100) % _mapWalkable.Count];
                    Instantiate(_tile, new Vector3(x, y), Quaternion.identity);
                }
                if (_mapScenery[x, y] >= 200 && _map[x, y] <= 201) {
                    GameData.TeamSpawnPoints.Add(new Pair<int, int>(x, y));
                }
                if (_mapScenery[x, y] != -1) {
                    _scenery.GetComponent<SpriteRenderer>().sprite = _mapSceneryObjects[(_mapScenery[x, y]) % _mapSceneryObjects.Count];
                    Instantiate(_scenery, new Vector3(x, y, -1), Quaternion.identity);
                }
            }
    }
}

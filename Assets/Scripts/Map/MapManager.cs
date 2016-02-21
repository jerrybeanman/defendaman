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
    /*Map width*/
    private int _mapWidth;
    /*Map height*/
    private int _mapHeight;
    /* Ancillary data about the map update event. */
    private string _data;

    //MapSprites mp = GameObject.AddComponent<MapSprites> as MapSprites;
    //public MapSprites mp;
    public GameObject _tile;
    public GameObject _obstacle;
    public List<Sprite> _mapSolids;
    public List<Sprite> _mapWalkable;

    //
    // METHOD DEFINITIONS
    //
    /**
	 * Use this for initialization.
	 */
    void Start() {
    }

    /**
	 * Update is called once per frame.
	 */
    void Update() {
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

        JSONArray mapArrays = message["mapIDs"].AsArray;

        for (int x = 0; x < _mapWidth; x++) {
            JSONArray mapX = mapArrays[x].AsArray;
            for (int y = 0; y < _mapHeight; y++)
                _map[x, y] = mapX[y].AsInt;
        }
        // Thomas/Jaegar's map generation function goes here
        // draw(map);
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
                    _obstacle.GetComponent<SpriteRenderer>().sprite = _mapSolids[ _map[x, y] % _mapSolids.Count];
                    Instantiate(_obstacle, new Vector3(x, y), Quaternion.identity);
                } else if (_map[x, y] >= 100 && _map[x, y] < 200) {
                    _tile.GetComponent<SpriteRenderer>().sprite = _mapWalkable[(_map[x, y] - 100) % _mapWalkable.Count];
                    Instantiate(_tile, new Vector3(x, y), Quaternion.identity);
                }
            }
    }


}

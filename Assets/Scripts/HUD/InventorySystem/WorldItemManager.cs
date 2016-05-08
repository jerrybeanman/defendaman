using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

enum ItemUpdate { Pickup = 1, Drop = 2, Create = 3, Magnetize = 4 }
/*-------------------------------------------------------------------------------------------
-- WorldItemManager.cs - Script attached to GameManager game object
--                       responsible for managing world items.
--
-- FUNCTIONS:
--      void Awake()
--      void Start()
--      public GameObject CreateWorldItem(int world_item_id, int item_id, 
--          int amt, float pos_x, float pos_y)
--      public void ReceiveCreateWorldItemPacket(JSONClass itemPacket)
--      public void ReceiveItemPickupPacket(JSONClass itemPacket)
--      public void ReceiveItemMagnetizePacket(JSONClass itemPacket)
--      public IEnumerator WaitSmallDelayBeforeReceivePickupPacket(JSONClass itemPacket)
--      public void ProcessPickUpEvent(int world_item_id, int player_id,
--          int item_id, int amt)
--      public void ProcessDropEvent(int world_item_id, int player_id, int item_id,
--      CreateWorldItem(world_item_id, item_id, amt, pos_x, pos_y);
--      void ToggleAutoLootable(int world_item_id)
--      public void ProcessMagnetizeEvent(int playerId, int worldItemId)
--      public void DestroyWorldItem(int world_item_id)
--      public int GenerateWorldItemId()
--      public List<Pair<string, string>> CreateDropItemNetworkMessage(int item_id,
--          int amt, int inv_pos)
--      public JSONClass ConvertListToJSONClass(List<Pair<string, string>> list)
--      public List<Pair<string, string>> CreatePickupItemNetworkMessage(int world_item_id,
--          int player_id, int item_id, int amt)
--      public List<Pair<string, string>> CreateWorldItemNetworkMessage(int world_item_id,
--          int item_id, int amt, float pos_x, float pos_y)
--
-- DATE:        05/03/2016
-- REVISIONS:   15/03/2016 - Add functions to to send and receive item drop
--                              and item pickup messages to/from the network
--              03/04/2016 - Add sound components for gold - Krystle
--              04/04/2016 - Add functions to send and receive createWorldItemMessages 
--              05/04/2016 - Add magnetize network update - Krystle
--              06/04/2016 - Fix bug with dropped gold autolooting - Krystle
-- DESIGNER:	Joseph Tam-Huang, Krystle Bulalakaw
-- PROGRAMMER:  Joseph Tam-Huang, Krystle Bulalakaw
------------------------------------------------------------------------------------------------*/
public class WorldItemManager : MonoBehaviour
{
    private int _dropped_item_instance_id;
    ItemManager _item_manager;
    public GameObject world_item;
    Inventory _inventory;
    int _my_player_id;
    public static WorldItemManager Instance;
    public AudioSource audioSource;
    public AudioClip audioPickup;
    public AudioClip audioDrop;


    /*---------------------------------------------------------------------------
    -- FUNCTION: 	Awake
    -- DATE: 		15/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	void Awake()
    -- RETURNS: 	void
    -- NOTES:
    -- Called once during the lifetime of the script, create one and only one
    -- instance of the WorldItemManager. Enforces the singleton pattern.
    -----------------------------------------------------------------------------*/
    void Awake()
    {
        if (Instance == null)               
            Instance = this;                
        else if (Instance != this)          
            Destroy(gameObject);             
        DontDestroyOnLoad(gameObject);      
    }

    /*-----------------------------------------------------------------------------------
    -- FUNCTION: 	Start
    -- DATE: 		05/03/2016
    -- REVISIONS: 	26/03/2016 - Added subscriptions to the networking manager
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	void Start()
    -- RETURNS: 	void
    -- NOTES:
    -- Subcribes to the NetworkingManager to listen for Item related messages 
    -- coming from the server and process them though call back functions.
    -- Retrives the ItemManager script, the Inventory script and the player id.
    ----------------------------------------------------------------------------------*/
    void Start()
    {
        NetworkingManager.Subscribe(ReceiveItemPickupPacket, DataType.Item, (int)ItemUpdate.Pickup);
        NetworkingManager.Subscribe(ReceiveItemDropPacket, DataType.Item, (int)ItemUpdate.Drop);
        NetworkingManager.Subscribe(ReceiveCreateWorldItemPacket, DataType.Item, (int)ItemUpdate.Create);
        NetworkingManager.Subscribe(ReceiveItemMagnetizePacket, DataType.Item, (int)ItemUpdate.Magnetize);

        _item_manager = GetComponent<ItemManager>();
        _inventory = GameObject.Find("Inventory").GetComponent<Inventory>();
        _my_player_id = GameData.MyPlayer.PlayerID;

        audioSource = (AudioSource)gameObject.GetComponent<AudioSource>();
    }

    /*-------------------------------------------------------------------------------
    -- FUNCTION: 	CreateWorldItem
    -- DATE: 		05/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public GameObject CreateWorldItem(int world_item_id, int item_id,
    --                  int amt, float pos_x, float pos_y)
    --                  int world_item_id - the id of the world item
    --                  int item_id - the id of the item
    --                  int amt - the amount of the item
    --                  float pos_x - the x coordinate on the world map
    --                  float pos_y - the y coordinate on the world map
    -- RETURNS: 	void
    -- NOTES:
    -- Creates a world item
    ---------------------------------------------------------------------------------*/
    public GameObject CreateWorldItem(int world_item_id, int item_id, int amt, float pos_x, float pos_y)
    {
        Item item = _item_manager.FetchItemById(item_id);
        GameObject _item = Instantiate(world_item);
        _item.GetComponent<WorldItemData>().world_item_id = world_item_id;
        _item.GetComponent<WorldItemData>().item = item;
        _item.GetComponent<WorldItemData>().amount = amt;
        _item.GetComponent<SpriteRenderer>().sprite = item.world_sprite;
        _item.transform.position = new Vector3(pos_x, pos_y, -5);

        // Add gold-specific components
        if (item_id == 2) {
            _item.GetComponent<WorldItemData>().autolootable = true;
            _item.AddComponent<Magnetize>();
            _item.GetComponent<Magnetize>().world_item_id = world_item_id;

            // Sound component and gold drop sound
            // audioDrop = Resources.Load ("Music/Inventory/currency") as AudioClip;
            // audioSource.PlayOneShot (audioDrop);
        }
        return _item;
    }

    /*---------------------------------------------------------------------------------
    -- FUNCTION: 	ReceiveCreateWorldItemPacket
    -- DATE: 		04/04/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void ReceiveCreateWorldItemPacket(JSONClass itemPacket)
    --                  JSONClass itemPacket - The CreateWorldItemPacket
    -- RETURNS: 	void
    -- NOTES:
    -- Processes the CreateWorldItemPacket and creates the speficied world item
    ----------------------------------------------------------------------------------*/
    public void ReceiveCreateWorldItemPacket(JSONClass itemPacket)
    {
        GameObject go = CreateWorldItem(itemPacket["WorldItemId"].AsInt, itemPacket["ItemId"].AsInt, 
            itemPacket["Amount"].AsInt, itemPacket["PosX"].AsFloat, itemPacket["PosY"].AsFloat);
        if (itemPacket["ItemId"].AsInt == 2)
        {
            go.AddComponent<Magnetize>();
        }
    }

    /*---------------------------------------------------------------------------
    -- FUNCTION: 	ReceiveItemPickupPacket
    -- DATE: 		15/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void ReceiveItemPickupPacket(JSONClass itemPacket)
    --                  JSONClass itemPacket - The PickUpItemPacket
    -- RETURNS: 	void
    -- NOTES:
    -- Processes the ItemPickupPacket and destroys the specified item from
    -- the world. If the player id specified in the packet matches the our actual
    -- player id, then add the item to the inventory.
    -----------------------------------------------------------------------------*/
    public void ReceiveItemPickupPacket(JSONClass itemPacket)
    {
        ProcessPickUpEvent(itemPacket["WorldItemId"].AsInt, itemPacket["PlayerId"].AsInt,
            itemPacket["ItemId"].AsInt, itemPacket["Amount"].AsInt);
    }

    /*---------------------------------------------------------------------------
    -- FUNCTION: 	ReceiveItemDropPacket
    -- DATE: 		15/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void ReceiveItemDropPacket(JSONClass itemPacket)
    --                  JSONClass itemPacket - The ItemDropPacket
    -- RETURNS: 	void
    -- NOTES:
    -- Processes the ItemDropPacket and creates the specified item in the world. 
    -- If the player id specified in the packet matches the our actual player id,
    -- then destroy the item from the inventory.
    -----------------------------------------------------------------------------*/
    public void ReceiveItemDropPacket(JSONClass itemPacket)
    {
        ProcessDropEvent(itemPacket["WorldItemId"].AsInt, itemPacket["PlayerId"].AsInt,
            itemPacket["ItemId"].AsInt, itemPacket["Amount"].AsInt, itemPacket["InvPos"].AsInt,
            itemPacket["PosX"].AsInt, itemPacket["PosY"].AsInt);
    }
    
    public void ReceiveItemMagnetizePacket(JSONClass itemPacket)
    {
        ProcessMagnetizeEvent(itemPacket[(NetworkKeyString.PlayerID)].AsInt, 
                              itemPacket[(NetworkKeyString.WorldItemID)].AsInt);
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 	WaitSmallDelayBeforeReceivePickupPacket
    -- DATE: 		04/04/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public IEnumerator WaitSmallDelayBeforeReceivePickupPacket(JSONClass itemPacket)
    --                  JSONClass itemPacket - The ItemPickupPacket
    -- RETURNS: 	IEnumerator
    -- NOTES:
    -- Used for testing to simulate receiving PickupPackets from the server and 
    -- process them.
    -------------------------------------------------------------------------------------------------*/
    public IEnumerator WaitSmallDelayBeforeReceivePickupPacket(JSONClass itemPacket)
    {
        yield return new WaitForSeconds(0.05f);
        ReceiveItemPickupPacket(itemPacket);
    }

    /*----------------------------------------------------------------------------------------------------
    -- FUNCTION: 	ProcessPickUpEvent
    -- DATE: 		15/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public IEnumerator WaitSmallDelayBeforeReceivePickupPacket(JSONClass itemPacket)
    --                  JSONClass itemPacket - The ItemPickupPacket
    -- RETURNS: 	
    -- NOTES:
    -- Used for testing to simulate receiving PickupPackets from the server and 
    -- process them.
    ---------------------------------------------------------------------------------------------------*/
    public void ProcessPickUpEvent(int world_item_id, int player_id, int item_id, int amt)
    {
        // if (_my_player_id == player_id) // Disabled for testing, the player ID is set later
        if (GameData.MyPlayer.PlayerID == player_id)
        {
            _inventory.AddItem(item_id, amt);
            GameData.ItemCollided = false;

            // Play gold pickup sound
            if (item_id == 2)
            {
                audioPickup = Resources.Load("Music/Inventory/gold_pickup") as AudioClip;
                audioSource.PlayOneShot(audioPickup);
            }
        }

        DestroyWorldItem(world_item_id);
    }

    /*-------------------------------------------------------------------------------------------
    -- FUNCTION: 	ProcessDropEvent
    -- DATE: 		15/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void ProcessDropEvent(int world_item_id, int player_id, int item_id,
    --                  int amt, int inv_pos, int pos_x, int pos_y)
    --                  int world_item_id: The world item id
    --                  int player_id: The player id
    --                  int item_id: The item id
    --                  int amt: The amount to drop
    --                  int inv_pos: The inventory slot position
    --                  int pos_x: The x coordinate in the world
    --                  int pos_y: the y coordinate in the world
    -- RETURNS: 	void
    -- NOTES:
    -- Processes an item drop message from the server.
    -- Removes item to the player's inventory if the player id matches the player id in the message.
    -- Adds item to the world.
    ------------------------------------------------------------------------------------------------*/
    public void ProcessDropEvent(int world_item_id, int player_id, int item_id,
                                 int amt, int inv_pos, int pos_x, int pos_y)
    {
        if (GameData.MyPlayer.PlayerID == player_id)
        {
            _inventory.DestroyInventoryItem(inv_pos, amt);
        }
        CreateWorldItem(world_item_id, item_id, amt, pos_x, pos_y);

        // Makes it so that dropped gold will not magnetize nor autoloot
        ToggleAutoLootable(world_item_id);
    }

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION:    DisableAutoLootable
    -- DATE:        April 6, 2016
    -- REVISIONS:   N/A
    -- DESIGNER:    Krystle Bulalakaw
    -- PROGRAMMER:  Krystle Bulalakaw
    -- INTERFACE:   ToggleAutoLootable(int world_item_id)
    --                      int worldItemId - the world item ID
    -- RETURNS:     void.
    -- NOTES:
    -- Finds the world item with matching ID, disables the autolootable property and removes magnetize component.
    ----------------------------------------------------------------------------------------------------------------------*/
    void ToggleAutoLootable(int world_item_id)
    {
        GameObject[] _world_items = GameObject.FindGameObjectsWithTag("WorldItem");

        foreach (GameObject _world_item in _world_items)
        {
            if (_world_item.GetComponent<WorldItemData>().world_item_id == world_item_id)
            {
                _world_item.GetComponent<WorldItemData>().autolootable = false;
                Destroy(_world_item.GetComponent<Magnetize>());
            }
        }
    }

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION:    ProcessMagnetizeEvent
    -- DATE:        April 5, 2016
    -- REVISIONS:   N/A
    -- DESIGNER:    Krystle Bulalakaw
    -- PROGRAMMER: 	Krystle Bulalakaw
    -- INTERFACE:   ProcessMagnetizeEvent(int playerId, int world_item_id)
    --                      int playerId - the player to magnetize the item towards
    --                      int worldItemId - the world item ID
    -- RETURNS:     void.
    -- NOTES:
    -- Processes an item magnetize message from the server.
    -- Finds the world item in the list with the matching world item ID, then gets the Magnetize component of the
    -- item and assigns the player ID parsed from the message.
    ----------------------------------------------------------------------------------------------------------------------*/
    public void ProcessMagnetizeEvent(int playerId, int worldItemId)
    {
        GameObject[] _world_items = GameObject.FindGameObjectsWithTag("WorldItem");

        foreach (GameObject _world_item in _world_items)
        {
            if (_world_item.GetComponent<WorldItemData>().world_item_id == worldItemId)
            {
                _world_item.GetComponent<Magnetize>().playerId = playerId;
            }
        }
    }

    /*---------------------------------------------------------------------------
    -- FUNCTION: 	DestroyWorldItem
    -- DATE: 		15/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void DestroyWorldItem(int world_item_id)
    --                  int world_item_id: The world item id
    -- RETURNS: 	void
    -- NOTES:
    -- Destroys the world item with the id specified
    -----------------------------------------------------------------------------*/
    public void DestroyWorldItem(int world_item_id)
    {
        GameObject[] _world_items = GameObject.FindGameObjectsWithTag("WorldItem");
        Debug.Log("num world items: " + _world_items.Length);
        foreach (GameObject _world_item in _world_items)
        {
            if (_world_item.GetComponent<WorldItemData>().world_item_id == world_item_id)
            {
                Destroy(_world_item);
                GameObject.Find("Inventory").GetComponent<Tooltip>().Deactivate();
                break;
            }
        }
    }

    /*---------------------------------------------------------------------------
    -- FUNCTION: 	GenerateWorldItemId
    -- DATE: 		15/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public int GenerateWorldItemId()
    -- RETURNS: 	int: A unique world item id
    -- NOTES:
    -- Generates a unique world item id
    -----------------------------------------------------------------------------*/
    public int GenerateWorldItemId()
    {
        return GameData.MyPlayer.PlayerID * 1000000 + _dropped_item_instance_id++;
    }

    /*---------------------------------------------------------------------------------------------
    -- FUNCTION: 	CreateDropItemNetworkMessage
    -- DATE: 		15/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public List<Pair<string, string>> CreateDropItemNetworkMessage(int item_id,
    --                  int amt, int inv_pos)
    --                  int item_id: The item id
    --                  int amt: The amount to drop
    --                  int inv_pos: The inventory slot position
    -- RETURNS: 	List<Pair<string, string>> _drop_item_message: The DropItem network message 
    -- NOTES:
    -- Creates the DropItem network message
    ---------------------------------------------------------------------------------------------*/
    public List<Pair<string, string>> CreateDropItemNetworkMessage(int item_id, int amt, int inv_pos)
    {
        List<Pair<string, string>> _drop_item_message = new List<Pair<string, string>>();

        Vector3 position = GameManager.instance.player.transform.position;
        int _world_item_id = GenerateWorldItemId();
        int _player_id = GameData.MyPlayer.PlayerID;

        _drop_item_message.Add(new Pair<string, string>("WorldItemId", _world_item_id.ToString()));
        _drop_item_message.Add(new Pair<string, string>("PlayerId", _player_id.ToString()));
        _drop_item_message.Add(new Pair<string, string>("ItemId", item_id.ToString()));
        _drop_item_message.Add(new Pair<string, string>("Amount", amt.ToString()));
        _drop_item_message.Add(new Pair<string, string>("InvPos", inv_pos.ToString()));
        _drop_item_message.Add(new Pair<string, string>("PosX", ((int)position.x).ToString()));
        _drop_item_message.Add(new Pair<string, string>("PosY", ((int)position.y).ToString()));

        return _drop_item_message;
    }

    /*---------------------------------------------------------------------------------------------
    -- FUNCTION: 	ConvertListToJSONClass
    -- DATE: 		15/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public JSONClass ConvertListToJSONClass(List<Pair<string, string>> list)
    --                  List<Pair<string, string>> list: The list to be converted to JSONClass
    -- RETURNS: 	JSONClass JsonClass: The converted JSONClass 
    -- NOTES:
    -- Converts a List of Pairs to JSONClass format which is the same as the message 
    -- format of network messages.
    ---------------------------------------------------------------------------------------------*/
    public JSONClass ConvertListToJSONClass(List<Pair<string, string>> list)
    {
        JSONClass JsonClass = new JSONClass();
        foreach (Pair<string, string> obj in list)
        {
            JsonClass[obj.first] = obj.second;
        }
        return JsonClass;
    }


    /*------------------------------------------------------------------------------------------------
    -- FUNCTION: 	CreatePickupItemNetworkMessage
    -- DATE: 		15/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public List<Pair<string, string>> CreatePickupItemNetworkMessage(int world_item_id,
    --                  int player_id, int item_id, int amt)
    --                  int world_item_id: The world item id
    --                  int player_id: The player id
    --                  int item_id: the item id
    --                  int amt: The amount to pickup
    -- RETURNS: 	 List<Pair<string, string>> _pickup_item_message: The PickupItem network message 
    -- NOTES:
    -- Creates the Pickup item network message
    ---------------------------------------------------------------------------------------------------*/
    public List<Pair<string, string>> CreatePickupItemNetworkMessage(int world_item_id, 
        int player_id, int item_id, int amt)
    {
        List<Pair<string, string>> _pickup_item_message = new List<Pair<string, string>>();

        _pickup_item_message.Add(new Pair<string, string>("WorldItemId", world_item_id.ToString()));
        _pickup_item_message.Add(new Pair<string, string>("PlayerId", player_id.ToString()));
        _pickup_item_message.Add(new Pair<string, string>("ItemId", item_id.ToString()));
        _pickup_item_message.Add(new Pair<string, string>("Amount", amt.ToString()));

        return _pickup_item_message;
    }

    /*------------------------------------------------------------------------------------------------
    -- FUNCTION: 	CreateWorldItemNetworkMessage
    -- DATE: 		04/04/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public List<Pair<string, string>> CreateWorldItemNetworkMessage(int world_item_id, 
    --                  int item_id, int amt, float pos_x, float pos_y)
    --                  int world_item_id: The world item id
    --                  int item_id: the item id
    --                  int amt: The amount to create
    --                  float pos_x: The x coordinate on the map
    --                  float pos_y: The y coordinate on the map
    -- RETURNS: 	 List<Pair<string, string>> _create_item_message: The createWorldItem network message 
    -- NOTES:
    -- Creates the createWorldItem network message
    ---------------------------------------------------------------------------------------------------*/
    public List<Pair<string, string>> CreateWorldItemNetworkMessage(int world_item_id, int item_id,
        int amt, float pos_x, float pos_y)
    {
        List<Pair<string, string>> _create_item_message = new List<Pair<string, string>>();

        _create_item_message.Add(new Pair<string, string>("WorldItemId", world_item_id.ToString()));
        _create_item_message.Add(new Pair<string, string>("ItemId", item_id.ToString()));
        _create_item_message.Add(new Pair<string, string>("Amount", amt.ToString()));
        _create_item_message.Add(new Pair<string, string>("PosX", pos_x.ToString()));
        _create_item_message.Add(new Pair<string, string>("PosY", pos_y.ToString()));

        return _create_item_message;
    }
}
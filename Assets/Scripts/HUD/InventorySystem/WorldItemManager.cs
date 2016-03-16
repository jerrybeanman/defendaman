using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

enum ItemUpdate { Pickup = 1, Drop = 2 }
/*-----------------------------------------------------------------------------
-- WorldItemManager.cs - Script attached to GameManager game object
--                       responsible for managing world items.
--
-- FUNCTIONS:
--		void Start()
--		public void CreateWorldItem(string world_item_id, int item_id, int amt,
--          Vector3 pos)
--		public void ProcessPickUpEvent(string world_item_id, int player_id, 
--          int item_id, int amt)
--      public void ProcessDropEvent(string world_item_id, int player_id, 
--          int item_id, int amt, int inv_pos, Vector3 position)
--      public void DestroyWorldItem(string world_item_id)
--
-- DATE:		05/03/2016
-- REVISIONS:	(V1.0)
-- DESIGNER:	Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang
-----------------------------------------------------------------------------*/
public class WorldItemManager : MonoBehaviour
{
    private int _dropped_item_instance_id;
    ItemManager _item_manager;
    public GameObject world_item;
    Inventory _inventory;
    int _my_player_id;

    /*
     * Retrives the ItemManager script, the Inventory script and the player id
     */
    void Start()
    {
        NetworkingManager.Subscribe(ReceiveItemPickupPacket, DataType.Item, (int)ItemUpdate.Pickup);
        NetworkingManager.Subscribe(ReceiveItemDropPacket, DataType.Item, (int)ItemUpdate.Drop);

        _item_manager = GetComponent<ItemManager>();
        _inventory = GameObject.Find("Inventory").GetComponent<Inventory>();
        _my_player_id = GameData.MyPlayer.PlayerID;

        // Adding initial world items (testing)
        CreateWorldItem(101, 1, 1, 35, 25);
        CreateWorldItem(102, 2, 1, 36, 30);
        CreateWorldItem(103, 2, 100, 30, 20);
        CreateWorldItem(104, 0, 1, 36, 20);
    }

    /*
     * Creates a world item
     */
    public void CreateWorldItem(int world_item_id, int item_id, int amt, int pos_x, int pos_y)
    {
        Item item = _item_manager.FetchItemById(item_id);
        GameObject _item = Instantiate(world_item);
        _item.GetComponent<WorldItemData>().world_item_id = world_item_id;
        _item.GetComponent<WorldItemData>().item = item;
        _item.GetComponent<WorldItemData>().amount = amt;
        _item.GetComponent<SpriteRenderer>().sprite = item.world_sprite;
        _item.transform.position = new Vector3(pos_x, pos_y, -5);
    }


    public void ReceiveItemPickupPacket(JSONClass itemPacket)
    {
        ProcessPickUpEvent(itemPacket["WorldItemId"].AsInt, itemPacket["PlayerId"].AsInt,
            itemPacket["ItemId"].AsInt, itemPacket["Amount"].AsInt);
    }

    public void ReceiveItemDropPacket(JSONClass itemPacket)
    {
        ProcessDropEvent(itemPacket["WorldItemId"].AsInt, itemPacket["PlayerId"].AsInt,
            itemPacket["ItemId"].AsInt, itemPacket["Amount"].AsInt, itemPacket["InvPos"].AsInt,
            itemPacket["PosX"].AsInt, itemPacket["PosY"].AsInt);
    }

    /*
     * Processes a pick up message from the server.
     * Adds item to the player's inventory if the player id matches the player id in the message.
     * Removes item from the world.
     */
    public void ProcessPickUpEvent(int world_item_id, int player_id, int item_id, int amt)
    {
        // if (_my_player_id == player_id) // Disabled for testing, the player ID is set later
        if (GameData.MyPlayer.PlayerID == player_id)
        {
            _inventory.AddItem(item_id, amt);
        }
        DestroyWorldItem(world_item_id);
    }

    /*
     * Processes an item drop message from the server.
     * Removes item to the player's inventory if the player id matches the player id in the message.
     * Adds item to the world.
     */
    public void ProcessDropEvent(int world_item_id, int player_id, int item_id,
                                 int amt, int inv_pos, int pos_x, int pos_y)
    {
        // if (_my_player_id == player_id) // Disabled for testing, the player ID is set later
        if (GameData.MyPlayer.PlayerID == player_id)
        {
            CreateWorldItem(world_item_id, item_id, amt, pos_x, pos_y);
        }
        _inventory.DestroyInventoryItem(inv_pos, amt);
    }

    /*
     * Find the world item with the matching world_item_id and destroys it
     */
    public void DestroyWorldItem(int world_item_id)
    {
        GameObject[] _world_items = GameObject.FindGameObjectsWithTag("WorldItem");
        foreach (GameObject _world_item in _world_items)
        {
            if (_world_item.GetComponent<WorldItemData>().world_item_id == world_item_id)
            {
                Destroy(_world_item);
                break;
            }
        }
    }

    public void UseItem()
    {

    }

    public int GenerateWorldItemId()
    {
        //return _my_player_id * 1000000 + _dropped_item_instance_id++; // Disabled for testing, the player ID is set later
        return GameData.MyPlayer.PlayerID * 1000000 + _dropped_item_instance_id++;
    }

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

    // For testing, packets from server arrive as JSONClass type
    public JSONClass ConvertListToJSONClass(List<Pair<string, string>> list)
    {
        JSONClass JsonClass = new JSONClass();
        foreach (Pair<string, string> obj in list)
        {
            JsonClass[obj.first] = obj.second;
        }
        return JsonClass;
    }


    //int world_item_id, int player_id, int item_id, int amt
    public List<Pair<string, string>> CreatePickupItemNetworkMessage(int world_item_id, int item_id, int amt)
    {
        List<Pair<string, string>> _pickup_item_message = new List<Pair<string, string>>();
        int _player_id = GameData.MyPlayer.PlayerID;

        _pickup_item_message.Add(new Pair<string, string>("WorldItemId", world_item_id.ToString()));
        _pickup_item_message.Add(new Pair<string, string>("PlayerId", _player_id.ToString()));
        _pickup_item_message.Add(new Pair<string, string>("ItemId", item_id.ToString()));
        _pickup_item_message.Add(new Pair<string, string>("Amount", amt.ToString()));

        return _pickup_item_message;
    }
}
using UnityEngine;
using System.Collections;

// public enum ItemUpdate { Pickup = 1, Drop = 2 }
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
    void Start ()
    {
        //NetworkingManager.Subscribe(ReceiveItemPickupPacket, DataType.Item, (int)ItemUpdate.Pickup);
        _item_manager = GetComponent<ItemManager>();
        _inventory = GameObject.Find("Inventory").GetComponent<Inventory>();
        _my_player_id = GameData.MyPlayerID;

        // Adding initial world items (testing)
        CreateWorldItem(100, 0, 1, 30, 30);
        CreateWorldItem(101, 1, 1, 35, 25);
        CreateWorldItem(102, 0, 1, 36, 30);
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
        _item.transform.position = new Vector3(pos_x, pos_y, -5);       
    }

    /*
    void ReceiveItemPickupPacket(JSONClass itemPacket)
    {
        ProcessPickUpEvent(itemPacket["WorldItemID"].AsInt, itemPacket["PlayerID"].AsInt, itemPacket["ItemID"].AsInt, itemPacket["Amount"].AsInt);
    }*/

    /*
     * Processes a pick up message from the server.
     * Adds item to the player's inventory if the player id matches the player id in the message.
     * Removes item from the world.
     */
    public void ProcessPickUpEvent(int world_item_id, int player_id, int item_id, int amt)
    {
        if (_my_player_id == player_id)
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
        if (_my_player_id == player_id)
        {
            CreateWorldItem(world_item_id, item_id, amt, pos_x, pos_y);
        }
        _inventory.DestroyInventoryItem(inv_pos);
        _inventory.inventory_item_list[inv_pos] = new Item();
        
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

    public int GenerateWorldItemId()
    {
        return _my_player_id * 1000000 + _dropped_item_instance_id++;
    }
}
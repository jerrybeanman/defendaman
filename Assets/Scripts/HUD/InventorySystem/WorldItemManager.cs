using UnityEngine;
using System.Collections;

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
    ItemManager _item_manager;
    public GameObject world_item;
    Inventory _inventory;
    int _my_player_id;

    /*
     * Retrives the ItemManager script, the Inventory script and the player id
     */
    void Start ()
    {
        _item_manager = GetComponent<ItemManager>();
        _inventory = GameObject.Find("Inventory").GetComponent<Inventory>();
        _my_player_id = GameData.MyPlayerID;

        // Adding initial world items (testing)
        CreateWorldItem("test", 0, 1, new Vector3(55, 55, 0));
        CreateWorldItem("test1", 1, 1, new Vector3(60, 55, 0));
        CreateWorldItem("test2", 0, 1, new Vector3(55, 60, 0));
    }

    /*
     * Creates a world item
     */
    public void CreateWorldItem(string world_item_id, int item_id, int amt, Vector3 pos)
    {
        Item item = _item_manager.FetchItemById(item_id);
        GameObject _item = Instantiate(world_item);
        _item.GetComponent<WorldItemData>().world_item_id = world_item_id;
        _item.GetComponent<WorldItemData>().item = item;
        _item.GetComponent<WorldItemData>().amount = amt;
        _item.transform.position = pos;       
    }

    /*
     * Processes a pick up message from the server.
     * Adds item to the player's inventory if the player id matches the player id in the message.
     * Removes item from the world.
     */
    public void ProcessPickUpEvent(string world_item_id, int player_id, int item_id, int amt)
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
    public void ProcessDropEvent(string world_item_id, int player_id, int item_id, int amt, int inv_pos, Vector3 position)
    {
        if (_my_player_id == player_id)
        {
            CreateWorldItem(world_item_id, item_id, amt, position);
        }
        _inventory.DestroyInventoryItem(inv_pos);
        _inventory.inventory_item_list[inv_pos] = new Item();
        
    }

    /*
     * Find the world item with the matching world_item_id and destroys it
     */
    public void DestroyWorldItem(string world_item_id)
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
}
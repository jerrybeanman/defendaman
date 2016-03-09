using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

static class Constants
{
    public const int SLOT_AMOUNT = 4;
}

/*-----------------------------------------------------------------------------
-- Inventory.cs - Script attached to the Inventory game object that stores a 
--                list of Slot game objects and Item game objects. 
--                Responsible for managing the weapons/items of a player
--
-- FUNCTIONS:
--		void Start()
--		public void AddItem(int id)
--		int check_if_item_in_inventory(Item item)
--      public void DestroyInventoryItem(int inv_pos)
--
-- DATE:		17/02/2016
-- REVISIONS:	(V1.0)
-- DESIGNER:	Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang
-----------------------------------------------------------------------------*/
public class Inventory : MonoBehaviour
{
    GameObject _inventory_panel;
    GameObject _slot_panel;
    ItemManager _item_manager;
    public GameObject inventory_slot;
    public GameObject inventory_item;

    public List<Item> inventory_item_list = new List<Item>();
    public List<GameObject> slot_list = new List<GameObject>();

    /*
     * Initializes empty inventory slot containing empty item objects
     */
    void Start()
    {
        _item_manager = GetComponent<ItemManager>();
        _inventory_panel = GameObject.Find("Inventory Panel");
        _slot_panel = _inventory_panel.transform.FindChild("Slot Panel").gameObject;
        for (int i = 0; i < Constants.SLOT_AMOUNT; i++)
        {
            inventory_item_list.Add(new Item());
            slot_list.Add(Instantiate(inventory_slot));
            slot_list[i].GetComponent<InventorySlot>().slot_pos = i;
            slot_list[i].transform.SetParent(_slot_panel.transform);
            // Sets scaling factor of gridlayout component to 1 to work with
            // the unity "Scale With Screen Size" ui scale mode
            slot_list[i].transform.localScale = new Vector3(1, 1, 1);
        }
        // Adding initial items to the inventory (testing)
        AddItem(0);
        AddItem(1);
        AddItem(1);
    }


    /* 
     * Takes an item id and the amount and adds an Item object into an open slot in the inventory or stacks 
     * it on a slot containing the same item.
     * Updates the attributes of the Item: item, amount, and item_pos  
     */
    public void AddItem(int id, int amt = 1)
    {
        Item _item_to_add = _item_manager.FetchItemById(id);
        int item_idx;
        if (_item_to_add.stackable && (item_idx = check_if_item_in_inventory(_item_to_add)) != -1)
        {
            ItemData data = slot_list[item_idx].transform.GetChild(0).GetComponent<ItemData>();
            data.amount += amt;
            data.transform.GetChild(0).GetComponent<Text>().text = data.amount.ToString();
        }
        else
        {
            for (int i = 0; i < inventory_item_list.Count; i++)
            {
                if (inventory_item_list[i].id == -1)
                {
                    inventory_item_list[i] = _item_to_add;
                    GameObject _item_obj = Instantiate(inventory_item);
                    ItemData data = _item_obj.GetComponent<ItemData>();
                    data.item = _item_to_add;
                    data.item_pos = i;
                    data.amount += amt;
                    if (_item_to_add.stackable)
                        data.transform.GetChild(0).GetComponent<Text>().text = data.amount.ToString();
                    _item_obj.transform.SetParent(slot_list[i].transform);
                    // Sets scaling factor of gridlayout component to 1 to work with
                    // the unity "Scale With Screen Size" ui scale mode
                    _item_obj.transform.localScale = new Vector3(1, 1, 1);
                    _item_obj.transform.localPosition = Vector2.zero; //centers item relative to parent
                    _item_obj.GetComponent<Image>().sprite = _item_to_add.sprite;
                    _item_obj.name = _item_to_add.title; //name shown in the inspector
                    break;
                }
            }
        }
    }

    /* 
     * Destroys the item in the specified inventory slot position
     */
    public void DestroyInventoryItem(int inv_pos)
    {
        GameObject[] _inventory_items = GameObject.FindGameObjectsWithTag("InventoryItem");
        foreach (GameObject _inventory_item in _inventory_items)
        {
            if (_inventory_item.GetComponent<ItemData>().item_pos == inv_pos)
            {
                Destroy(_inventory_item);
                break;
            }
        }
    }

    /* 
     * Checks whether the item being added is already present in the inventory.
     * Used to see if an item should be stacked or placed on a new slot when being added
     */
    int check_if_item_in_inventory(Item item)
    {
        for (int i = 0; i < inventory_item_list.Count; i++)
        {
            if (inventory_item_list[i].id == item.id)
            {
                return i;
            }
        }
        return -1;
    }
}
